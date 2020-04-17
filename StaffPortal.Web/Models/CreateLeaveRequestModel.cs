using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StaffPortal.Web.Models
{
    public class CreateLeaveRequestModel
    {
        public int Id { get; set; }
        public int LeaveTypeId { get; set; }
        [Required]
        public bool IsEmergency { get; set; }
        [Required]        
        public string Note { get; set; }
        public IList<CreateRequestedDateModel> RequestedDates { get; set; }
    }
}