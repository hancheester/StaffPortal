using System;

namespace StaffPortal.Common.Models
{
    public class DailyStaffLevel
    {
        public DateTime Date { get; set; }
        public string DayOfWeek { get; set; }
        public int StaffStatusLevel { get; set; }
        public int EmployeesOnLeave { get; set; }
        public int DepartmentStaffLevel { get; set; }
        public int BusinessRoleStaffLevel { get; set; }
    }
}
