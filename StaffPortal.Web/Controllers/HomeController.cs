using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StaffPortal.Web.Controllers
{
    [Authorize]
    //[ServiceFilter(typeof(AuditFilter))]
    public class HomeController : Controller
    {
        public HomeController()
        { }

        public IActionResult Index()
        {
            return View();
        }
    }
}