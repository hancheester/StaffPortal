using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StaffPortal.Service.Errors;
using StaffPortal.Service.Leave;
using StaffPortal.Service.Staff;
using StaffPortal.Common;
using StaffPortal.Common.Settings;
using StaffPortal.Web.Extensions;
using StaffPortal.Web.Infrastructure;
using StaffPortal.Web.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StaffPortal.Web.Controllers
{
    [Authorize]
    [Route("api/leave/v1")]
    public class LeaveApiController : Controller
    {
        private readonly ILeaveService _leaveService;
        private readonly IEmployeeService _employeeService;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IErrorService _errorService;
        private readonly IMapper _mapper;
        private readonly LeaveSettings _leaveSettings;

        public LeaveApiController(
            ILeaveService leaveService,
            IEmployeeService employeeService,
            ILeaveTypeService leaveTypeService,
            IErrorService errorService,
            IOptions<LeaveSettings> leaveSettings,
            IMapper mapper)
        {
            _leaveService = leaveService;
            _employeeService = employeeService;
            _leaveTypeService = leaveTypeService;
            _errorService = errorService;
            _mapper = mapper;
            _leaveSettings = leaveSettings.Value;
        }

        [HttpGet("leave-types")]
        public IActionResult LeaveTypes()
        {
            var leaveTypes = _leaveTypeService.GetAllLeaveTypes();
            return Ok(Json(leaveTypes));
        }

        [HttpPost("leave-type")]
        public IActionResult LeaveType([FromBody]LeaveType model)
        {
            var result = _leaveTypeService.InsertLeaveType(model);

            if (result.Succeeded)
            {
                model.Id = result.Object.Id;
                return Created($"/api/leave/v1/leave-type/{model.Id}", model);
            }

            return BadRequest(new { message = result.ErrorSummary });
        }

        [HttpDelete("leave-type/{id}")]
        public IActionResult LeaveType(int id)
        {
            var result = _leaveTypeService.DeleteLeaveType(id);

            if (result.Succeeded)
                return Ok();
            //return Accepted(); //For async, it should be 202

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpGet("calendar-leave-settings")]
        public IActionResult LeaveSettings()
        {
            return Ok(Json(new
            {
                _leaveSettings.LeaveNoticePeriod,
                _leaveSettings.EmergencyAllowance
            }));
        }

        [HttpGet("requestable-leave-types")]
        public IActionResult RequestableLeaveTypes()
        {
            var leaveTypes = _leaveTypeService.GetAllRequestable();
            return Ok(Json(leaveTypes));
        }

        [HttpGet("leave-quota")]
        public IActionResult LeaveQuota()
        {
            var employeeId = User.Claims.GetEmployeeId();
            var quota = _leaveService.GetLeaveQuota(employeeId);

            var model = new LeaveQuotaModel
            {
                Total = quota.Total,
                AccruedAsDay = quota.AccruedAsDay,
                AccruedAsPay = quota.AccruedAsPay,
                Remaining = quota.Remaining,
                Used = quota.Used,
                Pending = quota.Pending,
                NoImpact = quota.NoImpact
            };

            return Ok(Json(model));
        }

        [HttpGet("personal-calendar")]
        public IActionResult PersonalCalendar(string fromDate, string toDate)
        {
            var employeeId = User.Claims.GetEmployeeId();
            var from = DateTime.ParseExact(fromDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var to = DateTime.ParseExact(toDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var calendar = _leaveService.GetPersonalCalendar(employeeId, from, to);

            return Ok(Json(calendar));
        }

        [HttpGet("departmental-calendar/{departmentId}")]
        public IActionResult DepartmentalCalendar(int departmentId, string fromDate, string toDate)
        {
            var from = DateTime.ParseExact(fromDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var to = DateTime.ParseExact(toDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var calendar = _leaveService.GetDepartmentalCalendar(departmentId, from, to);

            return Ok(Json(calendar));
        }

        [HttpPost("leave-request")]
        [CustomAuthorize("Apply leave request")]
        public IActionResult CreateLeaveRequest([FromBody]CreateLeaveRequestModel model)
        {
            if (ModelState.IsValid)
            {
                var employeeId = User.Claims.GetEmployeeId();
                var leaveRequest = _mapper.Map<CreateLeaveRequestModel, LeaveRequest>(model, opt => {
                    opt.AfterMap((src, dest) => {
                        dest.EmployeeId = employeeId;
                        dest.DateCreated = DateTime.Now;
                    });
                });
                
                var result = _leaveService.AddLeaveRequest(leaveRequest);

                if (result.Succeeded)
                {
                    model.Id = result.Object.Id;
                    return Created($"/api/leave/v1/leave-request/{model.Id}", model);
                }

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return BadRequest();
        }

        [HttpPut("leave-request/{id}/approve")]
        [CustomAuthorize("Approve / disapprove leave requests")]
        public IActionResult ApproveLeaveRequest(int id)
        {
            var applicationUserId = User.Claims.GetApplicationUserId();
            var result = _leaveService.ApproveLeaveRequest(applicationUserId, id);

            if (result.Succeeded) return Ok();
           
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPut("leave-request/{id}/reject")]
        [CustomAuthorize("Approve / disapprove leave requests")]
        public IActionResult RejectLeaveRequest(int id)
        {
            //Refer: https://weblog.west-wind.com/posts/2017/Sep/14/Accepting-Raw-Request-Body-Content-in-ASPNET-Core-API-Controllers
            var reason = string.Empty;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                reason = reader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(reason)) return BadRequest(Json(new
            {
                code = "000",
                message = "Please provide a reason for rejection."
            }));

            var applicationUserId = User.Claims.GetApplicationUserId();
            var result = _leaveService.DisapproveLeaveRequest(applicationUserId, id, reason);

            if (result.Succeeded) return Ok();

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpGet("leave-request/{id}")]
        public IActionResult LeaveRequest(int id)
        {
            //TODO: To implement get leave request by id.
            //var leaveRequest = _leaveService.GetLeaveRequest(id);
            //return Ok(Json(leaveRequest));

            return Ok();
        }

        [HttpGet("leave-requests")]
        public IActionResult LeaveRequests(int pageNumber = 1, int pageSize = 10)
        {
            var employeeId = User.Claims.GetEmployeeId();
            var requests = _leaveService.GetLeaveHistory(employeeId, out int total, pageNumber, pageSize);
            var models = _mapper.Map<IList<MyLeaveRequestModel>>(requests);

            return Ok(Json(new
            {
                requests = models,
                total
            }));
        }

        [HttpGet("holiday/month/{month}/year/{year}")]
        public IActionResult HolidayByMonth(int month, int year)
        {
            var employeeId = User.Claims.GetEmployeeId();
            var days = _leaveService.GetHolidayDatesByMonth(employeeId, month, year);

            return Ok(Json(days));
        }

        [HttpGet("pending-leave-requests")]
        public async Task<IActionResult> PendingLeaveRequests()
        {
            var applicationUserId = User.Claims.GetApplicationUserId();
            var requests = await _leaveService.GetPendingLeaveRequestsAsync(applicationUserId, 3);
                            
            return Ok(Json(requests));
        }
    }
}