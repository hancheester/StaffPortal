using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StaffPortal.Common
{
    public class TrainingModule : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int Frequency { get; set; }
        public int Reminder { get; set; }
        public string TrainingMaterialsFolderPath { get; set; }
        public DateTime DateCreated { get; set; }

        [NotMapped]
        public IList<Invitation> Invitations { get; set; }
    }
}