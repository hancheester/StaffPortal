using StaffPortal.Common.APIModels;
using System;
using System.Collections.Generic;

namespace StaffPortal.Common.Models
{
    public class CodedCalendarDay
    {
        public DateTime Date { get; set; }
        public string DayOfTheWeekName { get; set; }
        public int StatusCode { get; set; }
        public string StatusCodeName { get; set; }
        public bool IsDisabled { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }
        public bool IsOnHoliday { get; set; }
        public IList<EmployeeInfoAPIModel> EmployeesOnHolidays { get; set; }
    }
}