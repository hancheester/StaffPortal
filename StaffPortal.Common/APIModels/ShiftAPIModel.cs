using System;

namespace StaffPortal.Common.APIModels
{
    public class ShiftAPIModel
    {
        public DateTime Date { get; set; }
        public string DepartmentName { get; set; }
        public string BusinessRoleName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAssignment { get; set; }
        public bool IsDayWorking { get; set; }
    }
}
