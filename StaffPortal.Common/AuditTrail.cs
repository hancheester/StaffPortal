using System;

namespace StaffPortal.Common
{
    public class AuditTrail
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Event { get; set; }
        public string Details { get; set; }
        public DateTime DateTimeCreated { get; set; }
    }
}