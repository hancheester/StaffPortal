using System;

namespace StaffPortal.Web.Models
{
    public class RotaRequestViewModel
    {
        public DateTime FromDate { get; set; }
        public int DepartmentId { get; set; }
        public int BusinessRoleId { get; set; }
        public int? EmployeeId { get; set; }
    }
}
