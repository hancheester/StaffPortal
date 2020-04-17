using AutoMapper;
using Microsoft.AspNetCore.Identity;
using StaffPortal.Service.Staff;
using System.Threading.Tasks;

namespace StaffPortal.Web.Infrastructure.AutoMapper
{
    public class FullNameResolver : IMemberValueResolver<object, object, string, string>
    {
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<IdentityUser> _userManager;

        public FullNameResolver(
            IEmployeeService employeeService,
            UserManager<IdentityUser> userManager)
        {
            _employeeService = employeeService;
            _userManager = userManager;
        }

        public string Resolve(object source, object destination, string sourceMember, string destMember, ResolutionContext context)
        {
            var employee = _employeeService.GetEmployeeByApplicationUserId(sourceMember);

            if (employee == null)
            {
                var user = Task.Run(() => _userManager.FindByIdAsync(sourceMember)).Result;
                var isAdmin = Task.Run(() => _userManager.IsInRoleAsync(user, "SuperAdmin")).Result;

                if (isAdmin)
                    return "Administrator";

                return string.Empty;
                   
            }
            return (employee.FirstName + " " + employee.LastName).Trim();
        }
    }
}
