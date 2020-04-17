using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace StaffPortal.Web.Infrastructure.AutoMapper.MappingProfiles
{
    public class PhoneNumberResolver : IMemberValueResolver<object, object, string, string>
    {
        private readonly UserManager<IdentityUser> _userManager;

        public PhoneNumberResolver(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public string Resolve(object source, object destination, string sourceMember, string destMember, ResolutionContext context)
        {
            var user = Task.Run(() => _userManager.FindByIdAsync(sourceMember)).Result;
            return user.PhoneNumber;
        }
    }
}
