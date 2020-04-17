using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StaffPortal.Common.Settings;
using StaffPortal.Service.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace StaffPortal.Web.Controllers
{
    [Authorize]
    [Route("api/config/v1")]
    public class ConfigApiController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISettingService _settingService;

        public ConfigApiController(
            IHostingEnvironment hostingEnvironment,
            ISettingService settingService)
        {
            _hostingEnvironment = hostingEnvironment;
            _settingService = settingService;
        }

        [HttpGet("company")]
        public IActionResult Company()
        {
            var companySettings = _settingService.LoadSetting<CompanySettings>();
            return Ok(Json(companySettings));
        }

        [HttpPut("company/{id}")]
        public async Task<IActionResult> Company(int id, IFormFile logo, CompanySettings model)
        {
            if (logo != null && logo.Length > 0)
            {
                var logoFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "img\\" + id + "\\logo.png");
                using (var stream = new FileStream(logoFilePath, FileMode.Create))
                {
                    await logo.CopyToAsync(stream);
                }
            }

            _settingService.SaveSetting(model);

            return Ok();
        }

        [HttpGet("email")]
        public IActionResult Email()
        {
            var emailSettings = _settingService.LoadSetting<EmailSettings>();
            return Ok(Json(emailSettings));
        }

        [HttpPut("email/{id}")]
        public IActionResult Email(int id, [FromBody]EmailSettings model)
        {
            _settingService.SaveSetting(model);
            return Ok();
        }

        [HttpGet("leave")]
        public IActionResult Leave()
        {
            var leaveSettings = _settingService.LoadSetting<LeaveSettings>();
            return Ok(Json(leaveSettings));
        }

        [HttpPut("leave/{id}")]
        public IActionResult Leave(int id, [FromBody]LeaveSettings model)
        {
            _settingService.SaveSetting(model);
            return Ok();
        }
    }
}