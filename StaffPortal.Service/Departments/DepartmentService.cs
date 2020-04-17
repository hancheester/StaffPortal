using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using StaffPortal.Service.Errors;
using StaffPortal.Common;
using StaffPortal.Common.Models;
using StaffPortal.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StaffPortal.Service.Cache;
using StaffPortal.Service.Permissions;

namespace StaffPortal.Service.Departments
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IErrorService _errorService;
        private readonly IPermissionService _permissionService;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<OpeningHour> _openingHourRepository;
        private readonly IRepository<BusinessRole> _businessRoleRepository;
        private readonly IRepository<BusinessRole_Department> _businessRoleDepartmentRepository;
        private readonly IRepository<WorkingDay> _daysWorkingRepository;
        private readonly IRepository<LeaveRequest> _leaveRequestRepository;
        private readonly IRepository<RequestedDate> _requestedDateRepository;
        private readonly IRepository<Assignment> _assignmentRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMemoryCache _memoryCache;

        public DepartmentService(
            IErrorService errorService,
            IPermissionService permissionService,
            IRepository<Department> departmentRepository,
            IRepository<OpeningHour> openingHourRepository,
            IRepository<BusinessRole> businessRoleRepository,
            IRepository<BusinessRole_Department> businessRoleDepartmentRepository,
            IRepository<WorkingDay> daysWorkingRepositoryNew,
            IRepository<LeaveRequest> leaveRequestRepositoryNew,
            IRepository<RequestedDate> requestedDateRepositoryNew,
            IRepository<Assignment> assignmentRepositoryNew,
            IRepository<Employee> employeeRepository,
            UserManager<IdentityUser> userManager,
            IMemoryCache memoryCache)
        {
            _errorService = errorService;
            _permissionService = permissionService;
            _departmentRepository = departmentRepository;
            _openingHourRepository = openingHourRepository;
            _businessRoleRepository = businessRoleRepository;
            _businessRoleDepartmentRepository = businessRoleDepartmentRepository;
            _daysWorkingRepository = daysWorkingRepositoryNew;
            _leaveRequestRepository = leaveRequestRepositoryNew;
            _requestedDateRepository = requestedDateRepositoryNew;
            _assignmentRepository = assignmentRepositoryNew;
            _employeeRepository = employeeRepository;
            _userManager = userManager;
            _memoryCache = memoryCache;
        }

        public async Task<IList<Department>> GetAllDepartmentsAsync()
        {
            var departments = await Task.FromResult(
                _memoryCache.Get("department.all", () =>
                {
                    return _departmentRepository.Table.ToList();
                })
            );
            return departments;
        }

        public OperationResult<Department> AddDepartment(Department department)
        {
            var result = new OperationResult<Department>(department);

            try
            {
                department.Id = _departmentRepository.Create(department);

                foreach(DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var openDay = department.OpeningHours
                        .Where(x => x.Day == day.ToString())
                        .Select(x => new { x.IsOpen, x.OpeningTime, x.ClosingTime })
                        .FirstOrDefault();

                    _openingHourRepository.Create(new OpeningHour
                    {
                        Day = day.ToString(),
                        DepartmentId = department.Id,
                        IsOpen = openDay != null ? openDay.IsOpen : false,
                        OpeningTime = openDay != null ? openDay.OpeningTime : TimeSpan.Zero,
                        ClosingTime = openDay != null ? openDay.ClosingTime : TimeSpan.Zero
                    });
                }

                if (department.DepartmentBusinessRoles.Count() > 0)
                {
                    foreach (var role in department.DepartmentBusinessRoles)
                    {
                        var item = new BusinessRole_Department
                        {
                            BusinessRoleId = role.Id,
                            DepartmentId = department.Id,
                            MinimumRequired = role.MinimumRequired
                        };
                        _businessRoleDepartmentRepository.Create(item);
                    }
                }

                _memoryCache.Remove("department.all");
            }
            catch (Exception ex)
            {
                result.AddOperationError("E1", "Failed to create department.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public async Task<IList<Department>> GetAssignedDepartmentsByEmployeeIdAsync(int employeeId)
        {   
            var isAdmin = await _permissionService.HasPermissionAsync(PermissionKeys.ADMINISTRATIVE_ACCESS, employeeId);

            IList<Department> departments = new List<Department>();

            if (isAdmin)
            {
                departments = await GetAllDepartmentsAsync();
            }
            else
            {
                var departmentIds = _daysWorkingRepository.Table
                    .Where(x => x.EmployeeId == employeeId)
                    .Select(x => x.DepartmentId)
                    .Distinct()
                    .ToList();

                departments = _departmentRepository.Table
                    .Where(x => departmentIds.Contains(x.Id))
                    .ToList();
            }
            
            return departments;
        }

        public OperationResult DeleteDepartment(int departmentId)
        {
            var result = new OperationResult();

            try
            {
                var department = _departmentRepository.Return(departmentId);

                if (department != null)
                {
                    var openingHours = _openingHourRepository.Table
                        .Where(x => x.DepartmentId == departmentId)
                        .ToList();

                    if (openingHours.Count > 0)
                    {
                        foreach (var openingHour in openingHours)
                        {
                            _openingHourRepository.Delete(openingHour);
                        }
                    }

                    var roles = _businessRoleDepartmentRepository.Table
                        .Where(x => x.DepartmentId == departmentId)
                        .ToList();

                    if (roles.Count() > 0)
                    {
                        foreach (var role in roles)
                        {
                            _businessRoleDepartmentRepository.Delete(role);
                        }
                    }

                    _departmentRepository.Delete(department);
                }
            }
            catch(Exception ex)
            {
                result.AddOperationError("E2", "Failed to delete department.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public OperationResult<Department> UpdateDepartment(Department department)
        {
            var result = new OperationResult<Department>(department);

            try
            {
                var foundDepartment = _departmentRepository.Return(department.Id);

                if (foundDepartment != null)
                {
                    foundDepartment.Name = department.Name;
                    foundDepartment.MinimumRequired = department.MinimumRequired;

                    _departmentRepository.Update(foundDepartment);

                    var foundOpeningHours = _openingHourRepository.Table
                        .Where(x => x.DepartmentId == department.Id)
                        .ToList();

                    if (foundOpeningHours.Count() > 0)
                    {
                        foreach (var item in foundOpeningHours)
                        {
                            _openingHourRepository.Delete(item);
                        }
                    }

                    foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                    {
                        var openDay = department.OpeningHours
                        .Where(x => x.Day == day.ToString())
                        .Select(x => new { x.IsOpen, x.OpeningTime, x.ClosingTime })
                        .FirstOrDefault();

                        _openingHourRepository.Create(new OpeningHour
                        {
                            Day = day.ToString(),
                            DepartmentId = department.Id,
                            IsOpen = openDay != null ? openDay.IsOpen : false,
                            OpeningTime = openDay != null ? openDay.OpeningTime : TimeSpan.Zero,
                            ClosingTime = openDay != null ? openDay.ClosingTime : TimeSpan.Zero
                        });
                    }

                    var foundRoles = _businessRoleDepartmentRepository.Table
                        .Where(x => x.DepartmentId == department.Id)
                        .ToList();

                    if (foundRoles.Count() > 0)
                    {
                        foreach (var item in foundRoles)
                        {
                            _businessRoleDepartmentRepository.Delete(item);
                        }
                    }

                    if (department.DepartmentBusinessRoles.Count() > 0)
                    {
                        foreach (var role in department.DepartmentBusinessRoles)
                        {
                            var item = new BusinessRole_Department
                            {
                                BusinessRoleId = role.BusinessRoleId,
                                DepartmentId = department.Id,
                                MinimumRequired = role.MinimumRequired
                            };
                            _businessRoleDepartmentRepository.Create(item);
                        }
                    }
                }

                _memoryCache.Remove("department.all");
                _memoryCache.Remove($"department.id.{department.Id}");
            }
            catch (Exception ex)
            {
                result.AddOperationError("E1", "Failed to update department.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public Department GetDepartmentById(int departmentId)
        {
            return _memoryCache.Get($"department.id.{departmentId}", () =>
            {
                var department = _departmentRepository.Return(departmentId);

                if (department != null)
                {
                    department.DepartmentBusinessRoles = _businessRoleDepartmentRepository.Table                         
                         .Where(x => x.DepartmentId == department.Id)                         
                         .ToList();

                    department.OpeningHours = _openingHourRepository.Table.Where(x => x.DepartmentId == department.Id).ToList();
                }

                return department;
            });            
        }

        public async Task<int> GetMinimumRequiredStaffAsync(int departmentId)
        {
            var result = await Task.Run(() =>
            {
                var department = GetDepartmentById(departmentId);
                if (department != null) return department.MinimumRequired;
                return 0;
            });

            return result;
        }

        public int GetMinimumRequiredStaff(int departmentId)
        {
            return Task.Run(() => GetMinimumRequiredStaffAsync(departmentId)).Result;
        }


        public int GetStaffCount(int departmentId, DateTime date, int businessRoleId)
        {
            var department = GetDepartmentById(departmentId);
            if (department == null) return 0;

            var employeeIds = _daysWorkingRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.IsAssigned == true)
                .Where(x => x.Day.ToLower() == date.DayOfWeek.ToString().ToLower())
                .Select(x => x.EmployeeId)
                .ToList();

            var employeeOnLeaveCount = _leaveRequestRepository.Table
                .Where(x => employeeIds.Contains(x.EmployeeId))
                .Join(_requestedDateRepository.Table, l => l.Id, r => r.LeaveRequestId, (l, r) => new { l.EmployeeId, r })
                .Where(x => x.r.Date.Date == date.Date)
                .Where(x => x.r.StatusCode == (int)RequestStatus.Accepted)
                .Distinct()
                .Count();

            var assignmentCount = _assignmentRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.BusinessRoleId == businessRoleId)
                .Where(x => x.StartDate > date.AddDays(6))
                .Where(x => x.EndDate >= date.Date)
                .Count();

            return employeeIds.Count - employeeOnLeaveCount + assignmentCount;            
        }

        public int GetStaffCount(int departmentId, DateTime date)
        {
            var department = GetDepartmentById(departmentId);
            if (department == null) return 0;

            var employeeIds = _daysWorkingRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.IsAssigned == true)
                .Where(x => x.Day.ToLower() == date.DayOfWeek.ToString().ToLower())
                .Select(x => x.EmployeeId)
                .ToList();

            var employeeOnLeaveCount = _leaveRequestRepository.Table
                .Where(x => employeeIds.Contains(x.EmployeeId))
                .Join(_requestedDateRepository.Table, l => l.Id, r => r.LeaveRequestId, (l, r) => new { l.EmployeeId, r })
                .Where(x => x.r.Date.Date == date.Date)
                .Where(x => x.r.StatusCode == (int)RequestStatus.Accepted)
                .Distinct()
                .Count();

            return employeeIds.Count - employeeOnLeaveCount;            
        }

        public int GetStaffLevelStatus(int departmentId, int businessRoleId, DateTime date)
        {
            var deparmtnetCount = GetStaffCount(departmentId, date);

            var department = GetDepartmentById(departmentId);

            if (department == null || department.MinimumRequired > deparmtnetCount)
                return (int)StaffLevelStatus.Critical;
            else
            {
                var br_count = GetStaffCount(departmentId, date, businessRoleId);
                var businessRole_Dep = _businessRoleDepartmentRepository.Table
                    .Where(x => x.BusinessRoleId == businessRoleId)
                    .Where(x => x.DepartmentId == departmentId)
                    .FirstOrDefault();
                
                if (businessRole_Dep != null && br_count <= businessRole_Dep.MinimumRequired)
                    return (int)StaffLevelStatus.Decent;
            }

            return (int)StaffLevelStatus.Acceptable;
        }

        public int[] GetEmployeeIds(int departmentId, DateTime date)
        {
            var employeeIdsAssignment = _assignmentRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.StartDate.Date == date.Date)
                .Select(x => x.EmployeeId);

            var employeeIdsDefault = _daysWorkingRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.Day.ToLower() == date.DayOfWeek.ToString().ToLower())
                .Select(x => x.EmployeeId);

            return employeeIdsDefault.Concat(employeeIdsAssignment).Distinct().ToArray();

            //return _assignmentRepository.GetManyOnDepartment(departmentId, date)
            //                            .Select(a => a.EmployeeId)
            //                            .Concat(_daysWorkingRepository.GetEmployeeIds(departmentId, date.DayOfWeek.ToString()))
            //                            .Distinct()
            //                            .ToArray();
        }

        public int[] GetEmployeeIdsOnHolidays(int departmentId, DateTime date)
        {
            return GetEmployeeIds(departmentId, date)
                    .Intersect(GetEmployeeIdsOnHoliday(date))
                    .ToArray();
        }

        public int[] GetEmployeeIdsOnHoliday(DateTime date)
        {
            var leaveRequestIds = _requestedDateRepository.Table
                .Where(x => x.Date == date.Date)
                .Where(x => x.StatusCode == (int)RequestStatus.Accepted)
                .Select(x => x.LeaveRequestId)
                .ToList();

            var employeeIds = _leaveRequestRepository.Table
                .Where(x => leaveRequestIds.Contains(x.Id))
                .Select(x => x.EmployeeId)
                .ToList()
                .ToArray();

            return employeeIds;

            //return _leaveRequestRepository.GetMany(date, (int)RequestStatus.Accepted)
            //                        .Select(r => r.EmployeeId)
            //                        .ToArray();
        }        
    }
}