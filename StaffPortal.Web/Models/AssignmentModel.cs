using System;

namespace StaffPortal.Web.Models
{
    public class AssignmentModel
    {
        public int EmployeeId { get; set; }
        public int BusinessRoleId { get; set; }
        public int DepartmentId { get; set; }
        public string Day { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
