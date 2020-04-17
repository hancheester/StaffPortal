using AutoMapper;
using StaffPortal.Common;
using StaffPortal.Web.Models;

namespace StaffPortal.Web.Infrastructure.AutoMapper.MappingProfiles
{
    public class RequestedDateProfile : Profile
    {
        public RequestedDateProfile()
        {
            CreateMap<RequestedDate, MyRequestedDateModel>()
                .ForMember(x => x.Date, opt => opt.MapFrom(src => src.Date.ToShortDateString()))
                .ForMember(x => x.Department, opt => opt.ResolveUsing<DepartmentNameResolver, int>(src => src.DepartmentId))
                .ForMember(x => x.Status, opt => opt.ResolveUsing<RequestedDateStatusResolver>())
                .ForMember(x => x.Approver, opt => opt.ResolveUsing<FullNameResolver, string>(src => src.ApproverId))
                .ForMember(x => x.Reason, opt => opt.MapFrom(src => (src.Reason != null) ? src.Reason.Message : string.Empty));
        }
    }
}
