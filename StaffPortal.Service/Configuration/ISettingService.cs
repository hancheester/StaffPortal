using StaffPortal.Common.Settings;

namespace StaffPortal.Service.Configuration
{
    public interface ISettingService
    {
        T LoadSetting<T>(int organizationId = 0) where T : ISettings, new();
        void SaveSetting<T>(T settings, int organizationId = 0) where T : ISettings, new();
        void DeleteSetting<T>() where T : ISettings, new();
    }
}
