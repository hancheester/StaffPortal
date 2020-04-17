using System;

namespace StaffPortal.Common
{
    public class Recipient : BaseEntity
    {
        public DateTime CreateDate { get; set; }
        public int EmployeeId { get; set; }
        public int AnnouncementId { get; set; }
        public int Status { get; set; }

        public Recipient()
        { }

        public Recipient(int employeeId, int announcementId)
        {
            this.EmployeeId = employeeId;
            this.AnnouncementId = announcementId;
            this.CreateDate = DateTime.Now;
            this.Status = 0;
        }        
    }
}