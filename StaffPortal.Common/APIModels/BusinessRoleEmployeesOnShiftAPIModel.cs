using StaffPortal.Common.Models;
using System.Collections.Generic;

namespace StaffPortal.Common.APIModels
{
    public class BusinessRoleEmployeesOnShiftAPIModel
    {
        public BusinessRole BusinessRole { get; set; }
        public int? BusinessRoleMinRequired { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public IList<EmployeeOnShiftAPIModel> EmployeesOnShift { get; set; }
        public IList<DailyStaffLevel> StaffLevel { get; set; }
    }
}