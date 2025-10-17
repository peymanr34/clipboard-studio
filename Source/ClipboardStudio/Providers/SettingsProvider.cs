using Windows.Storage;

namespace ClipboardStudio.Providers
{
    public class SettingsProvider(ApplicationDataContainer settings)
    {
        private readonly ApplicationDataContainer _settings = settings;

        public void SetValue<T>(string key, T value)
        {
            _settings.Values[key] = value;
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            var value = _settings.Values[key];

            if (value is null)
            {
                return defaultValue;
            }

            return (T)value;
        }
    }
}
