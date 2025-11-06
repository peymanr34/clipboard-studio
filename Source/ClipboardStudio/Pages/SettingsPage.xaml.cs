using ClipboardStudio.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

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
    }
}
