using System;

namespace StaffPortal.Common.APIModels
{
    /* 
     * This class is used to flatten information from LeaveRequests Table and RequestedDates Table
     * so to have one single instance for each day of approved leave request. It is intended to be used 
     * as model for the user shift views.
    */
    public class LeaveAPIModel
    {
        // From Leave Request
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int BusinessRoleId { get; set; }
        public string BusinessRole { get; set; }
        public string LeaveTypeName { get; set; }
        public DateTime DateCreated { get; set; }

        // From Requested Date
        public DateTime Date { get; set; }
        public bool IsFullDay { get; set; }
        public string DepartmentName { get; set; }
    }
}