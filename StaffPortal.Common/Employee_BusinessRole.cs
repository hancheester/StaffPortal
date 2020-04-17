namespace StaffPortal.Common
{
    public class Employee_BusinessRole : BaseEntity
    {
        public int EmployeeId { get; set; }
        public int BusinessRoleId { get; set; }
        public bool IsPrimary { get; set; }
    }
}