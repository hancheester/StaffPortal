using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaffPortal.Common.APIModels
{
    public class BusinessRole_PermissionAPIModel : BusinessRole_Permission
    {
        public bool IsAllowed { get; set; }
    }
}
