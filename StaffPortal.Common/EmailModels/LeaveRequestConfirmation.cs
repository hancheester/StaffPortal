namespace StaffPortal.Common.EmailModels
{
    public class LeaveRequestConfirmation : EmailModelBase
    {
        public string BusinessRoleName { get; set; }
        public string LeaveTypeName { get; set; }
        public string[] RequestedDates { get; set; }
        
        public LeaveRequestConfirmation()
        {
            this.Template_FileName = GlobalConstants.EMAILTEMPLATES_LEAVEREQUEST;
        }

        public LeaveRequestConfirmation(string firstName, string lastName, string to)
            : base(firstName, lastName, to)
        {
            this.Subject = $"Staff Portal - Leave Request Confirmation as {this.BusinessRoleName}";
            this.Template_FileName = GlobalConstants.EMAILTEMPLATES_LEAVEREQUEST;
        }
    }
}