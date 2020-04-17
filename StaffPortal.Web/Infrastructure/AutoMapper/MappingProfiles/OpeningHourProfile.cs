using AutoMapper;
using StaffPortal.Common;
using StaffPortal.Web.Models;

namespace StaffPortal.Web.Infrastructure.AutoMapper.MappingProfiles
{
    public class OpeningHourProfile : Profile
    {
        public OpeningHourProfile()
        {
            CreateMap<DepartmentOpeningHour, OpeningHour>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.DepartmentId, opt => opt.Ignore());
        }
    }
}
