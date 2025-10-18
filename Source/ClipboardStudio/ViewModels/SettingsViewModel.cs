using System;
using System.Threading.Tasks;
using MvvmGen;
using Windows.ApplicationModel;

namespace ClipboardStudio.ViewModels
{
    [ViewModel]
    public partial class SettingsViewModel
    {
        [Property]
        private StartupTask _startupTask;

        [Property]
        [PropertyCallMethod(nameof(StartupChanged))]
        private bool _startupEnabled;

        [Property]
        private bool _canToggleStartup;

        [Property]
        [PropertyCallMethod(nameof(SaveSettings))]
        private bool _trayIconEnabled;

        [Property]
        [PropertyCallMethod(nameof(SaveSettings))]
        private bool _closeToTrayEnabled;

        public async void Load()
        {
            StartupTask = await StartupTask.GetAsync("ClipboardStudioStartupTask");
            SetStartupState(StartupTask.State);

            LoadSettings();
        }

        private async Task StartupChanged()
        {
            if (!CanToggleStartup)
            {
                return;
            }

            if (StartupEnabled)
            {
                await StartupTask.RequestEnableAsync();
                return;
            }

            StartupTask.Disable();
        }

        private void SetStartupState(StartupTaskState state)
        {
            // Set via the fields to prevent executing the 'StartupChanged' method.
            _startupEnabled = state is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
            _canToggleStartup = state is StartupTaskState.Enabled or StartupTaskState.Disabled;

            OnPropertyChanged(nameof(StartupEnabled));
            OnPropertyChanged(nameof(CanToggleStartup));
        }

        private void LoadSettings()
        {
            // Set via the fields to avoid invoking the save method.
            _trayIconEnabled = App.Settings.GetValue(SettingsKeys.TrayIconEnabled, false);
            _closeToTrayEnabled = App.Settings.GetValue(SettingsKeys.CloseToTrayEnabled, false);

            OnPropertyChanged(nameof(TrayIconEnabled));
            OnPropertyChanged(nameof(CloseToTrayEnabled));
        }

        private void SaveSettings()
        {
            App.Settings.SetValue(SettingsKeys.TrayIconEnabled, TrayIconEnabled);
            App.Settings.SetValue(SettingsKeys.CloseToTrayEnabled, CloseToTrayEnabled);

            App.TrayIcon.IsVisible = TrayIconEnabled;
        }
    }
}
