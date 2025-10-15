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

        public async void Load()
        {
            StartupTask = await StartupTask.GetAsync("ClipboardStudioStartupTask");
            SetStartupState(StartupTask.State);
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
    }
}
