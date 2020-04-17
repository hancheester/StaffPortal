using Microsoft.AspNetCore.Mvc;
using StaffPortal.Web.Infrastructure;
using StaffPortal.Web.Models;
using System;

namespace StaffPortal.Web.Controllers
{
    //[Authorize(Roles = GlobalConstants.APPLICATIONROLE_MAIN)]
    public class AdminController : Controller
    {
        public AdminController()   
        {
           
        }

        public IActionResult Manage()
        {
            return View();
        }

        [CustomAuthorize("Manage system configuration")]
        public IActionResult Settings()
        {
            return View();
        }
        
        [CustomAuthorize("View business roles")]
        public IActionResult BusinessRoles()
        {
            return View();
        }

        public IActionResult Departments()
        {
            return View();
        }

        public IActionResult CreateDepartment()
        {
            DepartmentViewModel model = new DepartmentViewModel();

            return View(model);
        }

        //[ServiceFilter(typeof(AuditFilter))]
        //public IActionResult EditDepartment(int id)
        //{
        //    Department department = _departmentBLL.GetDepartmentById(id);
        //    DepartmentViewModel model = PrepareDepartment(department);

        //    return View(model);
        //}

        [HttpPost]
        public IActionResult EditDepartment(DepartmentViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {

            }

            return BadRequest();
        }

        //public DepartmentViewModel PrepareDepartment(Department department)
        //{
        //    DepartmentViewModel model = Mapper.Map<DepartmentViewModel>(department);
        //    model.BusinessRoles = new List<CheckableBusinessRole>();
        //    var businessRoles = _businessRoleBLL.GetAll();
        //    foreach (var role in businessRoles)
        //    {
        //        CheckableBusinessRole chkRole = Mapper.Map<CheckableBusinessRole>(role);
        //        if (department.BusinessRoles.Any(r => r.Id == role.Id))
        //        {
        //            chkRole.IsChecked = true;
        //            chkRole.MinimumRequired = _businessRole_DepartmentRepository.GetAll()
        //                .FirstOrDefault(bd => bd.BusinessRoleId == role.Id && bd.DepartmentId == department.Id)
        //                .MinimumRequired;
        //        }
        //        model.BusinessRoles.Add(chkRole);
        //    }

        //    return model;
        //}

        [CustomAuthorize("View users")]
        public IActionResult Users()
        {
            return View();
        }

        public IActionResult AuditTrail()
        {
            return View();
        }

        [CustomAuthorize("View pending leave requests")]
        public IActionResult Leave()
        {
            return View();
        }

        public IActionResult Training()
        {
            return View();
        }

        // TODO: Only Authorize Managers
        public IActionResult EditShifts()
        {
            return View();
        }

        public IActionResult EditTimesheet()
        {
            return View();
        }

        public IActionResult AccountantReports()
        {
            return View();
        }
    }
}