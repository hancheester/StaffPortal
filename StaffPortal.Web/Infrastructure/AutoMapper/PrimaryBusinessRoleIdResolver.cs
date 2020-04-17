using AutoMapper;
using StaffPortal.Common;
using StaffPortal.Data;
using System.Linq;

namespace StaffPortal.Web.Infrastructure.AutoMapper
{
    public class PrimaryBusinessRoleIdResolver : IMemberValueResolver<object, object, int, int>
    {
        private readonly IRepository<Employee_BusinessRole> _employeeBusinessRoleRepository;

        public PrimaryBusinessRoleIdResolver(IRepository<Employee_BusinessRole> employeeBusinessRoleRepository)
        {
            _employeeBusinessRoleRepository = employeeBusinessRoleRepository;
        }

        public int Resolve(object source, object destination, int sourceMember, int destMember, ResolutionContext context)
        {
            var roleId = _employeeBusinessRoleRepository.Table
                .Where(x => x.EmployeeId == sourceMember)
                .Where(x => x.IsPrimary == true)
                .Select(x => x.BusinessRoleId)
                .FirstOrDefault();

            return roleId;
        }
    }
}
