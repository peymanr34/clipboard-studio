using System;
using System.Linq;
using System.Threading.Tasks;
using ClipboardStudio.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.Storage.Pickers;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
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
                if (await ViewModel.Paste())
                {
                    var hwnd = WindowNative.GetWindowHandle(App.MainWindow);

                    NativeMethods.FlashWindowEx(hwnd);
                    await Task.Delay(1500);
                    NativeMethods.FlashWindowEx(hwnd, false);
                }
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
            var savePicker = new FileSavePicker(App.MainWindow.AppWindow.Id)
            {
                SuggestedFileName = "list",
                SuggestedStartLocation = PickerLocationId.Desktop,
            };

            savePicker.FileTypeChoices.Add("Text File", [".txt"]);

            var file = await savePicker.PickSaveFileAsync();

            if (file is null)
            {
                return;
            }

            var storageFile = await StorageFile.GetFileFromPathAsync(file.Path);

            CachedFileManager.DeferUpdates(storageFile);

            var items = ViewModel.Items.Select(x => x.Text);
            await FileIO.WriteLinesAsync(storageFile, items);

            var status = await CachedFileManager.CompleteUpdatesAsync(storageFile);

            if (status == FileUpdateStatus.Complete ||
                status == FileUpdateStatus.CompleteAndRenamed)
            {
                ViewModel.ShowNotificationOrDefault("File has been saved.", storageFile.Name);
                return;
            }

            ViewModel.ShowNotificationOrDefault("File has not been saved.", storageFile.Name);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            clearFlyout.Hide();
        }
    }
}
