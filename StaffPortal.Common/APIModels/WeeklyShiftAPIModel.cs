using System.Collections.Generic;

namespace StaffPortal.Common.APIModels
{
    public class WeeklyShiftAPIModel
    {
        public int DepartmentMinRequired { get; set; }
        public IList<BusinessRoleEmployeesOnShiftAPIModel> BusinessRole_Employees { get; set; }
        public IList<OpeningHour> OpeningHours { get; set; }
    }
}