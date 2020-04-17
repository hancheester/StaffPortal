using Microsoft.AspNetCore.Mvc;
using StaffPortal.Common.Settings;

namespace StaffPortal.Web.ViewComponents
{
    public class LogoViewComponent : ViewComponent
    {
        private readonly CompanySettings _companySettings;

        public LogoViewComponent(CompanySettings companySettings)
        {
            _companySettings = companySettings;
        }

        public IViewComponentResult Invoke()
        {
           return View("Default", _companySettings.LogoPath);           
        }
    }
}
