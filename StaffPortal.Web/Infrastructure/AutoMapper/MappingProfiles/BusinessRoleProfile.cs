using AutoMapper;
using StaffPortal.Common;
using StaffPortal.Web.Models;

namespace StaffPortal.Web.Infrastructure.AutoMapper.MappingProfiles
{
    public class BusinessRoleProfile : Profile
    {
        public BusinessRoleProfile()
        {
            CreateMap<DepartmentBusinessRoleModel, BusinessRole>()
                .ForMember(x => x.Id, opt => opt.MapFrom(source => source.RoleId))
                .ForMember(x => x.ParentBusinessRoleId, opt => opt.Ignore())
                .ForMember(x => x.Name, opt => opt.Ignore())
                .ForMember(x => x.Description, opt => opt.Ignore())
                .ForMember(x => x.Children, opt => opt.Ignore())
                .ForMember(x => x.Permissions, opt => opt.Ignore());

            CreateMap<BusinessRole, RoleTreeNodeModel>();

            CreateMap<BusinessRoleModel, BusinessRole>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.Description, opt => opt.Ignore())
                .ForMember(x => x.Children, opt => opt.Ignore())
                .ForMember(x => x.Permissions, opt => opt.Ignore());

            CreateMap<BusinessRole, BusinessRoleModel>();

        }
    }
}
