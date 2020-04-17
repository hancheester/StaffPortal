using StaffPortal.Common;
using StaffPortal.Data;
using System;
using System.Linq;

namespace StaffPortal.Service
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Employee_BusinessRole> _employeeBusinessRoleRepository;
        private readonly IRepository<WorkingDay> _daysWorkingRepository;
        private readonly IRepository<Assignment> _assignmentRepository;

        public RoleService(
            IRepository<Employee_BusinessRole> employeeBusinessRoleRepository,
            IRepository<WorkingDay> daysWorkingRepository,
            IRepository<Assignment> assignmentRepository)
        {
            _employeeBusinessRoleRepository = employeeBusinessRoleRepository;
            _daysWorkingRepository = daysWorkingRepository;
            _assignmentRepository = assignmentRepository;
        }

        public AssignedRole GetRoleSpecificDate(int employeeId, DateTime date)
        {
            var assignedRole = _assignmentRepository.Table
                .Where(x => x.EmployeeId == employeeId)
                .Where(x => x.CreatedOnDate.CompareTo(date) <= 0)
                //.Where(x => x.EndDate.CompareTo(date) >= 0)
                .Where(x => x.Day == date.DayOfWeek.ToString())
                .Select(x => new AssignedRole
                {
                    RoleId = x.BusinessRoleId,
                    DepartmentId = x.DepartmentId
                })
                .FirstOrDefault();

            if (assignedRole != null)
                return assignedRole;

            var primaryRoleId = _employeeBusinessRoleRepository.Table
               .Where(x => x.EmployeeId == employeeId)
               .Where(x => x.IsPrimary == true)
               .Select(x => x.BusinessRoleId)
               .FirstOrDefault();

            if (primaryRoleId == 0)
                return null;

            assignedRole = _daysWorkingRepository.Table
                .Where(x => x.EmployeeId == employeeId)
                .Where(x => x.Day == date.DayOfWeek.ToString())
                .Where(x => x.IsAssigned == true)
                .Select(x => new AssignedRole
                {
                    RoleId = primaryRoleId,
                    DepartmentId = x.DepartmentId.Value
                })
                .FirstOrDefault();

            return assignedRole;
        }
    }
}
