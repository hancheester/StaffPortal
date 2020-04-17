using Microsoft.AspNetCore.Mvc;
using StaffPortal.Service.Menu;
using StaffPortal.Web.Extensions;
using System.Security.Claims;

namespace StaffPortal.Web.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;

        public MenuViewComponent(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public IViewComponentResult Invoke()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User as ClaimsPrincipal;
                var roleId = user.Claims.GetPrimaryBusinessRoleId();
                var menu = _menuService.GetMenuItemsByBusinessRoleId(roleId);

                return View(menu);
            }

            return View();
        }
    }
}
