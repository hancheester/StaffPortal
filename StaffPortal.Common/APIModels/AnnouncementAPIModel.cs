using System.Collections.Generic;

namespace StaffPortal.Common.APIModels
{
    public class AnnouncementAPIModel : Announcement
    {
        public IList<EmployeeInfoAPIModel> Recipients { get; set; }
    }
}