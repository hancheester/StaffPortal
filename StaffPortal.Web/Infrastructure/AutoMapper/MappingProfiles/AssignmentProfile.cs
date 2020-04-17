using AutoMapper;
using StaffPortal.Common;
using StaffPortal.Web.Models;

namespace StaffPortal.Web.Infrastructure.AutoMapper.MappingProfiles
{
    public class AssignmentProfile : Profile
    {
        public AssignmentProfile()
        {
            CreateMap<AssignmentModel, Assignment>();
        }
    }
}
