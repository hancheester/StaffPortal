using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace StaffPortal.Web.Infrastructure.AutoMapper
{
    public class UsernameResolver : IMemberValueResolver<object, object, string, string>
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsernameResolver(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public string Resolve(object source, object destination, string sourceMember, string destMember, ResolutionContext context)
        {
            var user = Task.Run(() => _userManager.FindByIdAsync(sourceMember)).Result;
            return user.UserName;
        }
    }
}
