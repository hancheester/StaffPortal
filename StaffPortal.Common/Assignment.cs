using System;

namespace StaffPortal.Common
{
    public class Assignment : BaseEntity
    {
        public int EmployeeId { get; set; }
        public int BusinessRoleId { get; set; }
        public int DepartmentId { get; set; }
        public string Day { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public DateTime CreatedOnDate { get; set; }
    }
}