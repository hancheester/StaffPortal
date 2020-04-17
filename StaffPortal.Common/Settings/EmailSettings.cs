namespace StaffPortal.Common.Settings
{
    public class EmailSettings : ISettings
    {
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DisplayEmail { get; set; }
        public string FromEmail { get; set; }
    }
}
