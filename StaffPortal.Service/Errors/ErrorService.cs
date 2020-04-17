using StaffPortal.Common;
using StaffPortal.Data;

namespace StaffPortal.Service.Errors
{
    public class ErrorService : IErrorService
    {
        private readonly IRepository<ErrorLog> _errorLogRepository;

        public ErrorService(IRepository<ErrorLog> errorLogRepository)
        {
            _errorLogRepository = errorLogRepository;
        }

        public void Insert(ErrorLog log)
        {
            _errorLogRepository.Create(log);
        }
    }
}
