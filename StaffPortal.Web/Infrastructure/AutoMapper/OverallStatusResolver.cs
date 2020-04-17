using AutoMapper;
using StaffPortal.Common;
using System.Linq;

namespace StaffPortal.Web.Infrastructure.AutoMapper
{
    public class OverallStatusResolver : IValueResolver<LeaveRequest, object, string>
    {
        public string Resolve(LeaveRequest source, object destination, string destMember, ResolutionContext context)
        {
            var foundReject = source.RequestedDates.Any(x => x.StatusCode == (int)RequestStatus.Rejected);
            if (foundReject) return "Rejected";

            var allAccepted = source.RequestedDates.All(x => x.StatusCode == (int)RequestStatus.Accepted);
            if (allAccepted) return "Approved";

            return "Pending";
        }
    }
}
