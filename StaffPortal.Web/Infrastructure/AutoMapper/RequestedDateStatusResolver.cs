using AutoMapper;
using StaffPortal.Common;

namespace StaffPortal.Web.Infrastructure.AutoMapper
{
    public class RequestedDateStatusResolver : IValueResolver<RequestedDate, object, string>
    {
        public string Resolve(RequestedDate source, object destination, string destMember, ResolutionContext context)
        {
            var status = (RequestStatus)source.StatusCode;

            switch (status)
            {
                case RequestStatus.Pending:
                    return "Pending";
                case RequestStatus.Accepted:
                    return "Approved";
                default:
                case RequestStatus.Rejected:
                    return "Rejected";                
            }
        }
    }
}
