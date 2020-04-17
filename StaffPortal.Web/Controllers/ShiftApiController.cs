using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StaffPortal.Common;
using StaffPortal.Service.Shift;
using StaffPortal.Web.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace StaffPortal.Web.Controllers
{
    [Route("api/shift/v1")]
    public class ShiftApiController : Controller
    {
        private readonly IShiftService _shiftService;
        private readonly IMapper _mapper;

        public ShiftApiController(
            IShiftService shiftService,
            IMapper mapper)
        {
            _shiftService = shiftService;
            _mapper = mapper;
        }

        [HttpGet("rota/department/{departmentId}/role/{roleId}/date/{date}")]
        public IActionResult Rota(int departmentId, int roleId, string date)
        {
            var targetDate = DateTime.ParseExact(date, "ddMMyyyy", CultureInfo.InvariantCulture);
            var rotas = _shiftService.GetRotas(departmentId, roleId, targetDate, DateTime.Now);

            return Ok(Json(rotas));
        }

        [HttpGet("roles/department/{departmentId}/from-date/{fromDate}/to-date/{toDate}")]
        public IActionResult Roles(int departmentId, string fromDate, string toDate)
        {
            var targetFromDate = DateTime.ParseExact(fromDate, "ddMMyyyy", CultureInfo.InvariantCulture);
            var targetToDate = DateTime.ParseExact(toDate, "ddMMyyyy", CultureInfo.InvariantCulture);

            var roles = _shiftService.GetRolesInRota(departmentId, targetFromDate, targetToDate);

            return Ok(Json(roles));
        }

        [HttpGet("roles/{roleId}/department/{departmentId}/from-date/{fromDate}/to-date/{toDate}")]
        public IActionResult Roles(int roleId, int departmentId, string fromDate, string toDate)
        {
            var targetFromDate = DateTime.ParseExact(fromDate, "ddMMyyyy", CultureInfo.InvariantCulture);
            var targetToDate = DateTime.ParseExact(toDate, "ddMMyyyy", CultureInfo.InvariantCulture);

            var roles = _shiftService.GetRolesInRota(departmentId, targetFromDate, targetToDate, roleId);

            return Ok(Json(roles));
        }

        [HttpGet("minimum-staff-status/department/{id}/date/{date}")]
        public async Task<IActionResult> MinimumStaffStatus(int id, string date)
        {
            var targetDate = DateTime.ParseExact(date, "ddMMyyyy", CultureInfo.InvariantCulture);

            var result = await Task.Run(() => _shiftService.GetMinimumStaffStatus(id, targetDate));

            if (result.Succeeded)
                return Ok(Json(result.Object));

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPut("role/department/{departmentId}/employee/{employeeId}")]
        public async Task<IActionResult> ChangeRole(int departmentId, int employeeId, [FromBody]IList<AssignmentModel> models)
        {
            var cleanUpResult = await Task.Run(() => _shiftService.RemoveAssignment(departmentId, employeeId));
            
            if (cleanUpResult.Succeeded == false)
                return StatusCode(StatusCodes.Status500InternalServerError);

            foreach (var model in models)
            {
                var assignment = _mapper.Map<Assignment>(model);
                assignment.CreatedOnDate = DateTime.Now;

                var result = await Task.Run(() => _shiftService.CreateAssignment(assignment));

                if (result.Succeeded == false)
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        [HttpPut("times/employee/{id}")]
        public async Task<IActionResult> ChangeTimes(int id, [FromBody]AssignmentModel model)
        {
            var assignment = _mapper.Map<Assignment>(model);
            assignment.CreatedOnDate = DateTime.Now;

            var result = await Task.Run(() => _shiftService.CreateAssignment(assignment));

            if (result.Succeeded)
                return Ok(Json(result.Object));

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}