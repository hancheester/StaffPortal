using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StaffPortal.Common.APIModels
{
    public class AssignmentAPIModel : IValidatableObject
    {
        public int BusinessRoleId { get; set; }
        public int EmployeeId { get; set; }
        public int DepartmentId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsOnAssignment { get; set; }
        public int RecurringWeeks { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (EndTime < StartTime)
            {
                results.Add(new ValidationResult("End Time cannot be earlier than Start Time"));
            }

            return results;
        }
    }
}