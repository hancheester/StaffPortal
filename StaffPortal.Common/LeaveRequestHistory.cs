using System.Collections.Generic;

namespace StaffPortal.Common
{
    public class LeaveRequestHistory
    {
        LeaveRequest leaveRequest { get; set; }
        IEnumerable<RequestedDate> RequestedDates { get; set; }
    }
}