using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClipboardStudio.Pages;
using ClipboardStudio.ViewModels;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppNotifications;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using WinRT.Interop;

namespace ClipboardStudio
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeSystemBackdrop();

            Title = "Clipboard Studio";
            ExtendsContentIntoTitleBar = true;

            SetWindowSize();
            AppWindow.SetIcon("Assets\\icon.ico");

            ViewModel = new MainViewModel(App.Context, AppNotificationManager.Default);
            ViewModel.Load();

            Clipboard.ContentChanged += Clipboard_ContentChanged;
        }

        public MainViewModel ViewModel { get; private set; }

        private void InitializeSystemBackdrop()
        {
            if (MicaController.IsSupported())
            {
                SystemBackdrop = new MicaBackdrop()
                {
                    Kind = MicaKind.BaseAlt,
                };
            }
            else if (DesktopAcrylicController.IsSupported())
            {
                SystemBackdrop = new DesktopAcrylicBackdrop();
            }
        }

        private void SetWindowSize()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var dpi = NativeMethods.GetDpiForWindow(hwnd);

            var scale = dpi / 96d;

            AppWindow.Resize(new SizeInt32
            {
                Height = (int)(400 * scale),
                Width = (int)(700 * scale),
            });
        }

        private async void Clipboard_ContentChanged(object sender, object e)
        {
            if (ViewModel.CaptureAllowed)
            {
                ViewModel.PasteCommand.Execute(null);

                var hwnd = WindowNative.GetWindowHandle(this);

                NativeMethods.FlashWindowEx(hwnd);
                await Task.Delay(1500);
                NativeMethods.FlashWindowEx(hwnd, false);
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.Load(sender.Text);
            }
        }

        private async void About_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "About",
                CloseButtonText = "OK",
                Content = new AboutPage(),
                XamlRoot = Content.XamlRoot,
            };

            await dialog.ShowAsync();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();

            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedFileName = "list";
            savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            savePicker.FileTypeChoices.Add("Text File", new List<string>() { ".txt" });

            var file = await savePicker.PickSaveFileAsync();

            if (file is null)
            {
                return;
            }

            CachedFileManager.DeferUpdates(file);

            var items = ViewModel.Items.Select(x => x.Text);
            await FileIO.WriteLinesAsync(file, items);

            var status = await CachedFileManager.CompleteUpdatesAsync(file);

            if (status == FileUpdateStatus.Complete ||
                status == FileUpdateStatus.CompleteAndRenamed)
            {
                ViewModel.ShowNotificationOrDefault("File has been saved.", file.Name);
                return;
            }

            ViewModel.ShowNotificationOrDefault("File has not been saved.", file.Name);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            clearFlyout.Hide();
        }
    }
}
