namespace StaffPortal.Common.APIModels
{
    public class RequestedDateAPIModel : RequestedDate
    {
        public string DepartmentName { get; set; }
        public string RejectionReason { get; set; }
    }
}
