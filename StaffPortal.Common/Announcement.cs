using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StaffPortal.Common
{
    public class Announcement : BaseEntity
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime Date { get; set; }
        public string Body { get; set; }

        [NotMapped]
        public IList<Recipient> Recipients { get; set; }
    }
}