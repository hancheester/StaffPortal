using AutoMapper;
using StaffPortal.Common;
using StaffPortal.Web.Models;

namespace StaffPortal.Web.Infrastructure.AutoMapper.MappingProfiles
{
    public class WorkingDayProfile : Profile
    {
        public WorkingDayProfile()
        {
            CreateMap<WorkingDayModel, WorkingDay>()
                .ForMember(x => x.EmployeeId, opt => opt.Ignore());

            CreateMap<WorkingDay, WorkingDayModel>();            
        }
    }
}
