using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StaffPortal.Common
{
    public class Employee : BaseEntity
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string NIN { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int HolidayAllowance { get; set; }
        public string Barcode { get; set; }
        public double HoursRequired { get; set; }

        [NotMapped]
        public IdentityUser User { get; set; }

        [NotMapped]
        public IList<WorkingDay> WorkingDays { get; set; }
    }
}