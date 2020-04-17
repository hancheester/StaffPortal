using System;

namespace StaffPortal.Common
{
    public class OpeningHour : BaseEntity
    {
        public int DepartmentId { get; set; }
        public string Day { get; set; }
        public bool IsOpen { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
    }
}