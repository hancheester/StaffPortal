using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StaffPortal.Common;
using StaffPortal.Service.Permissions;
using StaffPortal.Service.Roles;
using StaffPortal.Web.Extensions;
using StaffPortal.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Web.Controllers
{
    [Route("api/role/v1")]
    public class RoleApiController : Controller
    {
        private readonly IBusinessRoleService _businessRoleService;
        private readonly IPermissionService _permissionService;
        private readonly IMapper _mapper;

        public RoleApiController(
            IBusinessRoleService businessRoleService,
            IPermissionService permissionService,
            IMapper mapper)
        {
            _businessRoleService = businessRoleService;
            _permissionService = permissionService;
            _mapper = mapper;
        }

        [HttpGet("my-roles")]
        public async Task<IActionResult> MyRoles()
        {
            var employeeId = User.Claims.GetEmployeeId();
            var result = await _businessRoleService.GetBusinessRolesByEmployeeIdAsync(employeeId);

            if (result.Succeeded)
            {
                var models = _mapper.Map<IList<BusinessRoleModel>>(result.Object);
                return Ok(Json(models));
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpGet("roles/employee/{id}")]
        public async Task<IActionResult> Roles(int id)
        {            
            var result = await _businessRoleService.GetBusinessRolesByEmployeeIdAsync(id);

            if (result.Succeeded)
            {
                var models = _mapper.Map<IList<BusinessRoleModel>>(result.Object);
                return Ok(Json(models));
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpGet("role/{id}")]
        public IActionResult Role(int id)
        {
            var role = _businessRoleService.GetBusinessRoleById(id);
            var model = _mapper.Map<BusinessRoleModel>(role);

            return Ok(Json(model));
        }

        [HttpGet("roles")]
        public IActionResult Roles()
        {
            var roles = _businessRoleService.GetAll();
            var models = _mapper.Map<IList<BusinessRoleModel>>(roles);

            return Ok(Json(models));
        }

        [HttpGet("role-tree")]
        public IActionResult RoleTree()
        {
            var tree = _businessRoleService.GetRoleHierarchy();
            var model = _mapper.Map<IList<RoleTreeNodeModel>>(tree);

            return Ok(Json(model));
        }

        [HttpPost("role")]
        public IActionResult Role([FromBody]BusinessRoleModel model)
        {
            if (ModelState.IsValid)
            {
                var role = _mapper.Map<BusinessRole>(model);
                var result = _businessRoleService.AddBusinessRole(role);

                if (result.Succeeded)
                {
                    model.Id = result.Object.Id;
                    return Created($"/api/role/v1/role/{model.Id}", model);
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

        [HttpPut("role/{id}")]
        public IActionResult UpdateRole([FromBody]BusinessRoleModel model)
        {
            if (ModelState.IsValid)
            {
                var role = _mapper.Map<BusinessRoleModel, BusinessRole>(model, opt =>
                {
                    opt.AfterMap((src, dest) =>
                    {
                        dest.Id = src.Id;
                    });
                });

                var result = _businessRoleService.UpdateBusinessRole(role);

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

        [HttpPut("role/{id}/allow-permission")]
        public IActionResult AllowPermission(int id, [FromBody]int permissionId)
        {
            var result = _permissionService.AllowPermission(permissionId, id);

            if (result.Succeeded) return Ok();

            if (result.Succeeded)
                return Ok();

            return BadRequest(new { message = result.ErrorSummary });
        }

        [HttpPut("role/{id}/disallow-permission")]
        public IActionResult DisallowPermission(int id, [FromBody]int permissionId)
        {
            var result = _permissionService.DisallowPermission(permissionId, id);

            if (result.Succeeded) return Ok();

            if (result.Succeeded)
                return Ok();

            return BadRequest(new { message = result.ErrorSummary });
        }

        [HttpDelete("role/{id}")]
        public IActionResult DeleteRole(int id)
        {
            var result = _businessRoleService.DeleteRole(id);

            if (result.Succeeded)
                return Ok();
            //return Accepted(); //For async, it should be 202

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}