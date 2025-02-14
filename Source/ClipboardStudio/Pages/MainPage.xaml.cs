using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClipboardStudio.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using WinRT.Interop;

namespace ClipboardStudio.Pages
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            ViewModel = new MainViewModel(App.Context, AppNotificationManager.Default);
            ViewModel.Load();

            Clipboard.ContentChanged += Clipboard_ContentChanged;
        }

        public MainViewModel ViewModel { get; private set; }

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
