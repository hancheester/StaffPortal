using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StaffPortal.Common.APIModels
{
    public class TrainingModuleAPIModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = nameof(Name) + " field is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = nameof(Description) + " field is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = nameof(Location) + " field is required")]
        public string Location { get; set; }

        [Required(ErrorMessage = nameof(Frequency) + " field is required")]
        public int Frequency { get; set; }

        [Required(ErrorMessage = nameof(Reminder) + " field is required")]
        public int Reminder { get; set; }

        public DateTime DateCreated { get; set; }
        public int InvitationsCount { get; set; }
        public int AcceptedCount { get; set; }
        public string TrainingMaterialsFolderPath { get; set; }        
        public IList<EmployeeInfoAPIModel> InvitedEmployees { get; set; }
        public IList<Invitation> Invitations { get; set; }
        public IList<string> TrainingMaterialsFileNames { get; set; }
    }
}