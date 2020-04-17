using System;

namespace StaffPortal.Common
{
    public class RejectionReason : BaseEntity
    {
        public string Message { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;        
    }
}