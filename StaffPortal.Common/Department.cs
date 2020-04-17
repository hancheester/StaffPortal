using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StaffPortal.Common
{
    public class Department : BaseEntity
    {
        public string Name { get; set; }
        public int MinimumRequired { get; set; }
        [NotMapped]
        public IList<OpeningHour> OpeningHours { get; set; }        
        [NotMapped]
        public IList<BusinessRole_Department> DepartmentBusinessRoles { get; set; }
    }
}