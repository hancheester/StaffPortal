using System;

namespace StaffPortal.Common.APIModels
{
    public class DaysWorkingAPIModel : WorkingDay
    {
        public string DepartmentName { get; set; }
        public DateTime? Date { get; set; }
        public bool IsOnHoliday { get; set; }
        public string LeaveTypeName { get; set; }
        public int DepartmentStaffLevel { get; set; }
        public int BusinessRoleStaffLevel { get; set; }
        public bool isAssignment { get; set; }
    }
}