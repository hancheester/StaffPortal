using Microsoft.AspNetCore.Mvc;
using StaffPortal.Service.Staff;
using StaffPortal.Web.Extensions;
using StaffPortal.Web.Models;

namespace StaffPortal.Web.Controllers
{
    public class ManageController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public ManageController(
            IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public IActionResult MyAccount()
        {
            var id = User.Claims.GetEmployeeId();
            return View(id);
        }

        public IActionResult MyLeaves()
        {
            return View();
        }

        public IActionResult MyShifts()
        {
            return View();
        }

        public IActionResult PendingLeaveRequest()
        {
            return View();
        }

        public IActionResult Departments()
        {
            return View();
        }

        public IActionResult NewDepartment()
        {
            return View();
        }

        public IActionResult Department(int id)
        {
            return View(id);
        }

        public IActionResult Employees()
        {
            return View();
        }

        public IActionResult NewEmployee()
        {
            return View();
        }

        public IActionResult Employee(int id)
        {
            return View(id);
        }

        public IActionResult BusinessRoles()
        {
            return View();
        }

        public IActionResult EditStaffShifts()
        {
            return View();
        }


        public IActionResult Training()
        {
            return View();
        }

        public IActionResult Announcements()
        {
            return View();
        }

        public IActionResult ClockInOut(string returnUrl = null)
        {
            return View();
        }

        [HttpPost]
        public IActionResult ClockInOut(string barcode, string returnUrl = null)
        {
            var result = _employeeService.GetEmployeeByBarcode(barcode);
            if (result.Succeeded)
            {
                var timeclock = _employeeService.InsertTimeclockTimeStamp(result.Object.Id);

                ClocktimeViewModel vm = new ClocktimeViewModel(result.Object.FirstName, result.Object.LastName, timeclock.Timestamp, timeclock.IsClockIn);

                return RedirectToAction(nameof(ClockInOutSucceeded), vm);
            }

            ModelState.AddErrors(result.ErrorMessages);
            return ClockInOut(returnUrl);
        }

        public IActionResult ClockInOutSucceeded(ClocktimeViewModel model)
        {
            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}