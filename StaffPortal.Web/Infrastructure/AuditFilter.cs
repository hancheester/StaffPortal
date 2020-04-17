using Microsoft.AspNetCore.Mvc.Filters;
using StaffPortal.Common;
using System;
using System.ComponentModel;

namespace StaffPortal.Web.Infrastructure
{
    // To use the Filter with dependency Injection add the filter this way:     [ServiceFilter(typeof(AuditFilter))]

    public class AuditFilter : ActionFilterAttribute, IActionFilter
    {
        public string Description { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var actionName = filterContext.ActionDescriptor.DisplayName;
            var controllerName = filterContext.Controller;
            var routeData = filterContext.RouteData;

            var attributes = filterContext.ActionDescriptor.GetType().GetCustomAttributes(true);
            var attributeCollection = TypeDescriptor.GetProperties(filterContext.ActionDescriptor);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var actionName = filterContext.ActionDescriptor.DisplayName;
            var controllerName = filterContext.Controller;
            var routeData = filterContext.RouteData;

            var attributes = filterContext.ActionDescriptor.GetType().GetCustomAttributes(true);

            var attributeCollection = TypeDescriptor.GetProperties(filterContext.ActionDescriptor);

            var report = new AuditTrail
            {
                Details = $"{controllerName}",
                Event = $"{actionName}",
                DateTimeCreated = DateTime.Now,
                Username = filterContext.HttpContext.User.Identity.Name
            };

            //_auditTrailRepository.Create(report);
        }
    }
}