using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StaffPortal.Service.Staff;
using StaffPortal.Common;
using StaffPortal.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Web.Controllers
{
    [Authorize]
    [Route("api/user/v1")]
    public class UserApiController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;

        public UserApiController(
            IEmployeeService employeeService,
            IMapper mapper)
        {
            _employeeService = employeeService;
            _mapper = mapper;
        }

        [HttpPost("employee")]
        public async Task<IActionResult> Employee([FromBody]EmployeeModel model)
        {
            if (ModelState.IsValid)
            {
                var employee = _mapper.Map<Employee>(model);
                var username = model.UserName;
                var email = model.Email;
                var password = model.Password;
                var primaryRoleId = model.PrimaryBusinessRoleId;
                var secondaryRoleIds = model.SecondaryBusinessRoleIds;

                var result = await _employeeService.Register(employee, username, email, password, model.PhoneNumber, primaryRoleId, secondaryRoleIds);

                if (result.Succeeded)
                {
                    model.Id = result.Object.Id;
                    return Created($"/api/user/v1/employee/{model.Id}", model);
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

        [HttpGet("employee/{id}")]
        public IActionResult Employee(int id)
        {
            var employee = _employeeService.GetEmployeeById(id);
            var model = _mapper.Map<EmployeeModel>(employee);

            return Ok(Json(model));
        }

        [HttpDelete("employee/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var result = await _employeeService.DeleteAsync(id);

            if (result.Succeeded)
                return Ok();
            //return Accepted(); //For async, it should be 202

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPut("employee/{id}")]
        public async Task<IActionResult> Employee(int id, [FromBody]EmployeeModel model)
        {
            if (ModelState.IsValid)
            {
                var employee = _mapper.Map<EmployeeModel, Employee>(model, opt =>
                {
                    opt.AfterMap((src, dest) =>
                    {
                        dest.Id = id;
                    });
                });

                var username = model.UserName;
                var email = model.Email;
                var password = model.Password;
                var primaryRoleId = model.PrimaryBusinessRoleId;
                var secondaryRoleIds = model.SecondaryBusinessRoleIds;

                var result = await _employeeService.Update(employee, username, email, password, primaryRoleId, secondaryRoleIds);

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

        [HttpPut("my-account/{id}")]
        public async Task<IActionResult> MyAccount(int id, [FromBody]MyAccountModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _employeeService.UpdateMyAccount(
                    id, 
                    model.FirstName,
                    model.LastName,
                    model.PhoneNumber,
                    model.Gender,
                    model.NIN,
                    model.Email, 
                    model.Password);

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

        [HttpGet("employees")]
        public IActionResult Employees(string userId, string firstName, string lastName, string email, int pageNumber = 1, int pageSize = 10)
        {
            var employees = _employeeService.GetEmployees(userId, firstName, lastName, email, out int total, pageNumber, pageSize);

            return Ok(Json(new
            {
                employees,
                total
            }));
        }
    }
}