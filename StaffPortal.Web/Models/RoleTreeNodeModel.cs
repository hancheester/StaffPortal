using System.Collections.Generic;

namespace StaffPortal.Web.Models
{
    public class RoleTreeNodeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<RoleTreeNodeModel> Children { get; set; }
    }
}
