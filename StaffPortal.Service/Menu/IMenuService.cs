using StaffPortal.Common;
using System.Collections.Generic;

namespace StaffPortal.Service.Menu
{
    public interface IMenuService
    {
        IList<MenuItem> GetMenuItemsByBusinessRoleId(int businessRoleId);
    }
}
