using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace StaffPortal.Common
{
    public class RequestedDate : BaseEntity
    {
        public int LeaveRequestId { get; set; }
        public DateTime Date { get; set; }
        public bool IsFullDay { get; set; }
        public int DepartmentId { get; set; }
        public string ApproverId { get; set; }
        //TODO: Should have CreatedOne
        //TODO: Should change to UpdatedOn
        public DateTime DateProcessed { get; set; }
        public int StatusCode { get; set; }
        public int? RejectionReasonId { get; set; }
        public int AccruableAs { get; set; }

        [NotMapped]
        public RejectionReason Reason { get; set; }
    }
}