using System.Collections.Generic;

namespace StaffPortal.Common.Models
{
    public class DaysWorkingResult
    {
        public string UserId { get; set; }
        public int EmployeeId { get; set; }
        public bool Succeded { get; set; }
        public IList<DayWorkingResult> DayWorkingResult { get; set; }
    }
}
