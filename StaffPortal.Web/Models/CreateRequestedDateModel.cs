using System;

namespace StaffPortal.Web.Models
{
    public class CreateRequestedDateModel
    {        
        public DateTime Date { get; set; }
        public bool IsFullDay { get; set; }
        public int DepartmentId { get; set; }
    }
}