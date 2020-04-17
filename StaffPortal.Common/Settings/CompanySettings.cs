namespace StaffPortal.Common.Settings
{
    public class CompanySettings : ISettings
    {
        public string CompanyName { get; set; }
        public string LogoPath { get; set; }
        public string ContactNumber { get; set; }        
        public string FinancialYearStart { get; set; }
        public string FinancialYearEnd { get; set; }
    }
}
