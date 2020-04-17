using System.Collections.Generic;

namespace StaffPortal.Web.Models
{
    public class MyLeaveRequestModel
    {
        public string RequestedOn { get; set; }
        public string Type { get; set; }
        public bool IsEmergency { get; set; }
        public string Note { get; set; }
        public string OverallStatus { get; set; }
        public IList<MyRequestedDateModel> RequestedDates { get; set; }
    }
}
