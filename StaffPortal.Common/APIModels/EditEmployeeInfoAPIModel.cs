using Microsoft.AspNetCore.Identity;
using StaffPortal.Common.APIModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StaffPortal.Common.ViewModels
{
    public class EditEmployeeInfoAPIModel : Employee, IValidatableObject
    {
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string RepeatPassword { get; set; }
        public BusinessRole PrimaryBusinessRole { get; set; }
        public IdentityRole SystemRole { get; set; }
        public IList<SecondaryBusinessRoleAPIModel> SecondaryBusinessRoles { get; set; }
        new public IList<DaysWorkingAPIModel> DaysWorking { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            foreach (var dayWorking in DaysWorking)
            {

                if (dayWorking.IsAssigned)
                {
                    if (dayWorking.StartTime == null)
                        results.Add(new ValidationResult(string.Format("Please provide a Start Time, on day: {0}",
                            dayWorking.Day.ToUpper())));
                    if (dayWorking.EndTime == null)
                        results.Add(new ValidationResult(string.Format("Please provide a End Time, on day: {0}",
                            dayWorking.Day.ToUpper())));
                }

                if (dayWorking.IsAssigned && dayWorking.EndTime < dayWorking.StartTime)
                {
                    results.Add(new ValidationResult(string.Format("End Time cannot be earlier than Start Time, on day: {0}",
                        dayWorking.Day.ToUpper())));
                }
            }

            if (EndDate != null && EndDate < StartDate)
            {
                results.Add(new ValidationResult(string.Format("End Date ({0}) cannot be earlier than Start Date ({1}).",
                    EndDate.Value.ToShortDateString(),
                    StartDate.ToShortDateString())));
            }

            return results;
        }
    }
}