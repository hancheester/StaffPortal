using System.Collections.Generic;

namespace StaffPortal.Common.Models
{
    public class Timesheet
    {
        public IList<TimeclockTimestamp> Clockins { get; set; }
        public IList<TimeclockTimestamp> Clockouts { get; set; }
    }
}
