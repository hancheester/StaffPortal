using StaffPortal.Common;
using StaffPortal.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StaffPortal.Service.Staff
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<RequestedDate> _requestedDateRepository;
        private readonly IRepository<Assignment> _assignmentRepository;
        private readonly IRepository<BusinessRole_Department> _businessRoleDepartmentRepository;
        private readonly IRepository<WorkingDay> _workingDayRepository;
        private readonly IRepository<LeaveRequest> _leaveRequestRepository;
        private readonly IRepository<Employee_BusinessRole> _employeeBusinessRoleRepository;
        
        public AttendanceService(
            IRepository<Department> departmentRepository,
            IRepository<RequestedDate> requestedDateRepository,
            IRepository<Assignment> assignmentRepository,
            IRepository<BusinessRole_Department> businessRoleDepartmentRepository,
            IRepository<WorkingDay> workingDayRepository,
            IRepository<LeaveRequest> leaveRequestRepository,
            IRepository<Employee_BusinessRole> employeeBusinessRoleRepository)
        {
            _departmentRepository = departmentRepository;
            _requestedDateRepository = requestedDateRepository;
            _assignmentRepository = assignmentRepository;
            _businessRoleDepartmentRepository = businessRoleDepartmentRepository;
            _workingDayRepository = workingDayRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _employeeBusinessRoleRepository = employeeBusinessRoleRepository;
        }

        public StaffLevelStatus GetDepartmentalAttendanceStatus(int departmentId, DateTime date, int? roleId = null)
        {
            var department = _departmentRepository.Return(departmentId);

            var queryRoles = _businessRoleDepartmentRepository.Table
                .Where(x => x.DepartmentId == departmentId);

            if (roleId.HasValue)
                queryRoles = queryRoles.Where(x => x.BusinessRoleId == roleId.Value);

            var businessRoles = queryRoles.Select(x => new { x.BusinessRoleId, x.MinimumRequired }).ToList();

            var businessRoleIds = businessRoles.Select(x => x.BusinessRoleId).ToList();

            var onLeaveStaffCounts = _leaveRequestRepository.Table
                .Join(_requestedDateRepository.Table, l => l.Id, r => r.LeaveRequestId, (l, r) => new { l, r })
                .Join(_employeeBusinessRoleRepository.Table, l_r => l_r.l.EmployeeId, eb => eb.EmployeeId, (l_r, eb) => new { l_r.l, l_r.r, eb })
                .Where(l_r_eb => businessRoleIds.Contains(l_r_eb.eb.BusinessRoleId))
                .Where(l_r_eb => l_r_eb.r.DepartmentId == departmentId)
                .Where(l_r_eb => l_r_eb.r.StatusCode == (int)RequestStatus.Accepted)
                .Where(l_r_eb => l_r_eb.r.Date.Date == date.Date)
                .GroupBy(l_r_eb => l_r_eb.eb.BusinessRoleId)
                .Select(x => new
                {
                    BusinessRoleId = x.Key,
                    StaffCount = x.Count()
                })
                .ToList();

            var assignedStaffCounts = _assignmentRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.StartDate.CompareTo(date) <= 0)
                .Where(x => x.EndDate.HasValue ? x.EndDate.Value.CompareTo(date) >= 0 : true)
                .Where(x => businessRoleIds.Contains(x.BusinessRoleId))
                .Where(x => x.Day == date.DayOfWeek.ToString())
                .GroupBy(x => x.BusinessRoleId)
                .Select(x => new
                {
                    BusinessRoleId = x.Key,
                    StaffCount = x.Count()
                })
                .ToList();

            var expectedStaffCount = _workingDayRepository.Table
                .Join(_employeeBusinessRoleRepository.Table, d => d.EmployeeId, eb => eb.EmployeeId, (d, eb) => new { d, eb })
                .Where(d_eb => d_eb.eb.IsPrimary == true)
                .Where(d_eb => d_eb.d.DepartmentId == departmentId)
                .Where(d_eb => d_eb.d.Day == date.DayOfWeek.ToString())
                .Where(d_eb => d_eb.d.IsAssigned == true)
                .GroupBy(d_eb => d_eb.eb.BusinessRoleId)
                .Select(x => new
                {
                    BusinessRoleId = x.Key,
                    StaffCount = x.Count()
                })
                .ToList();

            var query = businessRoles
                .GroupJoin(onLeaveStaffCounts, br => br.BusinessRoleId, ol => ol.BusinessRoleId, (br, ol) => new { br, ol = ol.DefaultIfEmpty() })
                .GroupJoin(assignedStaffCounts, br_ol => br_ol.br.BusinessRoleId, ab => ab.BusinessRoleId, (br_ol, ab) => new { br_ol.br, br_ol.ol, ab = ab.DefaultIfEmpty() })
                .GroupJoin(expectedStaffCount, br_ol_ab => br_ol_ab.br.BusinessRoleId, ex => ex.BusinessRoleId, (br_ol_ab, ex) => new { br_ol_ab.br, br_ol_ab.ol, br_ol_ab.ab, ex = ex.DefaultIfEmpty() });

            var result = query.Select(x => new
            {
                x.br.BusinessRoleId,
                x.br.MinimumRequired,
                OnLeaveStaffCount = x.ol.Sum(y => y == null ? 0 : y.StaffCount),
                AssignedStaffCount = x.ab.Sum(y => y == null ? 0 : y.StaffCount),
                ExpectedStaffCount = x.ex.Sum(y => y == null ? 0 : y.StaffCount)
            })
            .ToList();

            var actualStaffCount = result.Sum(x => x.ExpectedStaffCount - x.OnLeaveStaffCount + x.AssignedStaffCount);

            var hasZeroPersonRole = result
                .Where(x => x.MinimumRequired > 0)
                .Any(x => (x.ExpectedStaffCount - x.OnLeaveStaffCount + x.AssignedStaffCount) <= 0);

            if ((department.MinimumRequired > actualStaffCount) || hasZeroPersonRole)
                return StaffLevelStatus.Critical;

            var hasLowStaffRole = result
                .Where(x => x.MinimumRequired > 0)
                .Any(x => x.MinimumRequired > (x.ExpectedStaffCount - x.OnLeaveStaffCount + x.AssignedStaffCount));

            if (hasLowStaffRole)
                return StaffLevelStatus.Decent;

            return StaffLevelStatus.Acceptable;
        }

        public int GetDepartmentalStaffCount(int departmentId, DateTime date, DateTime currentDate, int? roleId = null)
        {
            var department = _departmentRepository.Return(departmentId);

            var queryRequiredRoles = _businessRoleDepartmentRepository.Table
                .Where(x => x.MinimumRequired > 0)
                .Where(x => x.DepartmentId == departmentId);

            var queryStaffRoles = _employeeBusinessRoleRepository.Table
                .Join(_workingDayRepository.Table, eb => eb.EmployeeId, w => w.EmployeeId, (eb, w) => new { eb, w })
                .GroupJoin(_assignmentRepository.Table, x => x.eb.EmployeeId, a => a.EmployeeId, (x, a) => new { x.eb, x.w, a = a.DefaultIfEmpty() })
                .SelectMany(x => x.a.DefaultIfEmpty(), (x, a) => new { x.eb, x.w, a })
                .Where(x => x.w.DepartmentId == departmentId)
                .Where(x => x.eb.IsPrimary == true)
                .Where(x => x.w.IsAssigned == true)
                .Where(x => x.a == null);

            var queryAssignedRoles = _assignmentRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.Day == date.DayOfWeek.ToString())
                .Where(x => x.StartDate <= currentDate)
                .Where(x => x.EndDate.HasValue ? x.EndDate.Value >= currentDate : true);

            if (roleId.HasValue)
            {
                queryRequiredRoles = queryRequiredRoles.Where(x => x.BusinessRoleId == roleId.Value);
                queryStaffRoles = queryStaffRoles.Where(x => x.eb.BusinessRoleId == roleId.Value);
                queryAssignedRoles = queryAssignedRoles.Where(x => x.BusinessRoleId == roleId.Value);
            }

            var businessRoles = queryRequiredRoles.Select(x => new { x.BusinessRoleId, x.MinimumRequired }).ToList();
            var requiredBusinessRoleIds = businessRoles.Select(x => x.BusinessRoleId).ToList();
            var staffRoles = queryStaffRoles
                .Where(x => !requiredBusinessRoleIds.Contains(x.eb.BusinessRoleId))
                .Select(x => x.eb.BusinessRoleId)
                .Distinct()
                .Select(x => new { BusinessRoleId = x, MinimumRequired = 0 }).ToList();
            var assignedRoles = queryAssignedRoles
                .Where(x => !requiredBusinessRoleIds.Contains(x.BusinessRoleId))
                .Select(x => x.BusinessRoleId)
                .Distinct()
                .Select(x => new { BusinessRoleId = x, MinimumRequired = 0 }).ToList();

            businessRoles = businessRoles.Union(staffRoles).Union(assignedRoles).ToList();

            var businessRoleIds = businessRoles.Select(x => x.BusinessRoleId).ToList();

            var onLeaveStaffCounts = _leaveRequestRepository.Table
                .Join(_requestedDateRepository.Table, l => l.Id, r => r.LeaveRequestId, (l, r) => new { l, r })
                .Join(_employeeBusinessRoleRepository.Table, l_r => l_r.l.EmployeeId, eb => eb.EmployeeId, (l_r, eb) => new { l_r.l, l_r.r, eb })
                .Where(x => businessRoleIds.Contains(x.eb.BusinessRoleId))
                .Where(x => x.r.DepartmentId == departmentId)
                .Where(x => x.r.StatusCode == (int)RequestStatus.Accepted)
                .Where(x => x.r.Date.Date == date.Date)
                .GroupBy(x => x.eb.BusinessRoleId)
                .Select(x => new
                {
                    BusinessRoleId = x.Key,
                    EmployeeIds = x.Select(y => y.eb.EmployeeId).Distinct()
                })
                .ToList();

            var assignedStaffCounts = _assignmentRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.Day == date.DayOfWeek.ToString())
                .Where(x => x.StartDate <= currentDate)
                .Where(x => x.EndDate.HasValue ? x.EndDate.Value >= currentDate : true)
                .Where(x => x.StartTime.HasValue)
                .Where(x => x.EndTime.HasValue)
                .Where(x => businessRoleIds.Contains(x.BusinessRoleId))
                .Where(x => x.Day == date.DayOfWeek.ToString())
                .GroupBy(x => x.BusinessRoleId)
                .Select(x => new
                {
                    BusinessRoleId = x.Key,
                    EmployeeIds = x.Select(y => y.EmployeeId).Distinct().ToList()
                })
                .ToList();

            var expectedStaffCount = _workingDayRepository.Table
                .Join(_employeeBusinessRoleRepository.Table, d => d.EmployeeId, eb => eb.EmployeeId, (d, eb) => new { d, eb })                
                .Where(x => x.d.DepartmentId == departmentId)
                .Where(x => businessRoleIds.Contains(x.eb.BusinessRoleId))
                .Where(x => x.d.Day == date.DayOfWeek.ToString())
                .Where(x => x.d.IsAssigned == true)
                .Where(x => x.eb.IsPrimary == true)
                .GroupBy(x => x.eb.BusinessRoleId)
                .Select(x => new
                {
                    BusinessRoleId = x.Key,
                    EmployeeIds = x.Select(y => y.eb.EmployeeId).Distinct().ToList()
                })
                .ToList();

            foreach (var role in expectedStaffCount)
            {
                foreach (var id in role.EmployeeIds)
                {
                    var assignedRole = assignedStaffCounts.Where(x => x.BusinessRoleId == role.BusinessRoleId).FirstOrDefault();

                    if (assignedRole != null)
                    {
                        assignedRole.EmployeeIds.RemoveAll((employeeId) => employeeId == id);
                    }
                }
            }

            var query = businessRoles
                .GroupJoin(onLeaveStaffCounts, br => br.BusinessRoleId, ol => ol.BusinessRoleId, (br, ol) => new { br, ol = ol.DefaultIfEmpty() })
                .GroupJoin(assignedStaffCounts, br_ol => br_ol.br.BusinessRoleId, ab => ab.BusinessRoleId, (br_ol, ab) => new { br_ol.br, br_ol.ol, ab = ab.DefaultIfEmpty() })
                .GroupJoin(expectedStaffCount, br_ol_ab => br_ol_ab.br.BusinessRoleId, ex => ex.BusinessRoleId, (br_ol_ab, ex) => new { br_ol_ab.br, br_ol_ab.ol, br_ol_ab.ab, ex = ex.DefaultIfEmpty() });

            var result = query.Select(x => new
            {
                x.br.BusinessRoleId,
                x.br.MinimumRequired,
                OnLeaveStaffCount = x.ol.Sum(y => y == null ? 0 : y.EmployeeIds.Count()),
                AssignedStaffCount = x.ab.Sum(y => y == null ? 0 : y.EmployeeIds.Count()),
                ExpectedStaffCount = x.ex.Sum(y => y == null ? 0 : y.EmployeeIds.Count())
            })
            .ToList();

            var actualStaffCount = result.Sum(x => x.ExpectedStaffCount - x.OnLeaveStaffCount + x.AssignedStaffCount);

            return actualStaffCount;
        }
    }
}
