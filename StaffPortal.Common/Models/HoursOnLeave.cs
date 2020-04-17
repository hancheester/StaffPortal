using System;

namespace StaffPortal.Common.Models
{
    public class HoursOnLeave
    {
        public TimeSpan Payable { get; set; }
        public TimeSpan NonPayable { get; set; }

        public HoursOnLeave()
        {
            this.Payable = new TimeSpan(0);
            this.NonPayable = new TimeSpan(0);
        }
    }
}
