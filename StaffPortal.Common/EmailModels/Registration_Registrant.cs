namespace StaffPortal.Common.EmailModels
{
    public class Registration_Registrant : EmailModelBase
    {
        public string BusinessRoleName { get; set; }

        public Registration_Registrant()
        {
            this.Subject = $"Staff Portal - Registration Confirmation";
            this.Template_FileName = GlobalConstants.EMAILTEMPLATES_REGISTRATION_REGISTRANT;
        }

        public Registration_Registrant(string firstName, string lastName, string to)
            : base(firstName, lastName, to)
        {
            this.Subject = $"Staff Portal - Registration Confirmation";
            this.Template_FileName = GlobalConstants.EMAILTEMPLATES_REGISTRATION_REGISTRANT;
        }
    }
}