using System;
using ClipboardStudio.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Services.Store;
using WinRT.Interop;

namespace ClipboardStudio.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        public SettingsViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = new SettingsViewModel();
            ViewModel.Load();

            base.OnNavigatedTo(e);
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ImportFlyout.Hide();
        }

        private async void RateAndReview_Click(object sender, RoutedEventArgs e)
        {
            var context = StoreContext.GetDefault();

            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(context, hwnd);

            await context.RequestRateAndReviewAppAsync();
        }
    }
}
