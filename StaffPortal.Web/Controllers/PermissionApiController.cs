using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StaffPortal.Service.Permissions;
using StaffPortal.Web.Models;
using System.Collections.Generic;

namespace StaffPortal.Web.Controllers
{
    [Route("api/permission/v1")]
    public class PermissionApiController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly IMapper _mapper;

        public PermissionApiController(
            IPermissionService permissionService,
            IMapper mapper)
        {
            _permissionService = permissionService;
            _mapper = mapper;
        }

        [HttpGet("permissions")]
        public IActionResult Permissions()
        {
            var items = _permissionService.GetAll();
            var models = _mapper.Map<IList<PermissionModel>>(items);

            return Ok(Json(models));
        }
    }
}