using System;
using System.Collections.Generic;

namespace StaffPortal.Common
{
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public string DayOfTheWeekName { get; set; }
        public int StatusCode { get; set; }
        public string StatusCodeName { get; set; }
        public bool IsDisabled { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }
        public bool IsOnHoliday { get; set; }
        public IList<OnHolidayEmployee> EmployeesOnHolidays { get; set; }
    }

    public class OnHolidayEmployee
    {
        public string Name { get; set; }
    }
}
