using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmGen;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;

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

        [Property]
        [PropertyCallMethod(nameof(SaveSettings))]
        private bool _launchToTrayEnabled;

        [Property]
        private bool _canImportHistory;

        [Property]
        private bool _importCompleted;

        [Command(CanExecuteMethod = nameof(CanImport))]
        public async Task Import()
        {
            var result = await Clipboard.GetHistoryItemsAsync();

            if (result.Status != ClipboardHistoryItemsResultStatus.Success)
            {
                return;
            }

            foreach (var item in result.Items.OrderBy(x => x.Timestamp))
            {
                await App.Context.AddOrUpdateEntryAsync(item.Content);
            }

            ImportCompleted = true;
        }

        [CommandInvalidate(nameof(ImportCompleted))]
        [CommandInvalidate(nameof(CanImportHistory))]
        public bool CanImport()
        {
            return CanImportHistory && !ImportCompleted;
        }

        public async void Load()
        {
            StartupTask = await StartupTask.GetAsync("ClipboardStudioStartupTask");
            SetStartupState(StartupTask.State);

            LoadSettings();
            LoadClipboardSettings();

            Clipboard.HistoryEnabledChanged += Clipboard_HistoryEnabledChanged;
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

        private void LoadClipboardSettings()
        {
            CanImportHistory = Clipboard.IsHistoryEnabled();
        }

        private void Clipboard_HistoryEnabledChanged(object sender, object e)
        {
            LoadClipboardSettings();
        }

        private void LoadSettings()
        {
            // Set via the fields to avoid invoking the save method.
            _trayIconEnabled = App.Settings.GetValue(SettingsKeys.TrayIconEnabled, false);
            _closeToTrayEnabled = App.Settings.GetValue(SettingsKeys.CloseToTrayEnabled, false);
            _launchToTrayEnabled = App.Settings.GetValue(SettingsKeys.LaunchToTrayEnabled, false);

            OnPropertyChanged(nameof(TrayIconEnabled));
            OnPropertyChanged(nameof(CloseToTrayEnabled));
            OnPropertyChanged(nameof(LaunchToTrayEnabled));
        }

        private void SaveSettings()
        {
            App.Settings.SetValue(SettingsKeys.TrayIconEnabled, TrayIconEnabled);
            App.Settings.SetValue(SettingsKeys.CloseToTrayEnabled, CloseToTrayEnabled);
            App.Settings.SetValue(SettingsKeys.LaunchToTrayEnabled, LaunchToTrayEnabled);

            App.TrayIcon.IsVisible = TrayIconEnabled;
        }
    }
}
