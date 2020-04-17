using System;

namespace StaffPortal.Web.Models
{
    public class MyRequestedDateModel
    {
        public string Date { get; set; }
        public bool IsFullDay { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }        
        public string Approver { get; set; }
        public string Reason { get; set; }



        //TODO: Should have CreatedOne
        //TODO: Should change to UpdatedOn
        public DateTime DateProcessed { get; set; }
        public int StatusCode { get; set; }
        public int? RejectionReasonId { get; set; }
        public int AccruableAs { get; set; }
    }
}
