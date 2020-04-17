using AutoMapper;
using StaffPortal.Common;
using StaffPortal.Data;
using System.Linq;

namespace StaffPortal.Web.Infrastructure.AutoMapper
{
    public class SecondaryBusinessRoleIdsResolver : IMemberValueResolver<object, object, int, int[]>
    {
        private readonly IRepository<Employee_BusinessRole> _employeeBusinessRoleRepository;

        public SecondaryBusinessRoleIdsResolver(IRepository<Employee_BusinessRole> employeeBusinessRoleRepository)
        {
            _employeeBusinessRoleRepository = employeeBusinessRoleRepository;
        }

        public int[] Resolve(object source, object destination, int sourceMember, int[] destMember, ResolutionContext context)
        {
            var roleIds = _employeeBusinessRoleRepository.Table
                .Where(x => x.EmployeeId == sourceMember)
                .Where(x => x.IsPrimary == false)
                .Select(x => x.BusinessRoleId)
                .ToArray();

            return roleIds;
        }
    }
}
