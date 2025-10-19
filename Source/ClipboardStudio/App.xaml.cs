using System;
using System.Linq;
using System.Threading.Tasks;
using ClipboardStudio.Data;
using ClipboardStudio.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using WinUIEx;

namespace ClipboardStudio
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        public static MainWindow MainWindow { get; private set; }

        public static DatabaseContext Context { get; private set; }

        public static SettingsProvider Settings { get; private set; }

        public static TrayIcon TrayIcon { get; private set; }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Context = await GetDatabaseContextAsync();

            var pending = await Context.Database.GetPendingMigrationsAsync();

            if (pending.Any())
            {
                await Context.Database.MigrateAsync();
            }

            Settings = new SettingsProvider(ApplicationData.Current.LocalSettings);

            MainWindow = new MainWindow();
            MainWindow.AppWindow.Closing += AppWindow_Closing;

            var launchToTray = LaunchedFromStartupTask() &&
                Settings.GetValue(SettingsKeys.LaunchToTrayEnabled, false);

            if (!launchToTray)
            {
                MainWindow.Activate();
            }

            TrayIcon = new TrayIcon(0, "Assets\\icon.ico", "Clipboard Studio");
            TrayIcon.Selected += TrayIcon_Selected;
            TrayIcon.ContextMenu += TrayIcon_ContextMenu;

            TrayIcon.IsVisible = Settings.GetValue(SettingsKeys.TrayIconEnabled, false);
        }

        private static async Task<DatabaseContext> GetDatabaseContextAsync()
        {
            var file = await ApplicationData.Current.LocalFolder
                .CreateFileAsync("data.db", CreationCollisionOption.OpenIfExists);

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlite($"Data Source={file.Path}");

            return new DatabaseContext(optionsBuilder.Options);
        }

        private static bool LaunchedFromStartupTask()
        {
            var args = Windows.ApplicationModel.AppInstance.GetActivatedEventArgs();
            return args?.Kind == Windows.ApplicationModel.Activation.ActivationKind.StartupTask;
        }

        private static void TrayIcon_ContextMenu(TrayIcon sender, TrayIconEventArgs e)
        {
            var flyout = new MenuFlyout();

            var show = new MenuFlyoutItem { Text = "Show" };
            show.Click += (s, e) =>
            {
                MainWindow.Activate();
            };

            var exit = new MenuFlyoutItem { Text = "Exit" };
            exit.Click += (s, e) =>
            {
                MainWindow.Close();
                TrayIcon.Dispose();
            };

            flyout.Items.Add(show);
            flyout.Items.Add(new MenuFlyoutSeparator());
            flyout.Items.Add(exit);

            e.Flyout = flyout;
        }

        private void TrayIcon_Selected(TrayIcon sender, TrayIconEventArgs e)
        {
            MainWindow.Activate();
        }

        private static void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs e)
        {
            if (TrayIcon.IsVisible && Settings.GetValue(SettingsKeys.CloseToTrayEnabled, false))
            {
                e.Cancel = true;
                sender.Hide();

                return;
            }

            TrayIcon.Dispose();
        }
    }
}
