using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ClipboardStudio.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using MvvmGen;
using Windows.ApplicationModel.DataTransfer;

namespace ClipboardStudio.ViewModels
{
    [ViewModel]
    [Inject(typeof(AppNotificationManager), PropertyName = "NotificationManager")]
    [Inject(typeof(DatabaseContext), PropertyName = "Context")]
    public partial class MainViewModel
    {
        partial void OnInitialize()
        {
            LoadSettings();
        }

        [Property]
        private ObservableCollection<EntryViewModel> _items;

        [Property]
        private EntryViewModel _selectedItem;

        [Property]
        [PropertyCallMethod(nameof(SaveSettings))]
        private bool _captureAllowed;

        [Property]
        [PropertyCallMethod(nameof(SaveSettings))]
        private bool _notificationAllowed;

        [Command(CanExecuteMethod = nameof(CanRemove))]
        public void Remove()
        {
            var item = Context.Entries
                .First(x => x.Id == SelectedItem.Id);

            Context.Entries.Remove(item);
            Context.SaveChanges();

            Items.Remove(SelectedItem);
            OnPropertyChanged(nameof(Items));
        }

        [CommandInvalidate(nameof(SelectedItem))]
        public bool CanRemove()
        {
            return SelectedItem is not null;
        }

        [Command(CanExecuteMethod = nameof(CanCopy))]
        public void Copy()
        {
            var package = new DataPackage();
            package.SetText(SelectedItem.Text);

            Clipboard.SetContent(package);
            ShowNotificationOrDefault("Copied to Clipboard", SelectedItem.Text);
        }

        [CommandInvalidate(nameof(SelectedItem))]
        public bool CanCopy()
        {
            return SelectedItem is not null;
        }

        [Command(CanExecuteMethod = nameof(CanClear))]
        public void Clear()
        {
            Context.Entries.ExecuteDelete();
            Items.Clear();

            OnPropertyChanged(nameof(Items));
        }

        [CommandInvalidate(nameof(Items))]
        public bool CanClear()
        {
            return Items.Any();
        }

        [Command]
        public async Task<bool> Paste()
        {
            var package = Clipboard.GetContent();
            var result = await Context.AddOrUpdateEntryAsync(package);

            if (result is null)
            {
                return false;
            }

            Load();
            return true;
        }

        public void Load(string search = null)
        {
            var query = Context.Entries
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => EF.Functions.Like(x.Text, $"%{search}%"));
            }
            else
            {
                query = query.OrderByDescending(x => x.ModifyDateUtc);
            }

            var items = query
                .Select(x => new EntryViewModel
                {
                    Id = x.Id,
                    Text = x.Text,
                    Title = x.GetTitle(),
                }).ToList();

            Items ??= [];
            Items.Clear();

            foreach (var item in items)
            {
                Items.Add(item);
            }

            SelectedItem ??= Items.FirstOrDefault();
            OnPropertyChanged(nameof(Items));
        }

        public void ShowNotificationOrDefault(string title, string content)
        {
            if (!NotificationAllowed)
            {
                return;
            }

            var builder = new AppNotificationBuilder()
                .SetAttributionText(title)
                .AddText(content)
                .MuteAudio();

            var result = builder.BuildNotification();

            result.ExpiresOnReboot = true;
            result.Expiration = DateTime.Now.AddSeconds(5);

            NotificationManager.Show(result);
        }

        private void LoadSettings()
        {
            // Set via the fields to avoid invoking the save method.
            _captureAllowed = App.Settings.GetValue(SettingsKeys.CaptureAllowed, false);
            _notificationAllowed = App.Settings.GetValue(SettingsKeys.NotificationAllowed, false);
        }

        private void SaveSettings()
        {
            App.Settings.SetValue(SettingsKeys.CaptureAllowed, CaptureAllowed);
            App.Settings.SetValue(SettingsKeys.NotificationAllowed, NotificationAllowed);
        }
    }
}
