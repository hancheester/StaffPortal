using AutoMapper;
using StaffPortal.Service.Leave;
using StaffPortal.Common;

namespace StaffPortal.Web.Infrastructure.AutoMapper
{
    public class LeaveTypeNameResolver : IValueResolver<LeaveRequest, object, string>
    {
        private readonly ILeaveTypeService _leaveTypeService;

        public LeaveTypeNameResolver(ILeaveTypeService leaveTypeService)
        {
            _leaveTypeService = leaveTypeService;
        }

        public string Resolve(LeaveRequest source, object destination, string destMember, ResolutionContext context)
        {
            var type = _leaveTypeService.GetLeaveTypeById(source.LeaveTypeId);
            return type.Name;
        }
    }
}
