using StaffPortal.Common;
using StaffPortal.Web.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StaffPortal.Web.Models
{
    public class DepartmentViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        [Required]
        public int MinimumRequired { get; set; }

        public IList<CheckableBusinessRole> BusinessRoles { get; set; }
        public IList<OpeningHour> OpeningHours { get; set; }
    }
}