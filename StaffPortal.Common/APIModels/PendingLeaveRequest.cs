using System;
using System.Collections.Generic;

namespace StaffPortal.Common.APIModels
{
    public class PendingLeaveRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string BusinessRole { get; set; }
        public int BusinessRoleId { get; set; }
        public string LeaveTypeName { get; set; }
        public string DepartmentName { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime DateCreated { get; set; }
        public string StatusCodeName { get; set; }
        public bool IsEmergency { get; set; }
        public bool IsAccepted { get; set; }
        public string Note { get; set; }
        public string RejectionReason { get; set; }
        public IList<RequestedDate> RequestedDates { get; set; }
        public IList<PendingRequestedDate> PendingRequestedDates { get; set; }
    }
}