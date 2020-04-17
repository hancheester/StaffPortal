using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StaffPortal.Service.Roles;
using StaffPortal.Service.Staff;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StaffPortal.Web.Infrastructure
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
    {
        private readonly IBusinessRoleService _businessRoleService;
        private readonly IEmployeeService _employeeService;

        public CustomUserClaimsPrincipalFactory(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IBusinessRoleService businessRoleService,
            IEmployeeService employeeService)
            : base(userManager, roleManager, optionsAccessor)
        {
            _businessRoleService = businessRoleService;
            this._employeeService = employeeService;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {
            var employee = _employeeService.GetEmployeeByApplicationUserId(user.Id);
            var identity = await base.GenerateClaimsAsync(user);
            var primaryRole = _businessRoleService.GetPrimaryBusinessRoleByEmployeeId(employee.Id);

            identity.AddClaim(new Claim(ClaimTypes.GivenName, employee.FirstName ?? string.Empty));
            identity.AddClaim(new Claim("EmployeeId", employee.Id.ToString()));
            identity.AddClaim(new Claim("PrimaryBusinessRoleId", primaryRole.Id.ToString()));

            return identity;
        }
    }
}
