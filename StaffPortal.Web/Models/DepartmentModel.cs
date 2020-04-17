using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StaffPortal.Web.Models
{
    public class DepartmentModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int MinimumRequired { get; set; }
        public IList<DepartmentBusinessRoleModel> Roles { get; set; }
        public IList<DepartmentOpeningHour> OpeningHours { get; set; }
    }

    public class DepartmentBusinessRoleModel
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public bool ShowOnRota { get; set; }
        public int MinimumRequired { get; set; }        
    }

    public class DepartmentOpeningHour
    {
        public string Day { get; set; }
        public bool IsOpen { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
    }
}
