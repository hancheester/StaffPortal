using Microsoft.AspNetCore.Mvc;

namespace StaffPortal.Web.Controllers
{
    public class SystemController : Controller
    {
        public IActionResult Settings()
        {
            return View();
        }
    }
}