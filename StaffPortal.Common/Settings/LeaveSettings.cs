using System.Collections.Generic;

namespace StaffPortal.Common.Settings
{
    public class LeaveSettings : ISettings
    {
        public bool LeavesToNewYear { get; set; }
        public int LeaveNoticePeriod { get; set; }
        public int EmergencyAllowance { get; set; }
        public List<string> MonthsToAccrue { get; set; }

        public LeaveSettings()
        {
            MonthsToAccrue = new List<string>();
        }
    }
}
