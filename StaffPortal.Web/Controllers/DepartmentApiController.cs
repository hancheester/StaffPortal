using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StaffPortal.Common;
using StaffPortal.Service.Departments;
using StaffPortal.Service.Roles;
using StaffPortal.Web.Extensions;
using StaffPortal.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Web.Controllers
{
    [Route("api/department/v1")]
    public class DepartmentApiController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly IBusinessRoleService _businessRoleService;
        private readonly IMapper _mapper;

        public DepartmentApiController(
            IDepartmentService departmentService,
            IBusinessRoleService businessRoleService,
            IMapper mapper)
        {
            _departmentService = departmentService;
            _businessRoleService = businessRoleService;
            _mapper = mapper;
        }

        [HttpGet("my-departments")]
        public async Task<IActionResult> MyDepartments()
        {
            var employeeId = User.Claims.GetEmployeeId();
            var departments = await _departmentService.GetAssignedDepartmentsByEmployeeIdAsync(employeeId);

            return Ok(Json(departments));
        }

        [HttpGet("departments")]
        public async Task<IActionResult> Departments()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();

            return Ok(Json(departments));
        }

        [HttpDelete("department/{id}")]
        public IActionResult DeleteDepartment(int id)
        {
            var result = _departmentService.DeleteDepartment(id);

            if (result.Succeeded)
                return Ok();
                //return Accepted(); //For async, it should be 202

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpGet("department/{id}")]
        public IActionResult Department(int id)
        {
            var department = _departmentService.GetDepartmentById(id);

            if (department == null) return StatusCode(StatusCodes.Status500InternalServerError);

            var model = _mapper.Map<DepartmentModel>(department);
            var roles = _businessRoleService.GetAll();

            var roleModels = new List<DepartmentBusinessRoleModel>();
            foreach (var role in roles)
            {
                var roleModel = new DepartmentBusinessRoleModel
                {
                    RoleId = role.Id,
                    Name = role.Name,
                    ShowOnRota = department.DepartmentBusinessRoles.Where(x => x.BusinessRoleId == role.Id).Any(),
                    MinimumRequired = department.DepartmentBusinessRoles.Where(x => x.BusinessRoleId == role.Id).Select(x => x.MinimumRequired).FirstOrDefault()
                };

                roleModels.Add(roleModel);
            }

            model.Roles = roleModels;

            return Ok(Json(model));
        }
        
        [HttpPost("department")]
        public IActionResult Department([FromBody]DepartmentModel model)
        {
            if (ModelState.IsValid)
            {
                var department = _mapper.Map<Department>(model);
                var result = _departmentService.AddDepartment(department);

                if (result.Succeeded)
                {
                    model.Id = result.Object.Id;
                    return Created($"/api/department/v1/department/{model.Id}", model);
                }

                return BadRequest(new { message = result.ErrorSummary });
            }

            var errors = ModelState.Values.SelectMany(e => e.Errors.Select(er => er.ErrorMessage)).ToArray();
            var message = string.Join(Environment.NewLine, errors);

            return BadRequest(Json(new
            {
                message
            }));
        }

        [HttpPut("department/{id}")]
        public IActionResult UpdateDepartment(int id, [FromBody]DepartmentModel model)
        {
            if (ModelState.IsValid)
            {
                var department = _mapper.Map<DepartmentModel, Department>(model, opt =>
                {
                    opt.AfterMap((src, dest) =>
                    {
                        foreach (var role in dest.DepartmentBusinessRoles)
                        {
                            role.DepartmentId = id;
                        } 
                    });
                });

                var result = _departmentService.UpdateDepartment(department);

                if (result.Succeeded)
                    return Ok();

                return BadRequest(new { message = result.ErrorSummary });
            }

            var errors = ModelState.Values.SelectMany(e => e.Errors.Select(er => er.ErrorMessage)).ToArray();
            var message = string.Join(Environment.NewLine, errors);

            return BadRequest(Json(new
            {
                message
            }));
        }
    }
}