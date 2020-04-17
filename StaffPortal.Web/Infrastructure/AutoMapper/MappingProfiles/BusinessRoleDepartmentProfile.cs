using AutoMapper;
using StaffPortal.Common;
using StaffPortal.Web.Models;

namespace StaffPortal.Web.Infrastructure.AutoMapper.MappingProfiles
{
    public class BusinessRoleDepartmentProfile : Profile
    {
        public BusinessRoleDepartmentProfile()
        {
            CreateMap<BusinessRole_Department, DepartmentBusinessRoleModel>()
                .ForMember(x => x.RoleId, opt => opt.MapFrom(source => source.BusinessRoleId))
                .ForMember(x => x.Name, opt => opt.Ignore())
                .ForMember(x => x.ShowOnRota, opt => opt.Ignore());

            CreateMap<DepartmentBusinessRoleModel, BusinessRole_Department>()
                .ForMember(x => x.BusinessRoleId, opt => opt.MapFrom(source => source.RoleId));
        }
    }
}
