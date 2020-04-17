namespace StaffPortal.Common.EmailModels
{
    public class Registration_Admin : EmailModelBase
    {
        public string BusinessRoleName { get; set; }
        public string Applicant_FirstName { get; set; }
        public string Applicant_LastName { get; set; }

        public Registration_Admin()
        {
            this.Subject = $"Staff Portal - New Account Registration";
            this.Template_FileName = GlobalConstants.EMAILTEMPLATES_REGISTRATION_ADMIN;
        }

        public Registration_Admin(string firstName, string lastName, string to)
            : base(firstName, lastName, to)
        {
            this.Subject = $"Staff Portal - New Account Registration";
            this.Template_FileName = GlobalConstants.EMAILTEMPLATES_REGISTRATION_ADMIN;
        }
    }
}