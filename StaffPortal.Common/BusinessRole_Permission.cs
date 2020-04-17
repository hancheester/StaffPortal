namespace StaffPortal.Common
{
    public class BusinessRole_Permission : BaseEntity
    {
        public int PermissionId { get; set; }
        public int BusinessRoleId { get; set; }      
    }
}