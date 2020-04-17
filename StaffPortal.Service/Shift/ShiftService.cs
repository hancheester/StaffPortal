using Microsoft.Extensions.Caching.Memory;
using StaffPortal.Common;
using StaffPortal.Common.Models;
using StaffPortal.Data;
using StaffPortal.Service.Departments;
using StaffPortal.Service.Staff;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StaffPortal.Service.Shift
{
    public class ShiftService : IShiftService
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IDepartmentService _departmentService;
        private readonly IMemoryCache _memoryCache;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<BusinessRole> _businessRoleRepository;
        private readonly IRepository<Employee_BusinessRole> _employeeBusinessRoleRepository;
        private readonly IRepository<BusinessRole_Department> _businessRoleDepartmentRepository;
        private readonly IRepository<OpeningHour> _openingHourRepository;
        private readonly IRepository<WorkingDay> _workingDayRepository;
        private readonly IRepository<Assignment> _assignmentRepository;
        private readonly IRepository<LeaveRequest> _leaveRequestRepository;
        private readonly IRepository<RequestedDate> _requestedDateRepository;

        public ShiftService(
            IAttendanceService attendanceService,
            IDepartmentService departmentService,
            IMemoryCache memoryCache,
            IRepository<Employee> employeeRepository,
            IRepository<BusinessRole> businessRoleRepository,
            IRepository<Employee_BusinessRole> employeeBusinessRoleRepository,
            IRepository<BusinessRole_Department> businessRoleDepartmentRepository,
            IRepository<OpeningHour> openingHourRepository,
            IRepository<WorkingDay> workingDayRepository,
            IRepository<Assignment> assignmentRepository,
            IRepository<LeaveRequest> leaveRequestRepository,
            IRepository<RequestedDate> requestedDateRepository)
        {
            _attendanceService = attendanceService;
            _departmentService = departmentService;
            _memoryCache = memoryCache;
            _employeeRepository = employeeRepository;
            _businessRoleRepository = businessRoleRepository;
            _employeeBusinessRoleRepository = employeeBusinessRoleRepository;
            _businessRoleDepartmentRepository = businessRoleDepartmentRepository;
            _openingHourRepository = openingHourRepository;
            _workingDayRepository = workingDayRepository;
            _assignmentRepository = assignmentRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _requestedDateRepository = requestedDateRepository;
        }

        public IList<ShiftRole> GetRolesInRota(int departmentId, DateTime fromDate, DateTime toDate, int? roleId = null)
        {
            var requiredRolesQuery = _businessRoleDepartmentRepository.Table
                .Join(_businessRoleRepository.Table, bd => bd.BusinessRoleId, b => b.Id, (bd, b) => new { bd, b })
                .Where(x => x.bd.DepartmentId == departmentId);

            var staffRolesQuery = _employeeBusinessRoleRepository.Table
                .Join(_workingDayRepository.Table, eb => eb.EmployeeId, w => w.EmployeeId, (eb, w) => new { eb, w })
                .GroupJoin(_assignmentRepository.Table, x => x.eb.EmployeeId, a => a.EmployeeId, (x, a) => new { x.eb, x.w, a = a.DefaultIfEmpty() })
                .SelectMany(x => x.a.DefaultIfEmpty(), (x, a) => new { x.eb, x.w, a })
                .Where(x => x.w.DepartmentId == departmentId)
                .Where(x => x.w.IsAssigned == true)
                .Where(x => x.eb.IsPrimary == true)
                .Where(x => x.a == null);

            var assignedRolesQuery = _assignmentRepository.Table
                .Join(_businessRoleRepository.Table, a => a.BusinessRoleId, b => b.Id, (a, b) => new { a, b })
                .Where(x => x.a.DepartmentId == departmentId)
                .Where(x => x.a.StartDate >= fromDate || x.a.EndDate.Value <= toDate);

            if (roleId.HasValue)
            {
                requiredRolesQuery = requiredRolesQuery.Where(x => x.bd.BusinessRoleId == roleId.Value);
                staffRolesQuery = staffRolesQuery.Where(x => x.eb.BusinessRoleId == roleId);
                assignedRolesQuery = assignedRolesQuery.Where(x => x.a.BusinessRoleId == roleId.Value);
            }

            var requiredRoles = requiredRolesQuery
                .Select(x => new ShiftRole
                {
                    Id = x.bd.BusinessRoleId,
                    Name = x.b.Name,
                    IsRequired = true,
                    MinimumRequired = x.bd.MinimumRequired
                })
                .ToList();
            
            var staffRoles = staffRolesQuery
            .Select(x => x.eb.BusinessRoleId)
            .Distinct()
            .Join(_businessRoleRepository.Table, x => x, b => b.Id, (x, b) => new { Id = x, b })
            .Select(x => new ShiftRole
            {
                Id = x.Id,
                Name = x.b.Name
            })
            .ToList();

            var assignedRoles = assignedRolesQuery
                .Select(x => new ShiftRole
                {
                    Id = x.a.BusinessRoleId,
                    Name = x.b.Name
                })
                .Distinct()
                .ToList();

            requiredRoles.ForEach((role) =>
            {
                staffRoles.Any(x => x.Id == role.Id);
                staffRoles.RemoveAll((x) => x.Id == role.Id);

                assignedRoles.Any(x => x.Id == role.Id);
                assignedRoles.RemoveAll((y) => y.Id == role.Id);
            });

            assignedRoles.ForEach((role) =>
            {
                staffRoles.Any(x => x.Id == role.Id);
                staffRoles.RemoveAll((y) => y.Id == role.Id);
            });

            var roles = requiredRoles.Union(assignedRoles).Union(staffRoles).ToList();

            return roles;
        }

        public IList<Rota> GetRotas(int departmentId, int roleId, DateTime date, DateTime currentDate)
        {
            var isOpen = _openingHourRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.IsOpen == true)
                .Where(x => x.Day == date.DayOfWeek.ToString())
                .Any();

            if (isOpen == false) return new List<Rota>();

            var employees = _workingDayRepository.Table
                .Join(_employeeBusinessRoleRepository.Table, w => w.EmployeeId, eb => eb.EmployeeId, (w, eb) => new { w, eb })
                .Join(_employeeRepository.Table, x => x.w.EmployeeId, e => e.Id, (x, e) => new { x.w, x.eb, e })
                .Where(x => x.w.DepartmentId == departmentId)
                .Where(x => x.eb.BusinessRoleId == roleId)
                .Where(x => x.eb.IsPrimary == true)
                .Where(x => x.w.IsAssigned == true)
                .Where(x => x.w.Day == date.DayOfWeek.ToString())
                .Select(x => new { x.w.EmployeeId, Name = (x.e.FirstName + " " + x.e.LastName).Trim(), x.w.StartTime, x.w.EndTime })
                .ToList();

            var assignedEmployees = _assignmentRepository.Table
                .Join(_employeeRepository.Table, a => a.EmployeeId, e => e.Id, (a, e) => new { a, e })
                .Where(x => x.a.DepartmentId == departmentId)
                .Where(x => x.a.BusinessRoleId == roleId)
                .Where(x => x.a.Day == date.DayOfWeek.ToString())
                .Where(x => x.a.StartDate <= currentDate)
                .Where(x => x.a.EndDate.HasValue ? x.a.EndDate.Value >= currentDate : true)
                .Select(x => new { x.a.EmployeeId, Name = (x.e.FirstName + " " + x.e.LastName).Trim(), x.a.StartTime, x.a.EndTime })
                .ToList();

            var onHolidayEmployees = _requestedDateRepository.Table
                .Join(_leaveRequestRepository.Table, r => r.LeaveRequestId, l => l.Id, (r, l) => new { r, l })
                .Where(x => x.r.DepartmentId == departmentId)
                .Where(x => x.r.Date.Date == date.Date)
                .Where(x => x.r.StatusCode == (int)RequestStatus.Accepted)
                .ToList();

            var employeeIds = employees.Select(x => x.EmployeeId).Union(assignedEmployees.Select(x => x.EmployeeId)).ToList();
            var rotas = new List<Rota>();

            foreach (var employeeId in employeeIds)
            {
                var isOnLeave = false;
                isOnLeave = onHolidayEmployees.Any(x => x.l.EmployeeId == employeeId);

                var assignedEmployee = assignedEmployees.Where(x => x.EmployeeId == employeeId).FirstOrDefault();
                if (assignedEmployee != null)
                {
                    var rota = new Rota
                    {
                        EmployeeId = assignedEmployee.EmployeeId,
                        Name = assignedEmployee.Name,
                        Day = date.DayOfWeek.ToString(),
                        Shift = new Tuple<string, string, bool>(assignedEmployee.StartTime.ToString(), assignedEmployee.EndTime.ToString(), isOnLeave)
                    };

                    rotas.Add(rota);
                }
                else
                {
                    var employee = employees.Where(x => x.EmployeeId == employeeId).FirstOrDefault();
                    var rota = new Rota
                    {
                        EmployeeId = employee.EmployeeId,
                        Name = employee.Name,
                        Day = date.DayOfWeek.ToString(),
                        Shift = new Tuple<string, string, bool>(employee.StartTime.ToString(), employee.EndTime.ToString(), isOnLeave)
                    };

                    rotas.Add(rota);
                }
            }

            return rotas;
        }

        public OperationResult<Tuple<int, int>> GetMinimumStaffStatus(int departmentId, DateTime date)
        {
            var isOpen = _openingHourRepository.Table
            .Where(x => x.DepartmentId == departmentId)
            .Where(x => x.IsOpen == true)
            .Where(x => x.Day == date.DayOfWeek.ToString())
            .Any();

            if (isOpen == false) return new OperationResult<Tuple<int, int>>(true);

            var actualStaffCount = _attendanceService.GetDepartmentalStaffCount(departmentId, date, DateTime.Now);
            var minimumStaff = _departmentService.GetMinimumRequiredStaff(departmentId);

            return new OperationResult<Tuple<int, int>>(new Tuple<int, int>(minimumStaff, actualStaffCount));            
        }

        public OperationResult<Assignment> CreateAssignment(Assignment assignment)
        {
            var result = new OperationResult<Assignment>(assignment);

            var id = _assignmentRepository.Create(assignment);
            assignment.Id = id;

            return result;
        }

        public OperationResult RemoveAssignment(int departmentId, int employeeId)
        {
            var result = new OperationResult();

            var items = _assignmentRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.EmployeeId == employeeId)
                .ToList();

            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    _assignmentRepository.Delete(item);
                }
            }

            return result;
        }
    }
}
