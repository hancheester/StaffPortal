using Microsoft.Extensions.Caching.Memory;
using StaffPortal.Common.Settings;
using StaffPortal.Data;
using StaffPortal.Service.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace StaffPortal.Service.Configuration
{
    public class SettingService : ISettingService
    {
        private readonly IRepository<Setting> _settingRepository;
        private readonly IMemoryCache _memoryCache;

        public SettingService(
            IRepository<Setting> settingRepository,
            IMemoryCache memoryCache)
        {
            _settingRepository = settingRepository;
            _memoryCache = memoryCache;
        }

        public void DeleteSetting<T>() where T : ISettings, new()
        {
            var settingsToDelete = new List<Setting>();
            var allSettings = GetAllSettings();

            foreach (var prop in typeof(T).GetProperties())
            {
                var key = typeof(T).Name + "." + prop.Name;
                settingsToDelete.AddRange(allSettings.Where(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)));
            }

            DeleteSettings(settingsToDelete);
        }

        public T LoadSetting<T>(int organizationId = 0) where T : ISettings, new()
        {
            var settings = Activator.CreateInstance<T>();

            foreach (var prop in typeof(T).GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                var key = typeof(T).Name + "." + prop.Name;

                var setting = GetSettingByKey<string>(key, organizationId: organizationId);

                if (setting == null)
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).IsValid(setting))
                    continue;

                object value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(setting);

                prop.SetValue(settings, value, null);
            }

            return settings;
        }

        public T GetSettingByKey<T>(string key, T defaultValue = default(T), int organizationId = 0)    
        {
            if (string.IsNullOrEmpty(key)) return defaultValue;

            var settings = GetAllSettingsCached();

            key = key.Trim().ToLowerInvariant();

            if (settings.ContainsKey(key))
            {
                var settingsByKey = settings[key];
                var setting = settingsByKey.FirstOrDefault(x => x.OrganizationId == organizationId);

                if (setting != null)
                    return To<T>(setting.Value);
            }

            return defaultValue;
        }

        public void SaveSetting<T>(T settings, int organizationId = 0) where T : ISettings, new()
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    continue;

                var key = typeof(T).Name + "." + prop.Name;
                var value = prop.GetValue(settings, null);

                if (value != null)
                    SetSetting(prop.PropertyType, key, value, organizationId, false);
                else
                    SetSetting(key, string.Empty, organizationId, false);                
            }

            _memoryCache.Remove("setting.all");
        }

        private void SetSetting<T>(string key, T value, int storeId = 0, bool clearCache = true)
        {
            SetSetting(typeof(T), key, value, storeId, clearCache);
        }

        private void SetSetting(Type type, string key, object value, int organizationId = 0, bool clearCache = true)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            key = key.Trim().ToLowerInvariant();

            var valueStr = TypeDescriptor.GetConverter(type).ConvertToInvariantString(value);
            var allSettings = GetAllSettingsCached();
            var settingForCaching = allSettings.ContainsKey(key) ? allSettings[key].FirstOrDefault(x => x.OrganizationId == organizationId) : null;

            if(settingForCaching != null)
            {
                var setting = GetSettingById(settingForCaching.Id);
                setting.Value = valueStr;
                UpdateSetting(setting, clearCache);
            }
            else
            {
                var setting = new Setting
                {
                    Name = key,
                    Value = valueStr,
                    OrganizationId = organizationId
                };

                InsertSetting(setting, clearCache);
            }
        }

        private void InsertSetting(Setting setting, bool clearCache)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            _settingRepository.Create(setting);

            _memoryCache.Remove("setting.all");
        }

        private void UpdateSetting(Setting setting, bool clearCache = true)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            _settingRepository.Update(setting);

            _memoryCache.Remove("setting.all");
        }

        private Setting GetSettingById(int id)
        {
            if (id == 0) return null;
            return _settingRepository.Return(id);
        }

        private void DeleteSettings(IList<Setting> settings)
        {
            if (settings == null) throw new ArgumentException(nameof(settings));

            _settingRepository.Delete(settings);

            _memoryCache.Remove("setting.all");
        }

        private IList<Setting> GetAllSettings()
        {
            return _settingRepository.Table
                .OrderBy(x => x.Name)
                .ThenBy(x => x.OrganizationId)
                .ToList();
        }

        private IDictionary<string, IList<Setting>> GetAllSettingsCached()
        {
            string key = "setting.all";

            return _memoryCache.Get(key, () =>
            {
                var settings = _settingRepository.TableNoTracking
                    .OrderBy(x => x.Name)
                    .ThenBy(x => x.OrganizationId)
                    .ToList();

                var dictionary = new Dictionary<string, IList<Setting>>();

                foreach (var setting in settings)
                {
                    var resourceName = setting.Name.ToLowerInvariant();

                    if (!dictionary.ContainsKey(resourceName))
                    {
                        dictionary.Add(resourceName, new List<Setting>
                        {
                            setting
                        });
                    }
                    else
                    {
                        dictionary[resourceName].Add(setting);
                    }
                }

                return dictionary;
            });
        }

        private T To<T>(object value)
        {
            return (T)To(value, typeof(T));
        }

        private object To(object value, Type destinationType)
        {
            return To(value, destinationType, CultureInfo.InvariantCulture);
        }

        private object To(object value, Type destinationType, CultureInfo culture)
        {
            if (value != null)
            {
                var sourceType = value.GetType();

                var destinationConverter = TypeDescriptor.GetConverter(destinationType);
                var sourceConverter = TypeDescriptor.GetConverter(sourceType);

                if (destinationConverter != null && destinationConverter.CanConvertFrom(value.GetType()))
                    return destinationConverter.ConvertFrom(null, culture, value);

                if (sourceConverter != null && sourceConverter.CanConvertTo(destinationType))
                    return sourceConverter.ConvertTo(null, culture, value, destinationType);

                if (destinationType.IsEnum && value is int)
                    return Enum.ToObject(destinationType, (int)value);

                if (!destinationType.IsInstanceOfType(value))
                    return Convert.ChangeType(value, destinationType, culture);
            }

            return value;
        }
    }
}
