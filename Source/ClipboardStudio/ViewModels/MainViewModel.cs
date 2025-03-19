using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using ClipboardStudio.Data;
using ClipboardStudio.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using MvvmGen;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace ClipboardStudio.ViewModels
{
    [ViewModel]
    [Inject(typeof(AppNotificationManager), PropertyName = "NotificationManager")]
    [Inject(typeof(DatabaseContext), PropertyName = "Context")]
    public partial class MainViewModel
    {
        private ApplicationDataContainer _settings;

        partial void OnInitialize()
        {
            _settings = ApplicationData.Current.LocalSettings;
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
        public async void Paste()
        {
            var package = Clipboard.GetContent();

            if (package.Contains(StandardDataFormats.Text))
            {
                var text = await package.GetTextAsync();

                var entry = Context.Entries
                    .FirstOrDefault(x => EF.Functions.Collate(x.Text, "NOCASE") == text);

                if (entry is null)
                {
                    entry = new Entry
                    {
                        Text = text,
                    };

                    Context.Entries.Add(entry);
                    ShowNotificationOrDefault("Captured from Clipboard", text);
                }
                else
                {
                    entry.ModifyDateUtc = DateTime.UtcNow;
                    ShowNotificationOrDefault("Already Exists", text);
                }

                Context.SaveChanges();
                Load();
            }
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
            _captureAllowed = Convert.ToBoolean(_settings.Values[nameof(CaptureAllowed)]);
            _notificationAllowed = Convert.ToBoolean(_settings.Values[nameof(NotificationAllowed)]);

            Debug.WriteLine("Settings loaded.");
        }

        private void SaveSettings()
        {
            _settings.Values[nameof(CaptureAllowed)] = _captureAllowed;
            _settings.Values[nameof(NotificationAllowed)] = _notificationAllowed;

            Debug.WriteLine("Settings saved.");
        }
    }
}
