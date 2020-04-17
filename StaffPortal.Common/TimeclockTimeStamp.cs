using System;

namespace StaffPortal.Common
{
    public class TimeclockTimestamp : BaseEntity
    {
        public DateTime Timestamp { get; set; }
        public int EmployeeId { get; set; }
        public bool IsClockIn { get; set; }
        public bool IsApproved { get; set; }
        // Set to 0 (as pending) in case of ClockIn new Entry
        public int ClockOutRefId { get; set; }

        public TimeclockTimestamp()
        { }

        public TimeclockTimestamp(int employeeId, bool isClockIn)
        {
            this.Timestamp = DateTime.Now;
            this.EmployeeId = employeeId;
            this.IsClockIn = isClockIn;
            this.ClockOutRefId = -1;
            if (isClockIn)
                this.ClockOutRefId = 0;

        }
    }
}