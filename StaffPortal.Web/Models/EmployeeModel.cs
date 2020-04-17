using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StaffPortal.Web.Models
{
    public class EmployeeModel
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string NIN { get; set; }
        public DateTime? DOB { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required]
        public int HolidayAllowance { get; set; }
        [Required]
        public string Barcode { get; set; }
        [Required]
        public double HoursRequired { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string RepeatPassword { get; set; }

        public int PrimaryBusinessRoleId { get; set; }
        public int[] SecondaryBusinessRoleIds { get; set; }
        public IList<WorkingDayModel> WorkingDays { get; set; }
    }

    public class WorkingDayModel
    {
        public string Day { get; set; }
        public int DepartmentId { get; set; }
        public bool IsAssigned { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
