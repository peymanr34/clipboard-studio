using System.Linq;
using ClipboardStudio.Pages;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Graphics;
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

            var size = GetScaledWindowSize(400, 700);

            AppWindow.Resize(size);
            AppWindow.SetIcon("Assets\\icon.ico");

            contentFrame.Navigate(typeof(MainPage));
        }

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

        private SizeInt32 GetScaledWindowSize(int height, int width)
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var dpi = NativeMethods.GetDpiForWindow(hwnd);

            var scale = dpi / 96d;

            return new SizeInt32
            {
                Height = (int)(height * scale),
                Width = (int)(width * scale),
            };
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            var allowed = new[]
            {
                typeof(SettingsPage),
            };

            titleBar.IsBackButtonVisible = contentFrame.CanGoBack && allowed.Contains(e.SourcePageType);
        }

        private void TitleBar_BackRequested(TitleBar sender, object args)
        {
            if (contentFrame.CanGoBack)
            {
                contentFrame.GoBack();
            }
        }
    }
}
