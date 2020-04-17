using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StaffPortal.Common
{
    public class BusinessRole : BaseEntity
    {
        public int ParentBusinessRoleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [NotMapped]
        public IList<BusinessRole> Children { get; set; }
        [NotMapped]
        public IList<Permission> Permissions { get; set; }
    }
}