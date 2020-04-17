using System.Collections.Generic;

namespace StaffPortal.Web.Models
{
    public class BusinessRoleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentBusinessRoleId { get; set; }
        public IList<PermissionModel> Permissions { get; set; }
    }
}
