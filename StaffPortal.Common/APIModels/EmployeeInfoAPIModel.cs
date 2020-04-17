using System;
using System.Collections.Generic;

namespace StaffPortal.Common.APIModels
{
    public class EmployeeInfoAPIModel
    {
        // FROM: ApplicationUser 
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }

        // FROM: EmployeeInfo
        public string ApplicationUserId { get; set; }
        public string NIN { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int HolidayAllowance { get; set; }
        public string Barcode { get; set; }
        public double HoursRequired { get; set; }
        public bool IsClockIn { get; set; }

        // FROM: BusinnessRole
        public BusinessRole PrimaryBusinessRole { get; set; }

        // FROM: Departments Ids
        public IList<Department> AssignedDepartments { get; set; }
    }
}