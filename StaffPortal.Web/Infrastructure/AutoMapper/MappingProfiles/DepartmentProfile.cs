using AutoMapper;
using StaffPortal.Common;
using StaffPortal.Web.Models;

namespace StaffPortal.Web.Infrastructure.AutoMapper.MappingProfiles
{
    public class DepartmentProfile : Profile
    {
        public DepartmentProfile()
        {
            CreateMap<DepartmentModel, Department>()
                .ForMember(x => x.DepartmentBusinessRoles, opt => opt.MapFrom(source => source.Roles));

            CreateMap<Department, DepartmentModel>()
                .ForMember(x => x.Roles, opt => opt.MapFrom(source => source.DepartmentBusinessRoles));
        }
    }
}
