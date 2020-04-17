using AutoMapper;
using StaffPortal.Common;
using StaffPortal.Web.Models;

namespace StaffPortal.Web.Infrastructure.AutoMapper.MappingProfiles
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<EmployeeModel, Employee>()
                .ForMember(x => x.User, opt => opt.Ignore());

            CreateMap<Employee, EmployeeModel>()
                .ForMember(x => x.PrimaryBusinessRoleId, opt => opt.ResolveUsing<PrimaryBusinessRoleIdResolver, int>(src => src.Id))
                .ForMember(x => x.SecondaryBusinessRoleIds, opt => opt.ResolveUsing<SecondaryBusinessRoleIdsResolver, int>(src => src.Id))
                .ForMember(x => x.Email, opt => opt.ResolveUsing<EmailResolver, string>(src => src.UserId))
                .ForMember(x => x.UserName, opt => opt.ResolveUsing<UsernameResolver, string>(src => src.UserId))
                .ForMember(x => x.PhoneNumber, opt => opt.ResolveUsing<PhoneNumberResolver, string>(src => src.UserId));
        }
    }
}
