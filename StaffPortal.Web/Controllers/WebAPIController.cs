using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StaffPortal.Service.Departments;
using StaffPortal.Service.Errors;
using StaffPortal.Service.Leave;
using StaffPortal.Service.Resource;
using StaffPortal.Service.Roles;
using StaffPortal.Service.Staff;
using StaffPortal.Common;
using StaffPortal.Common.APIModels;
using StaffPortal.Common.Models;
using StaffPortal.Web.Extensions;
using StaffPortal.Web.Infrastructure;
using StaffPortal.Web.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Web.Controllers
{
    public class WebAPIController : Controller
    {
        private readonly IBusinessRoleService _businessRoleBLL;
        //private readonly IPermissionBLL _permissionBLL;
        private readonly IDepartmentService _departmentService;
        //private readonly IOpeningHourBLL _openingHourBLL;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IEmployeeService _employeeService;
        private readonly IWorkingDaysService _daysWorkingService;
        private readonly ILeaveService _leaveService;
        private readonly IErrorService _errorService;
        private readonly IResourcesService _resourcesBLL;

        public WebAPIController(
            IBusinessRoleService businessRoleBLL,
            //IPermissionBLL permissionBLL,
            IDepartmentService departmentService,
            //IOpeningHourBLL openingHourBLL,
            IConfiguration configuration,
            IHostingEnvironment env,
            ILeaveTypeService leaveTypeService,
            IEmployeeService employeeService,
            IWorkingDaysService daysWorkingService,
            ILeaveService leaveService,
            IErrorService errorService,
            IResourcesService resourcesBLL,
            UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _businessRoleBLL = businessRoleBLL;
            _departmentService = departmentService;
            //_openingHourBLL = openingHourBLL;            //_openingHourBLL = openingHourBLL;
            _configuration = configuration;
            _env = env;
            GENERALSETTINGS_FILEPATH = Path.Combine(_env.WebRootPath, GlobalConstants.FILENAME_GENERALSETTINGS);
            _leaveTypeService = leaveTypeService;
            _employeeService = employeeService;
            _daysWorkingService = daysWorkingService;
            _leaveService = leaveService;
            _errorService = errorService;
            _resourcesBLL = resourcesBLL;
        }

        private readonly string GENERALSETTINGS_FILEPATH;

        //[HttpGet("webapi/getpermissions")]
        //public JsonResult GetPermissions()
        //{
        //    var permissions = _permissionBLL.GetAll();
        //    return Json(permissions);
        //}

        [HttpGet("webapi/loggedinuserid")]
        public IActionResult GetLoggedinUserid()
        {
            var userId = _userManager.GetUserId(User);
            var employee = _employeeService.GetEmployeeByApplicationUserId(userId);

            return Ok(Json(employee.Id));
        }

        #region BUSINESS ROLES

        //[HttpGet("webapi/getuserbusinessrole")]
        //public IActionResult GetBusinessRolesByUserId()
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var userId = _userManager.GetUserId(User);
        //        var employee = _employeeService.GetEmployeeByApplicationUserId(userId);

        //        return Json(_businessRoleBLL.GetBusinessRolesByEmployeeId(employee.Id));
        //    }

        //    return Unauthorized();
        //}

        //[HttpPost("webapi/updatebusinessrole")]
        //public IActionResult UpdateBusinessRole([FromBody]BusinessRole role)
        //{
        //    var result = _businessRoleBLL.Update(role);

        //    if (result.Succeeded)
        //        return Ok(Json(role));
        //    else
        //    {
        //        var errorMessages = result.ErrorMessages.ToList();
        //        return BadRequest(Json(errorMessages));
        //    }
        //}

        //[HttpGet("webapi/getbusinessroleshierarchy")]        
        //public IActionResult GetAllBusinessRolesHierarchy()
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var hierarchy = _businessRoleBLL.GetHierarchy();
        //        return Ok(Json(hierarchy));
        //    }

        //    return Unauthorized();
        //}

        [HttpGet("webapi/getbusinessroles")]
        public IActionResult GetAllBusinessRoles()
        {
            if (User.Identity.IsAuthenticated)
            {
                var roles = _businessRoleBLL.GetAll();
                return Ok(Json(roles));
            }

            return Unauthorized();
        }

        [HttpGet("webapi/getbusinessrole/{roleId}")]
        public IActionResult GetAllBusinessRoles(int roleId)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Json(_businessRoleBLL.GetBusinessRoleById(roleId));
            }

            return Unauthorized();
        }

        //[HttpGet("webapi/getsecondarybusinessroles")]
        //public IActionResult GetSecondaryBusinessRoles()
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        try
        //        {
        //            var model = PrepareSecondaryBusinessRoles();

        //            return Json(model);
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }

        //    return Unauthorized();
        //}

        [HttpPost("webapi/createbusinessrole")]
        public IActionResult CreateBusinessRole([FromBody]BusinessRole model)
        {
            if (User.Identity.IsAuthenticated)
            {
                var role = new BusinessRole
                {
                    Name = model.Name,
                    ParentBusinessRoleId = model.ParentBusinessRoleId
                };

                _businessRoleBLL.Insert(role);
                return Created($"/webapi/createbusinessrole/", role);
            }

            return Unauthorized();
        }

        //[HttpGet("webapi/getchildrenroles/{roleId}")]
        //public IActionResult GetChildrenRoles(int roleId)
        //{
        //    var roles = _businessRoleBLL.GetChildrenBusinessRoles(roleId);

        //    return Ok(Json(new { ChildrenRoles = roles }));
        //}

        //[HttpGet("webapi/getchildrenroles")]
        //public IActionResult GetChildrenRoles()
        //{
        //    var userId = _userManager.GetUserId(User);
        //    var employeeId = _employeeService.GetEmployeeIdOnApplicationUserId(userId);
        //    var role = _businessRoleBLL.GetPrimaryBusinessRoleByEmployeeId(employeeId);

        //    var roles = _businessRoleBLL.GetChildrenBusinessRoles(role.Id);

        //    return Ok(Json(new { ChildrenRoles = roles }));
        //}

        #endregion BUSINESS ROLES

        //[HttpGet("webapi/getsystemroles")]
        //public IActionResult GetSystemRoles()
        //{

        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var roles = _businessRoleBLL.GetSystemRoles();

        //        return Ok(Json(roles));
        //    }

        //    return Unauthorized();
        //}

        //[HttpPost("webapi/businessRole/updatepermissions")]
        //public IActionResult UpdateBusinessRole_Permission([FromBody]List<BusinessRole_PermissionAPIModel> role_permissions)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        var result = _businessRoleBLL.UpdatePermissions(role_permissions);

        //        if (result)
        //            return Created("webapi/updatebusinessrole_permission", Json(role_permissions));
        //    }

        //    return BadRequest();
        //}

        //[HttpGet("webapi/GetDepartments")]
        //public IActionResult GetDepartments()
        //{
        //    var departments = _departmentService.GetAllDepartments();
        //    return Ok(Json(departments));
        //}

        //[HttpGet("webapi/read_only/getdepartments")]
        //public IActionResult ROGetDepartments()
        //{
        //    var departmnets = _departmentService.ReadOnly_GetAll();

        //    return Ok(Json(departmnets));
        //}

        //[HttpPost("webapi/createdepartment")]
        //public IActionResult CreateDepartment([FromBody]DepartmentViewModel model)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            Department newDepartment = new Department
        //            {
        //                Name = model.Name,
        //                MinimumRequired = model.MinimumRequired
        //            };
        //            _departmentService.AddDepartment(newDepartment);
        //            newDepartment = _departmentService.GetDepartmentById(newDepartment.Id);
        //            var viewModel = PrepareDepartment(newDepartment);

        //            return Created($"/webapi/createdepartment/", viewModel);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        // log error
        //    }

        //    // TODO: check the extension method is working and replace in all the API
        //    var errorMessages = ModelState.RetrieveModelStateErrorMessages();

        //    return BadRequest(Json(errorMessages));
        //}

        [HttpDelete("webapi/DeleteDepartment")]
        public IActionResult DeleteDepartment(int departmentId)
        {
            try
            {
                _departmentService.DeleteDepartment(departmentId);
                return Ok();
             
            }
            catch (Exception ex)
            {

            }

            return BadRequest();
        }

        //[HttpGet("webapi/getdepartment/{id}")]
        //public IActionResult GetDepartment(int id = 1)
        //{
        //    try
        //    {
        //        Department department = _departmentService.GetDepartmentById(id);
        //        DepartmentViewModel model = PrepareDepartment(department);

        //        if (department != null)
        //        {
        //            return Ok(Json(model));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log Error
        //    }

        //    return BadRequest();
        //}

        //[HttpGet("webapi/getdepatmentonemployee/{employeeId}")]
        //public IActionResult GetDepartmentOnEmployee(int employeeId = -1)
        //{
        //    var departments = _departmentService.GetAssignedDepartmentsByEmployeeId(employeeId);

        //    return Ok(Json(departments));
        //}

        //// TODO: Verify is Approver
        //[HttpGet("webapi/manage-admin/getdepatmentsonemployee")]
        //public IActionResult GetDepartmentOnEmployee()
        //{
        //    var userId = _userManager.GetUserId(User);
        //    var employeeId = _employeeService.GetEmployeeIdOnApplicationUserId(userId);
        //    var departments = _departmentService.GetAssignedDepartmentsByEmployeeId(employeeId);

        //    return Ok(Json(departments));
        //}

        [HttpGet("webapi/manage-admin/getstafflevel")]
        public IActionResult GetStaffLevel()
        {
            DateTime date = new DateTime(2017, 12, 18);
            var lvl = _departmentService.GetStaffCount(1, date, 3);

            return Ok(Json(lvl));
        }

        //[HttpPost("webapi/updateDepartment")]
        //public IActionResult UpdateDepartment([FromBody]DepartmentViewModel model)
        //{
        //    try
        //    {
        //        Department department = Mapper.Map<Department>(model);
        //        _departmentService.UpdateDepartment(department);

        //        foreach (var businessRole in model.BusinessRoles)
        //        {
        //            BusinessRole_Department brd = new BusinessRole_Department
        //            {
        //                BusinessRoleId = businessRole.Id,
        //                DepartmentId = department.Id,
        //                MinimumRequired = businessRole.MinimumRequired
        //            };
        //            if (businessRole.IsChecked)
        //            {
        //                if (!_businessRole_DepartmentRepository.GetAll().Any(r => r.DepartmentId == brd.DepartmentId && r.BusinessRoleId == brd.BusinessRoleId))
        //                    _businessRole_DepartmentRepository.Create(brd);
        //            }
        //            else
        //            {
        //                var brdToDelete = _businessRole_DepartmentRepository.GetAll().FirstOrDefault(r => r.DepartmentId == brd.DepartmentId && r.BusinessRoleId == brd.BusinessRoleId);
        //                if (brdToDelete != null)
        //                    _businessRole_DepartmentRepository.Delete(brdToDelete);
        //            }
        //        }

        //        foreach (var openingHours in model.OpeningHours)
        //        {
        //            _openingHourBLL.Update(openingHours);
        //        }
        //        return Ok(department);
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    return BadRequest();
        //}

        #region COMPANY INFO SETTINGS
        //[HttpGet("webapi/getcompanyinfo")]
        //public IActionResult GetCompanyInfo()
        //{
        //    GeneralSettingsViewModel json;
        //    using (StreamReader r = new StreamReader(GENERALSETTINGS_FILEPATH))
        //    {
        //        var file = r.ReadToEnd();
        //        json = JsonConvert.DeserializeObject<GeneralSettingsViewModel>(file);
        //    }

        //    return Json(json.CompanyInfo);
        //}

        //[HttpPost("webapi/editcompanyinfo")]
        //public IActionResult EditCompanyInfo([FromBody]CompanyInfoViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        GeneralSettingsViewModel json;
        //        using (StreamReader r = new StreamReader(GENERALSETTINGS_FILEPATH))
        //        {
        //            var file = r.ReadToEnd();
        //            json = JsonConvert.DeserializeObject<GeneralSettingsViewModel>(file);
        //        }
        //        json.CompanyInfo = model;
        //        CompanyInfoViewModel mock = Mapper.Map<CompanyInfoViewModel>(model);

        //        System.IO.File.WriteAllText(GENERALSETTINGS_FILEPATH, JsonConvert.SerializeObject(json, Formatting.Indented));
        //        return Ok();
        //    }

        //    var errors = ModelState.Select(x => x.Value.Errors)
        //   .Where(y => y.Count > 0)
        //   .ToList();

        //    var errorMessages = new List<string>();
        //    foreach (var modelState in errors)
        //    {
        //        foreach (var errMessage in modelState)
        //        {
        //            errorMessages.Add(errMessage.ErrorMessage);
        //        }
        //    }

        //    return BadRequest(Json(errorMessages));
        //}

        //[HttpPost("webapi/editcompanyinfofiles")]
        //public async Task<IActionResult> Editcompanyinfofiles(IFormFile file, CompanyInfoViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (file != null && file.Length > 0)
        //        {
        //            var logoFilePath = Path.Combine(_env.WebRootPath, "images", file.FileName);
        //            model.LogoPath = "/images/" + file.FileName;

        //            using (var filestream = new FileStream(logoFilePath, FileMode.Create))
        //            {
        //                await file.CopyToAsync(filestream);
        //            }
        //        }

        //        GeneralSettingsViewModel json;
        //        using (StreamReader r = new StreamReader(GENERALSETTINGS_FILEPATH))
        //        {
        //            var jsonFile = r.ReadToEnd();
        //            json = JsonConvert.DeserializeObject<GeneralSettingsViewModel>(jsonFile);
        //        }

        //        json.CompanyInfo = model;

        //        System.IO.File.WriteAllText(GENERALSETTINGS_FILEPATH, JsonConvert.SerializeObject(json, Formatting.Indented));

        //        return Ok();
        //    }

        //    var errors = ModelState.Select(x => x.Value.Errors)
        //   .Where(y => y.Count > 0)
        //   .ToList();

        //    var errorMessages = new List<string>();
        //    foreach (var modelState in errors)
        //    {
        //        foreach (var errMessage in modelState)
        //        {
        //            errorMessages.Add(errMessage.ErrorMessage);
        //        }
        //    }

        //    return BadRequest(Json(errorMessages));
        //}
        #endregion END COMPANY INFO SETTINGS

        #region TRAINING MODULES

        [HttpPost("webapi/trainingmaterialstemp")]
        public async Task<IActionResult> TrainingMaterialsTempFile(IFormFile file)
        {
            if (ModelState.IsValid)
            {
                if (file != null && file.Length > 0)
                {
                    var logoFilePath = Path.Combine(_env.WebRootPath, "temp", file.FileName);
                    var filePaths = HttpContext.Session.GetCollection<string>(User.Identity.Name + GlobalConstants.SESSIONKEYSUFFIX_TRAININGMODULE);
                    if (filePaths == null || !filePaths.Any(f => f.Equals(logoFilePath)))
                    {
                        HttpContext.Session.AddObject<string>(User.Identity.Name + GlobalConstants.SESSIONKEYSUFFIX_TRAININGMODULE, logoFilePath);

                        using (var filestream = new FileStream(logoFilePath, FileMode.Create))
                        {
                            await file.CopyToAsync(filestream);
                        }

                        return Created("webapi/trainingmaterialstemp", file.Name);
                    }
                    else
                    {
                        return BadRequest(new { errorMessage = "File Already Exist!" });
                    }


                }
            }

            var errorMessages = ModelState.RetrieveModelStateErrorMessages();

            return BadRequest(Json(errorMessages));
        }

        [HttpGet("webapi/session_trainingMaterials")]
        public IActionResult GetTrainingMaterialsFromSession()
        {
            var filePaths = HttpContext.Session.GetCollection<string>(User.Identity.Name + GlobalConstants.SESSIONKEYSUFFIX_TRAININGMODULE);

            return Ok(Json(filePaths));
        }

        [HttpGet("webapi/gettrainingmodules")]
        public IActionResult GetTrainingModules()
        {
            var modules = _resourcesBLL.GetAllOnAPIModel();

            return Ok(Json(modules));
        }

        [HttpPost("webapi/inserttrainingmodule")]
        public IActionResult InsertTrainingModule([FromBody]TrainingModuleAPIModel model)
        {
            if (ModelState.IsValid)
            {
                string trainingModulePath = Path.Combine(_env.WebRootPath, "training_module", Guid.NewGuid().ToString());

                var filePaths = HttpContext.Session.GetCollection<string>(User.Identity.Name + GlobalConstants.SESSIONKEYSUFFIX_TRAININGMODULE);
                if (filePaths != null && filePaths.Count > 0)
                {
                    model.TrainingMaterialsFolderPath = trainingModulePath;
                    foreach (var path in filePaths)
                    {
                        var file = PhysicalFile(path, "image/jpeg");
                        byte[] bytes = System.IO.File.ReadAllBytes(path);
                        FileContentResult img = null;

                        using (var r = new FileStream(path, FileMode.Create))
                        {
                            img = File(bytes, "image/jpeg", file.FileName);
                        }

                        var index = img.FileDownloadName.LastIndexOf('\\');

                        var filename = img.FileDownloadName.Substring(index + 1);

                        var trainingMaterialPath = Path.Combine(trainingModulePath, filename);
                        if (!Directory.Exists(trainingModulePath))
                            Directory.CreateDirectory(trainingModulePath);

                        using (var filestream = new FileStream(trainingMaterialPath, FileMode.CreateNew))
                        {
                            filestream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }

                HttpContext.Session.Remove(User.Identity.Name + GlobalConstants.SESSIONKEYSUFFIX_TRAININGMODULE);
                _resourcesBLL.Insert(model);

                return Ok(Json(model));
            }

            var errorMesgs = ModelState.RetrieveModelStateErrorMessages();

            return BadRequest(errorMesgs);
        }

        [HttpGet("webapi/gettrainingmodule/{moduleId}")]
        public IActionResult GetTrainingModule(int moduleId)
        {
            var module = _resourcesBLL.Get(moduleId);

            if (module.TrainingMaterialsFolderPath != null)
            {
                var tempFolderPath = Path.Combine(_env.WebRootPath, "temp");
                foreach (var filename in module.TrainingMaterialsFileNames)
                {
                    var sessionFileFullPaths = HttpContext.Session.GetCollection<string>(User.Identity.Name + GlobalConstants.SESSIONKEYSUFFIX_TRAININGMODULE);
                    var filePath = tempFolderPath + "\\" + filename;
                    if (sessionFileFullPaths == null || !sessionFileFullPaths.Any(p => String.Equals(p, filePath)))
                        HttpContext.Session.AddObject<string>(User.Identity.Name + GlobalConstants.SESSIONKEYSUFFIX_TRAININGMODULE, filePath);
                }
            }

            return Ok(Json(module));
        }

        [HttpGet("webapi/getinvitedemployees/{trainingModuleId}")]
        public IActionResult GetInvitedEmployees(int trainingModuleId)
        {
            var invited = _employeeService.GetAllInvitedEmployees(trainingModuleId);

            return Ok(Json(invited));
        }

        [HttpGet("webapi/getinvitableemployees/{departmentId}/{businessRoleId}/{trainingModuleId}")]
        public IActionResult GetInvitableEmployees(int departmentId, int businessRoleId, int trainingModuleId)
        {
            try
            {
                var query = new EmployeesQuery
                {
                    DepartmentId = departmentId,
                    BusinessRoleId = businessRoleId
                };

                var invitableEmployees = _employeeService.GetInvitableEmployees(departmentId, businessRoleId, trainingModuleId);

                return Ok(Json(invitableEmployees));
            }
            catch (Exception ex)
            {

            }

            return BadRequest();
        }

        [HttpPost("webapi/edittrainingmodule")]
        public IActionResult EditTrainingModule([FromBody]TrainingModuleAPIModel model)
        {
            if (ModelState.IsValid)
            {
                string trainingModulePath = "";
                if (string.IsNullOrEmpty(model.TrainingMaterialsFolderPath))
                    trainingModulePath = Path.Combine(_env.WebRootPath, "training_module", Guid.NewGuid().ToString());
                else
                    trainingModulePath = model.TrainingMaterialsFolderPath;

                var filePaths = HttpContext.Session.GetCollection<string>(User.Identity.Name + GlobalConstants.SESSIONKEYSUFFIX_TRAININGMODULE);
                if (filePaths != null && filePaths.Count > 0)
                {
                    model.TrainingMaterialsFolderPath = trainingModulePath;
                    foreach (var path in filePaths)
                    {
                        var file = PhysicalFile(path, "image/jpeg");
                        byte[] bytes = System.IO.File.ReadAllBytes(path);
                        FileContentResult img = null;

                        using (var r = new FileStream(path, FileMode.Create))
                        {
                            img = File(bytes, "image/jpeg", file.FileName);
                        }

                        var index = img.FileDownloadName.LastIndexOf('\\');

                        var filename = img.FileDownloadName.Substring(index + 1);

                        var trainingMaterialPath = Path.Combine(trainingModulePath, filename);
                        if (!Directory.Exists(trainingModulePath))
                            Directory.CreateDirectory(trainingModulePath);

                        if (!System.IO.File.Exists(trainingMaterialPath))
                            using (var filestream = new FileStream(trainingMaterialPath, FileMode.CreateNew))
                            {
                                filestream.Write(bytes, 0, bytes.Length);
                            }
                    }
                }

                HttpContext.Session.Remove(User.Identity.Name + GlobalConstants.SESSIONKEYSUFFIX_TRAININGMODULE);

                var module = Mapper.Map<TrainingModule>(model);
                module.DateCreated = DateTime.Now;

                _resourcesBLL.Update(module);

                return Ok(Json(model));
            }

            var errorMesgs = ModelState.RetrieveModelStateErrorMessages();

            return BadRequest(errorMesgs);

        }

        [HttpPost("webapi/removesession")]
        public IActionResult RemoveSession()
        {
            HttpContext.Session.Remove(User.Identity.Name + GlobalConstants.SESSIONKEYSUFFIX_TRAININGMODULE);

            return Ok();
        }

        [HttpPost("webapi/deleteimage")]
        public IActionResult RemoveFile([FromBody]FilePath filepath)
        {
            var fullpath = filepath.FolderPath + "\\" + filepath.Filename;
            var exists = System.IO.File.Exists(fullpath);
            if (exists)
            {
                System.IO.File.Delete(fullpath);
            }

            return Ok(Json(new { filepath, exists }));
        }

        [HttpPost("webapi/insertinvitation")]
        public IActionResult InsertInvitation([FromBody]InvitationAPIModel invitation)
        {
            _resourcesBLL.InsertInvitation(invitation.EmployeeId, invitation.TrainingModuleId);

            return Ok(Json(new { created = true }));
        }

        [HttpPost("webapi/deleteinvitation")]
        public IActionResult DeleteInvitation([FromBody]InvitationAPIModel invitation)
        {
            _resourcesBLL.DeleteInvitation(invitation.EmployeeId, invitation.TrainingModuleId);

            return Ok(Json(new { deleted = true }));
        }

        [HttpPost("webapi/trainingmodules/{employeeId}")]
        public IActionResult GetInvitations(int employeeId)
        {
            var invitations = _resourcesBLL.GetAllTrainingModules(employeeId, (int)RequestStatus.Accepted);

            return Ok(Json(invitations));
        }

        #endregion TRAINING MODULES

        #region ANNOUNCEMENTS

        [HttpGet("webapi/getannouncement/{announcementId}")]
        public IActionResult GetAnnouncements(int announcementId)
        {
            var announcement = _resourcesBLL.GetAnnouncement(announcementId);

            return Ok(Json(new { announcement = announcement, get = true }));
        }

        [HttpGet("webapi/getannouncements")]
        public IActionResult GetAnnouncements()
        {
            var announcements = _resourcesBLL.GetAllAnnouncements();

            return Ok(Json(announcements));
        }

        [HttpPost("webapi/editannouncement")]
        public IActionResult EditAnnouncement([FromBody]AnnouncementAPIModel model)
        {
            if (ModelState.IsValid)
            {
                _resourcesBLL.Update(model);

                return Ok(Json(new { model = model, edited = true }));
            }

            return BadRequest();
        }

        [HttpPost("webapi/insertannouncement")]
        public IActionResult InsertAnnouncement([FromBody]AnnouncementAPIModel model)
        {
            if (ModelState.IsValid)
            {
                _resourcesBLL.Insert(model);

                return Ok(Json(new { model = model, created = true }));
            }

            return BadRequest();
        }

        // TODO: At the moment is only retrieving all the recipients based on a list of businessroles Ids and a list of departments Ids
        [HttpPost("webapi/addrecipients")]
        public IActionResult AddRecipients([FromBody]AnnouncementRequest query)
        {
            var recipients = _employeeService.GetAll(query);

            return Ok(Json(recipients));
        }

        #endregion END ANNOUNCEMENTS

        #region LEAVE TYPES SETTINGS

        //[HttpGet("webapi/getdefaultleavetype")]
        //public IActionResult GetDefaultLeaveType()
        //{
        //    var defaultLeaveType = _leaveTypeService.GetDefaultLeaveType();

        //    return Json(defaultLeaveType);
        //}

        //[HttpGet("webapi/getleavesettings")]
        //public IActionResult GetLeaveSettings()
        //{
        //    GeneralSettingsViewModel json;
        //    using (StreamReader r = new StreamReader(GENERALSETTINGS_FILEPATH))
        //    {
        //        var file = r.ReadToEnd();
        //        json = JsonConvert.DeserializeObject<GeneralSettingsViewModel>(file);
        //    }

        //    return Json(json.LeaveSettings);
        //}

        //[HttpPost("webapi/editleavesettings")]
        //public IActionResult EditLeaveSettings([FromBody]EditLeaveSettingsViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        GeneralSettingsViewModel json;
        //        using (StreamReader r = new StreamReader(GENERALSETTINGS_FILEPATH))
        //        {
        //            var file = r.ReadToEnd();
        //            json = JsonConvert.DeserializeObject<GeneralSettingsViewModel>(file);
        //        }
        //        json.LeaveSettings = model.LeaveSettings;
        //        if (model.DefaultLeaveType.Id == 0)
        //            json.DefaultLeaveType = model.DefaultLeaveType;

        //        System.IO.File.WriteAllText(GENERALSETTINGS_FILEPATH, JsonConvert.SerializeObject(json, Formatting.Indented));
        //        return Ok();
        //    }

        //    return BadRequest();
        //}

        [HttpPost("webapi/createleavetype")]
        public IActionResult CreateLeaveType([FromBody]LeaveType toCreate)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        _leaveTypeService.InsertLeaveType(toCreate);

                        return Created($"/webapi/createleavetype/", toCreate);
                    }
                }
                catch (Exception ex)
                {

                }

                return BadRequest();
            }

            return Unauthorized();
        }

        [HttpDelete("webapi/deleteleavetype")]
        public IActionResult DeleteLeaveType(int leaveTypeId)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var leaveType = _leaveTypeService.GetLeaveTypeById(leaveTypeId);
                        if (leaveType != null)
                        {
                            _leaveTypeService.DeleteLeaveType(leaveType.Id);
                            return Ok();
                        }
                    }
                }
                catch (Exception ex)
                {

                }

                return BadRequest();
            }

            return Unauthorized();
        }

        [HttpPost("webapi/editleavetypes")]
        public IActionResult EditLeaveTypes([FromBody]List<LeaveType> leaveTypes)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        foreach (var leaveType in leaveTypes)
                        {
                            _leaveTypeService.UpdateLeaveType(leaveType);
                        }

                        return Ok();
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return Unauthorized();
        }

        #endregion END LEAVE TYPES SETTINGS

        [HttpPost("webapi/audittrail")]
        public IActionResult GetAuditTrail([FromBody] PageListRequest request)
        {
            //var auditTrails = _auditTrailBLL.Get(request.PageIndex, request.PageSize);
            //return Ok(Json(auditTrails));
            return Ok();
        }

        #region USERS

        [HttpGet("webapi/getusers")]        
        public IActionResult GetUsers()
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    return Ok(Json(_userManager.Users.ToList()));
                }
                catch (Exception ex)
                {

                }

                return BadRequest();

            }

            return Unauthorized();
        }

        [HttpGet("webapi/getuser")]
        public IActionResult GetUser(string id)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var user = _userManager.Users.FirstOrDefault(u => string.Equals(u.Id, id, StringComparison.InvariantCultureIgnoreCase));

                    if (user != null)
                        return Ok(Json(user));
                }
                catch (Exception ex)
                {

                }

                return BadRequest();
            }

            return Unauthorized();
        }

        #endregion END USERS

        #region EMPLOYEEINFO

        [HttpGet("webapi/getemployee")]
        public IActionResult GetEmployee(string userId)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var employee = _employeeService.GetEmployeeByApplicationUserId(userId);

                    return Ok(Json(employee));
                }
                catch (Exception ex)
                {

                }

                return BadRequest();

            }

            return Unauthorized();
        }

        [HttpGet("webapi/getemployees")]
        public IActionResult GetEmployees()
        {
            //if (User.Identity.IsAuthenticated)
            //{
            try
            {
                var employees = _employeeService.GetAll().ToList();

                return Ok(Json(employees));
            }
            catch (Exception ex)
            {

            }

            return BadRequest();

            //}

            //return Unauthorized();
        }

        //[HttpPost("webapi/getemployees")]
        //public IActionResult GetEmployees([FromBody] EmployeesQuery query)
        //{
        //    //if (User.Identity.IsAuthenticated)
        //    //{
        //    try
        //    {
        //        var employees = _employeeService.GetAll(query.DepartmentId, query.BusinessRoleId);

        //        return Ok(Json(employees));
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    return BadRequest();

        //    //}

        //    //return Unauthorized();
        //}

        [HttpGet("webapi/GetEmployeeByUserId")]
        public async Task<IActionResult> GetEmployeeByUserId(string id)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var employee = await _employeeService.PrepareUserToEdit(id);

                    return Ok(Json(employee));
                }
                catch (Exception ex)
                {

                }

                return BadRequest();

            }

            return Unauthorized();
        }

        [HttpGet("webapi/GetCurrentEmployee")]
        public IActionResult GetCurrentEmployee()
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    var employee = _employeeService.GetAll().FirstOrDefault(e => string.Equals(e.UserId, userId, StringComparison.InvariantCultureIgnoreCase));

                    return Ok(Json(employee));
                }
                catch (Exception ex)
                {

                }

                return BadRequest();

            }

            return Unauthorized();
        }

        [HttpGet("webapi/currentuser")]
        public IActionResult CurrentUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    var employee = _employeeService.GetAsAPIModel(userId);

                    return Ok(Json(employee));
                }
                catch (Exception ex)
                {

                }

                return BadRequest();

            }

            return Unauthorized();
        }

        //[HttpPost("webapi/createuser")]
        //[CustomAuthorize("Add / edit users")]
        //public async Task<IActionResult> CreateUser([FromBody]EditEmployeeInfoAPIModel model)
        //{
        //    var result = new OperationResult<ApplicationUser>();
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            result = await _employeeService.ValidateAndCreateUserFromModel(model);

        //            if (result.Succeeded)
        //                return Created($"/webapi/createuser/", result.Object);
        //        }
        //        catch (Exception ex)
        //        {
        //            await _userManager.DeleteAsync(model.User);
        //        }
        //    }

        //    ModelState.AddErrors(result.ErrorMessages);
        //    var errorMessages = ModelState.RetrieveModelStateErrorMessages();

        //    return BadRequest(Json(errorMessages));
        //}

        //[HttpPost("webapi/edituser")]
        //[CustomAuthorize("Add / edit users")]
        //public async Task<IActionResult> EditUser([FromBody]EditEmployeeInfoAPIModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var result = await _employeeService.EditEmployee(model);
        //            if (result.Succeeded)
        //                return Ok();
        //            else
        //                return BadRequest(Json(result.ErrorMessages
        //                                .Select(e => e.Value)
        //                                .ToList()));

        //        }
        //        catch (Exception ex)
        //        {
        //            return BadRequest();
        //        }
        //    }

        //    var errors = ModelState.RetrieveModelStateErrorMessages();

        //    return BadRequest(Json(errors));
        //}

        //[HttpPost("webapi/user-manage/edituser")]        
        //public async Task<IActionResult> UpdateUser([FromBody]EditUserViewModel model)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    user.Email = model.Email;
        //    user.FirstName = model.FirstName;
        //    user.LastName = model.LastName;
        //    user.Gender = model.Gender;
        //    user.PhoneNumber = model.PhoneNumber;
        //    var result = await _userManager.UpdateAsync(user);
        //    if (model.Password != null && string.Equals(model.Password, model.RepeatPassword))
        //    {
        //        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //        var pswResetResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
        //    }

        //    return Ok(Json(result));
        //}

        #endregion END EMPLOYEEINFO

        #region LEAVE REQUESTS

        [HttpGet("webapi/user-manage/GetLeaveRequests")]
        public IActionResult GetLeaveRequestHistory()
        {
            var userId = _userManager.GetUserId(User);
            var employee = _employeeService.GetEmployeeByApplicationUserId(userId);
            var total = 0;

            var requests = _leaveService.GetLeaveHistory(employee.Id, out total);

            return Ok(Json(requests));
        }

        //[HttpPost("webapi/insertleaverequest")]
        //[CustomAuthorize("Apply leave request")]        
        //public IActionResult InsertLeaveRequest([FromBody]LeaveRequestViewModel leaveRequest)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            LeaveRequest leave = Mapper.Map<LeaveRequest>(leaveRequest);
        //            var userId = _userManager.GetUserId(User);
        //            var employee = _employeeService.GetEmployeeByApplicationUserId(userId);

        //            leave.EmployeeId = employee.Id;
        //            leave.DateCreated = DateTime.Now;

        //            // Create Leave Request
        //            var result = _leaveService.Insert(leave);
        //            if (result.Succeeded)
        //                _employeeService.OnLeaveRequestInsertSuccess(result.Object, result.Object.EmployeeId);

        //            return Created($"/webapi/insertleaverequest/", leaveRequest);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var err = new ErrorLog(ex.Message, "InnerException");
        //        _errorService.Insert(err);
        //    }

        //    return BadRequest();
        //}

        [HttpGet("webapi/approver-manage/GetPendingLeaveRequests")]
        //[CustomAuthorize("View pending leave requests")]
        public IActionResult GetPendingLeaveRequests()
        {
            var userId = _userManager.GetUserId(User);
            var approverId = _employeeService.GetEmployeeIdOnApplicationUserId(userId);
            //var requests = _employeeService.GetPendingLeaveRequestsOnApproverId(approverId);
            //return Ok(Json(requests));
            return Ok();
        }
                
        //[HttpPost("webapi/approver-manage/updateLeaveRequest")]
        //[CustomAuthorize("Approve / disapprove leave requests")]
        //public IActionResult UpdateLeaveRequest([FromBody]PendingLeaveRequest leaveRequest)
        //{
        //    OperationResult<LeaveRequest> validateLeave = null;
        //    try
        //    {
        //        var approverId = _userManager.GetUserId(User);
        //        validateLeave = _leaveService.Update(leaveRequest, approverId);
        //        if (validateLeave.Succeeded)
        //            return Ok(Json(validateLeave));
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    if (validateLeave != null)
        //        return BadRequest(Json(validateLeave));

        //    return BadRequest();
        //}

        [HttpGet("webapi/approver-manage/GetRejectedLeaveRequests/{departmentId}")]
        [CustomAuthorize("Approve / disapprove leave requests")]
        public IActionResult GetRejectedLeaveRequests(int? departmentId)
        {
            var userId = _userManager.GetUserId(User);
            var approverId = _employeeService.GetEmployeeIdOnApplicationUserId(userId);

            if (departmentId == 0)
            {
                //var requests = _employeeService.GetRejectedLeaveRequests(approverId, null);
                //return Ok(Json(requests));
                return Ok();
            }
            else
            {
                //var requests = _employeeService.GetRejectedLeaveRequests(approverId, departmentId);
                //return Ok(Json(requests));
                return Ok();
            }
        }

        [HttpGet("webapi/getacceptedleave")]
        public IActionResult GetAcceptedLeave()
        {
            var userId = _userManager.GetUserId(User);
            var employeeId = _employeeService.GetEmployeeIdOnApplicationUserId(userId);
            //var requests = _leaveService.GetAllByEmployeeId(employeeId, (int)RequestStatus.Accepted);
            //return Ok(Json(requests));
            return Ok();
        }

        #endregion LEAVE REQUESTS

        #region TIMESHEET

        [HttpPost("webapi/gettimesheet")]
        public IActionResult GetTimesheet([FromBody]RotaRequestViewModel request)
        {
            var timesheet = _employeeService.GetTimesheet(request.DepartmentId, request.BusinessRoleId, request.FromDate);

            return Ok(Json(timesheet));
        }

        [HttpPost("webapi/approvetimesheet")]
        [CustomAuthorize("Approve timesheet")]
        public IActionResult ApproveTimesheet([FromBody]IList<TimesheetDetails> timeclockToEdit)
        {
            var result = _employeeService.ApproveTimesheet(timeclockToEdit);
            if (result)
                return Ok(Json(timeclockToEdit));
            else
                return BadRequest(Json(false));
        }

        [HttpPost("webapi/edittimestamp")]
        public IActionResult EditTimestamp([FromBody]TimeclockTimestamp timestamp)
        {
            _employeeService.UpdateTimeclockTimestamp(timestamp);

            return Ok(Json(new { edited = timestamp }));
        }

        #endregion END TIMESHEET

        #region ACCOUNTANT REPORTS

        // StartDate and EndDate only accept one format YYYYMMDD
        [HttpGet("webapi/accountantreport/{employeeId}/start_date/{startDate}/end_date/{endDate}/pageindex/{pageIndex}/pagesize/{pageSize}")]
        public IActionResult GetAccountantReport(int employeeId, string startDate, string endDate, int pageIndex, int pageSize)
        {

            var provider = CultureInfo.InvariantCulture;
            var format = "yyyyMMdd";

            DateTime from = DateTime.ParseExact(startDate, format, provider);
            DateTime to = DateTime.ParseExact(endDate, format, provider);

            var result = _employeeService.GetAccountantReports(pageIndex, pageSize, from, to);

            if (result.Succeeded)
                return Ok(Json(result.Object));
            else
                return BadRequest();
                //return BadRequest(Json(result.ErrorMessagesToList()));
        }

        #endregion ACCOUNTANT REPORTS

        //[HttpPost("webapi/GetCalendarCoded")]
        //public IActionResult GetCalendarCoded([FromBody]CalendarRequest request)
        //{
        //    var calendar = _employeeService.GetCalendarDays(request.EmployeeId, request.FromDate, request.ToDate);

        //    return Ok(Json(calendar));
        //}

        // TODO: Authorize Business Roles with proper Permisison!!
        [HttpPost("webapi/shift-edit/weeklyshift")]
        public IActionResult GetWeeklyShift([FromBody]RotaRequestViewModel request)
        {
            //TODO: move to business Logic in EmployeesInfoBLL (always get weekly rota starting from Monday) 
            int daysToMonday = System.DayOfWeek.Monday - request.FromDate.DayOfWeek;

            var date = request.FromDate.Date.AddDays(daysToMonday);
            if (daysToMonday == 1)
                date = date.Date.AddDays(-7);

            var weeklyShift = new WeeklyShiftAPIModel();

            if (request.BusinessRoleId == 0)
                weeklyShift = _employeeService.GetWeeklyShiftWithAssignments(request.DepartmentId, date);
            else
                weeklyShift = _employeeService.GetWeeklyShiftWithAssignments(request.DepartmentId, request.BusinessRoleId, date);

            return Ok(Json(weeklyShift));
        }

        [HttpPost("webapi/shift-view/userweeklyshift")]
        public IActionResult GetUserWeeklyShift([FromBody]RotaRequestViewModel request)
        {
            var userId = _userManager.GetUserId(User);
            var employee = _employeeService.GetEmployeeByApplicationUserId(userId);

            //TODO: move to business Logic in EmployeesInfoBLL (always get weekly rota starting from Monday) 
            int daysToMonday = System.DayOfWeek.Monday - request.FromDate.DayOfWeek;
            var date = request.FromDate.AddDays(daysToMonday);

            var weeklyShift = new WeeklyShiftAPIModel();

            weeklyShift = _employeeService.GetWeeklyShiftWithAssignments(request.DepartmentId, request.BusinessRoleId, employee.Id, date);

            return Ok(Json(weeklyShift));
        }

        [HttpPost("webapi/shift-edit/insertassignment")]
        public IActionResult InsertNewAssignment([FromBody]AssignmentAPIModel assignment)
        {
            var result = new OperationResult<AssignmentAPIModel>();
            if (ModelState.IsValid)
            {
                _employeeService.InsertAssignment(assignment);

                return Ok(Json(assignment));
            }

            ModelState.AddErrors(result.ErrorMessages);
            var errorMessages = ModelState.RetrieveModelStateErrorMessages();

            return BadRequest(errorMessages);
        }

        [HttpPost("webapi/shit-edit/editweek")]
        public IActionResult EditWeek([FromBody]AssignmentAPIModel assignment)
        {
            _employeeService.InsertWeekAssignment(assignment.EmployeeId, assignment.DepartmentId,
                assignment.BusinessRoleId, assignment.RecurringWeeks, assignment.StartDate);

            return Ok(Json(new { API_Respones = new { assignment = assignment } }));
        }

        [HttpGet("webapi/getassignments/{employeeId}/{fromDate}")]
        public IActionResult GetAssignments(int employeeId, string fromDate)
        {
            DateTime date = DateTime.MinValue;
            var dateParsing = DateTime.TryParse(fromDate, out date);
            if (dateParsing)
            {
                var assignments = _employeeService.GetAssignments(employeeId, date);

                return Ok(Json(assignments));
            }

            return BadRequest();
        }

        //// TODO: Move to Business Logic
        //public DepartmentViewModel PrepareDepartment(Department department)
        //{
        //    DepartmentViewModel model = Mapper.Map<DepartmentViewModel>(department);
        //    model.BusinessRoles = new List<CheckableBusinessRole>();
        //    var businessRoles = _businessRoleBLL.GetAll();
        //    foreach (var role in businessRoles)
        //    {
        //        CheckableBusinessRole chkRole = Mapper.Map<CheckableBusinessRole>(role);
        //        if (department.DepartmentBusinessRoles.Any(r => r.Id == role.Id))
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

        //// TODO: Move to Business Logic
        //public IList<SecondaryBusinessRoleAPIModel> PrepareSecondaryBusinessRoles(int employeeId = -1)
        //{
        //    var businessRoles = _businessRoleBLL.GetAll();
        //    IEnumerable<Employee_BusinessRole> userRoles = new List<Employee_BusinessRole>();
        //    if (employeeId > 0)
        //        userRoles = _applicationUser_BusinessRoleBLL.GetAll().Where(ubr => ubr.EmployeeId == employeeId);

        //    List<SecondaryBusinessRoleAPIModel> model = new List<SecondaryBusinessRoleAPIModel>();
        //    foreach (var br in businessRoles)
        //    {
        //        SecondaryBusinessRoleAPIModel chkRole = Mapper.Map<SecondaryBusinessRoleAPIModel>(br);
        //        if (employeeId > 0 && userRoles.Any(ur => ur.BusinessRoleId == br.Id))
        //            chkRole.IsChecked = true;
        //        model.Add(chkRole);
        //    }

        //    return model;
        //}

        //[HttpPost("webapi/returnobj")]
        //public IActionResult ReturnObject([FromBody]object model)
        //{
        //    DateTime date = DateTime.Now;

        //    TrainingModuleAPIModel mod = model as TrainingModuleAPIModel;
        //    if (mod == null)
        //        return Ok(Json(model));
        //    else
        //        return Ok(Json(new { error = "Not TrainingModuel model." }));
        //}
    }
}