using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaffPortal.Common.APIModels
{
    public class AnnouncementRequest
    {
        public IList<int> BusinessRolesIds { get; set; }
        public IList<int> DepartmentsIds { get; set; }
    }
}