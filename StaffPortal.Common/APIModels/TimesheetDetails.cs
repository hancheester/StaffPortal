using System;

namespace StaffPortal.Common.APIModels
{
    public class TimesheetDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int TimeclockId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Timestamp { get; set; }
        public double TotalTime { get; set; }
        public string BusinessRoleName { get; set; }
        public int BusinessRoleId { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }
        public bool IsClockIn { get; set; }
        public bool IsApproved { get; set; }
        public ShiftAPIModel Shift { get; set; }
    }
}