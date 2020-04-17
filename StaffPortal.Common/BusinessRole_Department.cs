namespace StaffPortal.Common
{
    public class BusinessRole_Department : BaseEntity
    {
        public int BusinessRoleId { get; set; }
        public int DepartmentId { get; set; }
        public int MinimumRequired { get; set; }
    }
}