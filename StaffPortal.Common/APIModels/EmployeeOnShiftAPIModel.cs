using System.Collections.Generic;

namespace StaffPortal.Common.APIModels
{
    public class EmployeeOnShiftAPIModel
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public BusinessRole BusinessRole { get; set; }
        public IList<BusinessRole> SecondaryBusinessRoles { get; set; }
        public IList<DaysWorkingAPIModel> DaysWorking { get; set; }
        public IList<LeaveAPIModel> LeaveDays { get; set; }
        public IList<AssignmentAPIModel> Assignments { get; set; }
    }
}