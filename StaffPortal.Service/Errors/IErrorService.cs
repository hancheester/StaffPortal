using StaffPortal.Common;

namespace StaffPortal.Service.Errors
{
    public interface IErrorService
    {
        void Insert(ErrorLog log);
    }
}
