using Microsoft.AspNetCore.Authorization;
using StaffPortal.Service.Permissions;
using StaffPortal.Service.Roles;
using StaffPortal.Service.Staff;
using StaffPortal.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Web.Infrastructure
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IEmployeeService _employeeService;
        private readonly IBusinessRoleService _businessRoleService;
        private readonly IPermissionService _permissionService;

        public PermissionHandler(
            IEmployeeService employeeService,
            IBusinessRoleService businessRoleService,
            IPermissionService permissionService)
        {
            _employeeService = employeeService;
            _businessRoleService = businessRoleService;
            _permissionService = permissionService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                return null;
            }

            if (context.User.HasClaim("Role", "SuperAdmin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var applicationUserId = context.User.Claims.First().Value;
            var employeeId = _employeeService.GetEmployeeIdOnApplicationUserId(applicationUserId);
            var result = Task.Run(() => _businessRoleService.GetBusinessRolesByEmployeeIdAsync(employeeId)).Result;

            if (result.Succeeded == false)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var roles = result.Object;

            var permissions = new List<Permission>();
            foreach (var role in roles)
            {
                permissions.AddRange(_permissionService.GetPermissionsByBusinessRoleId(role.Id));
            }

            var hasPermission = permissions.Where(x => x.Name == requirement.Permission).Any();

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
            
            //TODO: Use the following if targeting a version of
            //.NET Framework older than 4.6:
            //      return Task.FromResult(0);
            return Task.CompletedTask;
        }
    }
}
