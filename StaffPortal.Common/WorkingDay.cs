using System;

namespace StaffPortal.Common
{
    public class WorkingDay : BaseEntity
    {
        public int EmployeeId { get; set; }
        public string Day { get; set; }
        public int? DepartmentId { get; set; }
        public bool IsAssigned { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}