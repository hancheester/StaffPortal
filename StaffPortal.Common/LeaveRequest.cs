using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StaffPortal.Common
{
    public class LeaveRequest : BaseEntity
    {
        public DateTime DateCreated { get; set; }
        public int EmployeeId { get; set; }
        public bool IsEmergency { get; set; }
        public int LeaveTypeId { get; set; }
        public string Note { get; set; }    
        
        [NotMapped]
        public IList<RequestedDate> RequestedDates { get; set; }
    }
}