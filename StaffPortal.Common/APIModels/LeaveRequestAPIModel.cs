using System;
using System.Collections.Generic;

namespace StaffPortal.Common.APIModels
{
    public class LeaveRequestAPIModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; }
        public string DateCreated { get; set; }
        public bool IsEmergency { get; set; }
        public string Note { get; set; }         
        public IList<RequestedDateAPIModel> RequestedDates { get; set; }
    }
}