using AutoMapper;
using StaffPortal.Common;
using StaffPortal.Web.Models;

namespace StaffPortal.Web.Infrastructure.AutoMapper.MappingProfiles
{
    public class LeaveRequestProfile : Profile
    {
        public LeaveRequestProfile()
        {
            CreateMap<CreateLeaveRequestModel, LeaveRequest>();

            CreateMap<LeaveRequest, MyLeaveRequestModel>()
                .ForMember(x => x.RequestedOn, opt => opt.MapFrom(src => src.DateCreated.ToShortDateString()))
                .ForMember(x => x.Type, opt => opt.ResolveUsing<LeaveTypeNameResolver>())
                .ForMember(x => x.OverallStatus, opt => opt.ResolveUsing<OverallStatusResolver>());
        }
    }
}
