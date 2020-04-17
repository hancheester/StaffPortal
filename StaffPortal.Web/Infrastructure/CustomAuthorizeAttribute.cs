using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StaffPortal.Service.Permissions;
using StaffPortal.Service.Roles;
using StaffPortal.Service.Staff;
using StaffPortal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Web.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CustomAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public CustomAuthorizeAttribute(string permission)
        {
            this._permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                return;
            }

            if (user.HasClaim("Role", "SuperAdmin"))
            {
                return;
            }

            var employeeService = (IEmployeeService)context.HttpContext.RequestServices.GetService(typeof(IEmployeeService));
            var businessRoleService = (IBusinessRoleService)context.HttpContext.RequestServices.GetService(typeof(IBusinessRoleService));
            var permissionService = (IPermissionService)context.HttpContext.RequestServices.GetService(typeof(IPermissionService));

            var applicationUserId = user.Claims.First().Value;
            var employeeId = employeeService.GetEmployeeIdOnApplicationUserId(applicationUserId);
            var result = Task.Run(() => businessRoleService.GetBusinessRolesByEmployeeIdAsync(employeeId)).Result;
            
            if (result.Succeeded == false)
            { 
                return;
            }

            var roles = result.Object;

            var permissions = new List<Permission>();
            foreach (var role in roles)
            {
                permissions.AddRange(permissionService.GetPermissionsByBusinessRoleId(role.Id));
            }

            var hasPermission = permissions.Where(x => x.Name == _permission).Any();

            if (!hasPermission)
            {
                context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
            }

            return;
        }
    }
}
