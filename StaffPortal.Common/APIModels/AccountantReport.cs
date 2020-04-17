using StaffPortal.Common.Models;
using System;

namespace StaffPortal.Common.APIModels
{
    public class AccountantReport
    {
        public string EmployeeFirstName { get; set; }
        public string EmployeeLastName { get; set; }
        public int EmployeeId { get; set; }
        public TimeSpan ExpectedWorkingHours { get; set; }
        public TimeSpan WorkedHours { get; set; }
        public TimeSpan ApprovedHours { get; set; }
        public HoursOnLeave HoursOnLeave { get; set; }
        // A model for leave types { leaveTypeName | AmountOfHours }

        public TimeSpan TotalHours { get; set; }
        public int Adjustment { get; set; }
    }
}