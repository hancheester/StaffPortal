using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using StaffPortal.Service.Departments;
using StaffPortal.Service.Errors;
using StaffPortal.Service.Events;
using StaffPortal.Service.Leave;
using StaffPortal.Service.Message;
using StaffPortal.Service.Roles;
using StaffPortal.Common;
using StaffPortal.Common.APIModels;
using StaffPortal.Common.Models;
using StaffPortal.Common.ViewModels;
using StaffPortal.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Service.Staff
{
    public class EmployeeService : IEmployeeService, IConsumer<EntityDeletedEvent<BusinessRole>>
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMemoryCache _memoryCache;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkingDaysService _daysWorkingService;        
        private readonly ILeaveService _leaveService;
        private readonly IDepartmentService _departmentService;
        private readonly IAttendanceService _attendanceService;
        private readonly IBusinessRoleService _businessRoleService;
        private readonly IEmailSender _emailSender;        
        private readonly IErrorService _errorService;
        private readonly IRepository<LeaveRequest> _leaveRequestRepositoryNew;
        private readonly IRepository<RequestedDate> _requestedDateRepository;
        private readonly IRepository<LeaveType> _leaveTypeRepositoryNew;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<BusinessRole> _businessRoleRepository;
        private readonly IRepository<Employee_BusinessRole> _employeeBusinessRoleRepository;
        private readonly IRepository<BusinessRole_Department> _businessRoleDepartmentRepository;
        private readonly IRepository<WorkingDay> _workingDaysRepository;
        private readonly IRepository<Assignment> _assignmentRepository;
        private readonly IRepository<Invitation> _invitationRepository;
        private readonly IRepository<LastTimestamp_Employee> _lastTimeStampEmployeeRepository;
        private readonly IRepository<OpeningHour> _openingHourRepository;
        private readonly IRepository<TimeclockTimestamp> _timeclockTimestampRepository;

        public EmployeeService(
            IWorkingDaysService daysWorkingService,            
            ILeaveService leaveService,
            IDepartmentService departmentService,
            IAttendanceService attendanceService,
            IBusinessRoleService businessRoleService,
            IEmailSender emailSender,            
            IErrorService errorService,
            IRepository<WorkingDay> workingDaysRepository,
            IRepository<LeaveRequest> leaveRequestRepositoryNew,
            IRepository<RequestedDate> requestedDateRepository,
            IRepository<LeaveType> leaveTypeRepositoryNew,
            IRepository<Employee> employeeRepository,
            IRepository<BusinessRole> businessRoleRepository,
            IRepository<Employee_BusinessRole> employeeBusinessRoleRepository,
            IRepository<BusinessRole_Department> businessRoleDepartmentRepository,
            IRepository<Assignment> assignmentRepository,
            IRepository<Invitation> invitationRepository,
            IRepository<LastTimestamp_Employee> lastTimeStampEmployeeRepository,
            IRepository<OpeningHour> openingHourRepository,
            IRepository<TimeclockTimestamp> timeclockTimestampRepository,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMemoryCache memoryCache,
            IEventPublisher eventPublisher)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _memoryCache = memoryCache;
            _eventPublisher = eventPublisher;
            _daysWorkingService = daysWorkingService;            
            _leaveService = leaveService;
            _departmentService = departmentService;
            _attendanceService = attendanceService;
            _businessRoleService = businessRoleService;
            _assignmentRepository = assignmentRepository;
            _invitationRepository = invitationRepository;            
            _emailSender = emailSender;
            _timeclockTimestampRepository = timeclockTimestampRepository;
            _lastTimeStampEmployeeRepository = lastTimeStampEmployeeRepository;
            _errorService = errorService;
            _leaveRequestRepositoryNew = leaveRequestRepositoryNew;
            _requestedDateRepository = requestedDateRepository;
            _leaveTypeRepositoryNew = leaveTypeRepositoryNew;
            _employeeRepository = employeeRepository;
            _businessRoleRepository = businessRoleRepository;
            _employeeBusinessRoleRepository = employeeBusinessRoleRepository;
            _businessRoleDepartmentRepository = businessRoleDepartmentRepository;
            _workingDaysRepository = workingDaysRepository;
            _openingHourRepository = openingHourRepository;
        }

        public async Task<OperationResult> DeleteAsync(int employeeId)
        {
            var result = new OperationResult();

            try
            {
                var employee = _employeeRepository.Return(employeeId);
                if (employee == null) return result;

                var user = await _userManager.FindByIdAsync(employee.UserId);
                if (user != null)
                {
                    var identityResult = await _userManager.DeleteAsync(user);
                    if (identityResult.Succeeded == false)
                    {
                        foreach (var error in identityResult.Errors)
                        {
                            result.AddOperationError(error.Code, error.Description);
                            _errorService.Insert(new ErrorLog($"{error.Code}: {error.Description}", string.Empty));
                        }

                        return result;
                    }
                }

                var workingDays = _workingDaysRepository.Table.Where(x => x.EmployeeId == employeeId).ToList();
                if (workingDays.Count() > 0)
                {
                    foreach (var workingDay in workingDays)
                    {
                        _workingDaysRepository.Delete(workingDay);
                    }
                }

                var roles = _employeeBusinessRoleRepository.Table.Where(x => x.EmployeeId == employeeId).ToList();
                if (roles.Count() > 0)
                {
                    foreach (var role in roles)
                    {
                        _employeeBusinessRoleRepository.Delete(role);
                    }
                }

                _eventPublisher.EntityDeleted(employee);
            }
            catch (Exception ex)
            {
                result.AddOperationError("E2", "Failed to delete employee.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public IList<Employee> GetEmployees(string userId, string firstName, string lastName, string email, out int total, int pageNumber = 1, int pageSize = 10)
        {
            total = 0;

            try
            {
                var query = _employeeRepository.Table
                    .Join(_userManager.Users, e => e.UserId, u => u.Id, (e, u) => new { e, u });

                if (!string.IsNullOrEmpty(userId)) query = query.Where(x => x.u.Id.ToLower().Contains(userId.ToLower()));
                if (!string.IsNullOrEmpty(firstName)) query = query.Where(x => x.e.FirstName.ToLower().Contains(firstName.ToLower()));
                if (!string.IsNullOrEmpty(lastName)) query = query.Where(x => x.e.LastName.ToLower().Contains(lastName.ToLower()));
                if (!string.IsNullOrEmpty(email)) query = query.Where(x => x.u.NormalizedEmail.Contains(email.ToLower()));

                total = query.Count();

                var users = query
                    .OrderBy(x => x.e.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(x =>
                    {
                        x.e.User = x.u;
                        return x.e;
                    })
                    .ToList();

                return users;
            }
            catch (Exception ex)
            {
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return new List<Employee>();
        }

        public IList<Employee> GetAll()
        {
            IList<Employee> employees = _employeeRepository.Table.ToList();

            foreach (var employee in employees)
            {
                var user = Task.Run(() => _userManager.FindByIdAsync(employee.UserId)).Result;
                if (user != null)
                    employee.User = user;

                var daysWorking = _daysWorkingService.GetWeek(employee.Id);
                if (daysWorking != null)
                    employee.WorkingDays = daysWorking;
            }

            return employees.ToList();
        }

        public IList<EmployeeInfoAPIModel> GetAll(int departmentId, int businessRoleId)
        {
            var all = _employeeRepository.Table.ToList()
                    .Select(e =>
                    ToEmployeeInfoAPIModel(e))
                    .Where(e => (businessRoleId == 0 || e.PrimaryBusinessRole.Id == businessRoleId)
                    && (e.AssignedDepartments.Any(d => departmentId == 0 || d.Id == departmentId)))
                    .ToList();

            return all;
        }

        public IList<EmployeeInfoAPIModel> GetAll(AnnouncementRequest query)
        {
            var recipients = _employeeRepository.Table.ToList()
                    .Select(e =>
                    ToEmployeeInfoAPIModel(e))
                    .Where(e => (query.BusinessRolesIds.Any(id => id == e.PrimaryBusinessRole.Id)
                    && query.DepartmentsIds.Any(id => e.AssignedDepartments.Any(d => d.Id == id))))
                    .ToList();

            return recipients;
        }

        public PagedList<Employee> GetPagedList(int pageIndex, int numberOfItems)
        {
            var employees = _employeeRepository.Table
                .Skip((pageIndex - 1) * numberOfItems)
                .Take(numberOfItems)
                .ToList();

            var result = new PagedList<Employee>
            {
                PageIndex = pageIndex,
                TotalItems = _employeeRepository.Table.Count(),
                Items = employees.ToList()
            };

            return result;
        }

        private EmployeeInfoAPIModel ToEmployeeInfoAPIModel(Employee e)
        {
            var user = Task.Run(() => _userManager.FindByIdAsync(e.UserId)).Result;

            var primaryRole = _businessRoleService.GetPrimaryBusinessRoleByEmployeeId(e.Id);

            return new EmployeeInfoAPIModel
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Gender = e.Gender,
                ApplicationUserId = user.Id,
                NIN = e.NIN,
                DOB = e.DOB,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                HolidayAllowance = e.HolidayAllowance,
                Barcode = e.Barcode,
                HoursRequired = e.HoursRequired,
                PrimaryBusinessRole = primaryRole,
                AssignedDepartments = Task.Run(() => _departmentService.GetAssignedDepartmentsByEmployeeIdAsync(e.Id)).Result
            };
        }

        public void Update(Employee employee)
        {
            _employeeRepository.Update(employee);
        }

        public async Task<OperationResult<Employee>> Register(Employee employee, string username, string email, string phoneNumber, string password, int primaryBusinessRoleId, int[] secondaryBusinessRoleIds)
        {
            var result = new OperationResult<Employee>(employee);

            try
            {
                var user = new IdentityUser
                {
                    UserName = username,
                    Email = email,
                    PhoneNumber = phoneNumber
                };

                var identityResult = await _userManager.CreateAsync(user, password);

                if (identityResult.Succeeded == false)
                {
                    foreach (var error in identityResult.Errors)
                    {
                        result.AddOperationError(error.Code, error.Description);
                        _errorService.Insert(new ErrorLog($"{error.Code}: {error.Description}", string.Empty));
                    } 
                    
                    return result;
                }

                employee.UserId = user.Id;
                employee.Id = _employeeRepository.Create(employee);

                if (employee.WorkingDays != null && employee.WorkingDays.Count > 0)
                {
                    foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                    {
                        var workingDay = employee.WorkingDays.Where(x => x.Day == day.ToString()).FirstOrDefault();

                        if (workingDay != null)
                        {
                            workingDay.EmployeeId = employee.Id;
                            _workingDaysRepository.Create(workingDay);
                        }
                        else
                        {
                            _workingDaysRepository.Create(new WorkingDay
                            {
                                EmployeeId = employee.Id,
                                Day = day.ToString()
                            });
                        }
                    }
                }

                if (primaryBusinessRoleId > 0)
                {
                    var primaryRole = new Employee_BusinessRole
                    {
                        EmployeeId = employee.Id,
                        BusinessRoleId = primaryBusinessRoleId,
                        IsPrimary = true
                    };

                    _employeeBusinessRoleRepository.Create(primaryRole);
                }

                foreach (var roleId in secondaryBusinessRoleIds)
                {
                    _employeeBusinessRoleRepository.Create(new Employee_BusinessRole
                    {
                        EmployeeId = employee.Id,
                        BusinessRoleId = roleId,
                    });
                }
            }
            catch (Exception ex)
            {
                result.AddOperationError("E1", "Failed to register employee.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public async Task<OperationResult<Employee>> Update(Employee employee, string username, string email, string password, int primaryBusinessRoleId, int[] secondaryBusinessRoleIds)
        {
            var result = new OperationResult<Employee>(employee);

            try
            {
                var foundEmployee = _employeeRepository.Return(employee.Id);
                if (foundEmployee == null)
                {
                    result.AddOperationError("E1", "Employee not found.");
                    return result;
                }

                foundEmployee.FirstName = employee.FirstName;
                foundEmployee.LastName = employee.LastName;
                foundEmployee.Gender = employee.Gender;
                foundEmployee.NIN = employee.NIN;
                foundEmployee.DOB = employee.DOB;
                foundEmployee.StartDate = employee.StartDate;
                foundEmployee.EndDate = employee.EndDate;
                foundEmployee.HolidayAllowance = employee.HolidayAllowance;
                foundEmployee.Barcode = employee.Barcode;
                foundEmployee.HoursRequired = employee.HoursRequired;

                _employeeRepository.Update(foundEmployee);

                var user = await _userManager.FindByIdAsync(foundEmployee.UserId);
                if(user == null)
                {
                    result.AddOperationError("E1", "Membership not found.");
                    return result;
                }

                if (user.UserName != username || user.Email != email)
                {
                    user.UserName = username;
                    user.Email = email;
                    var identityResult = await _userManager.UpdateAsync(user);

                    if (identityResult.Succeeded == false)
                    {
                        foreach (var error in identityResult.Errors)
                        {
                            result.AddOperationError(error.Code, error.Description);
                            _errorService.Insert(new ErrorLog($"{error.Code}: {error.Description}", string.Empty));
                        }

                        return result;
                    }
                }

                if (string.IsNullOrEmpty(password) == false)
                {
                    await _userManager.RemovePasswordAsync(user);
                    await _userManager.AddPasswordAsync(user, password);
                }

                var foundWorkingDays = _workingDaysRepository.Table
                    .Where(x => x.EmployeeId == employee.Id)
                    .ToList();

                if (foundWorkingDays.Count() > 0)
                {
                    foreach (var workingDay in foundWorkingDays)
                    {
                        _workingDaysRepository.Delete(workingDay);
                    }
                }

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var workingDay = employee.WorkingDays.Where(x => x.Day == day.ToString()).FirstOrDefault();

                    if (workingDay != null)
                    {
                        workingDay.EmployeeId = employee.Id;
                        _workingDaysRepository.Create(workingDay);
                    }
                    else
                    {
                        _workingDaysRepository.Create(new WorkingDay
                        {
                            EmployeeId = employee.Id,
                            Day = day.ToString()
                        });
                    }
                }

                var foundRoles = _employeeBusinessRoleRepository.Table
                    .Where(x => x.EmployeeId == employee.Id)
                    .ToList();

                if (foundRoles.Count() > 0)
                {
                    foreach (var role in foundRoles)
                    {
                        _employeeBusinessRoleRepository.Delete(role);
                    }                    
                }

                var primaryRole = new Employee_BusinessRole
                {
                    EmployeeId = employee.Id,
                    BusinessRoleId = primaryBusinessRoleId,
                    IsPrimary = true
                };

                _employeeBusinessRoleRepository.Create(primaryRole);

                foreach (var roleId in secondaryBusinessRoleIds)
                {
                    _employeeBusinessRoleRepository.Create(new Employee_BusinessRole
                    {
                        EmployeeId = employee.Id,
                        BusinessRoleId = roleId,
                    });
                }
            }
            catch (Exception ex)
            {
                result.AddOperationError("E1", "Failed to update employee.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public async Task<OperationResult> UpdateMyAccount(int employeeId, string firstName, string lastName, string phoneNumber, string gender, string nin, string email, string password)
        {
            var result = new OperationResult();

            try
            {
                var foundEmployee = _employeeRepository.Return(employeeId);
                if (foundEmployee == null)
                {
                    result.AddOperationError("E1", "Employee not found.");
                    return result;
                }

                foundEmployee.FirstName = firstName;
                foundEmployee.LastName = lastName;
                foundEmployee.Gender = gender;
                foundEmployee.NIN = nin;

                _employeeRepository.Update(foundEmployee);

                var user = await _userManager.FindByIdAsync(foundEmployee.UserId);
                if (user == null)
                {
                    result.AddOperationError("E1", "Membership not found.");
                    return result;
                }

                if (user.PhoneNumber != phoneNumber || user.Email != email)
                {
                    user.Email = email;
                    user.PhoneNumber = phoneNumber;

                    var identityResult = await _userManager.UpdateAsync(user);

                    if (identityResult.Succeeded == false)
                    {
                        foreach (var error in identityResult.Errors)
                        {
                            result.AddOperationError(error.Code, error.Description);
                            _errorService.Insert(new ErrorLog($"{error.Code}: {error.Description}", string.Empty));
                        }

                        return result;
                    }
                }

                if (string.IsNullOrEmpty(password) == false)
                {
                    await _userManager.RemovePasswordAsync(user);
                    await _userManager.AddPasswordAsync(user, password);
                }                
            }
            catch (Exception ex)
            {
                result.AddOperationError("E1", "Failed to update employee.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public DaysWorkingResult InsertWeeklyHours(IList<DaysWorkingAPIModel> weekHours, int employeeId, string userId)
        {
            DaysWorkingResult result = new DaysWorkingResult
            {
                EmployeeId = employeeId,
                UserId = userId,
                Succeded = true,
                DayWorkingResult = new List<DayWorkingResult>()

            };
            foreach (var workingDay in weekHours)
            {
                try
                {
                    var depOpeningHours = _openingHourRepository.Table
                        .Where(x => x.DepartmentId == workingDay.DepartmentId)
                        .Where(x => x.Day == workingDay.Day)
                        .FirstOrDefault();
                    workingDay.EmployeeId = employeeId;
                    if (workingDay.IsAssigned && !depOpeningHours.IsOpen)
                    {
                        result.Succeded = false;
                        result.DayWorkingResult.Add(new DayWorkingResult
                        {
                            DepartmentId = depOpeningHours.DepartmentId,
                            Day = workingDay.Day,
                            Message = string.Format("On the required day '{0}' department is closed.", workingDay.Day)
                        });
                    }
                }
                catch (Exception ex)
                {

                }
            }

            if (result.Succeded)
                foreach (var workingDay in weekHours)
                {
                    WorkingDay day = Mapper.Map<WorkingDay>(workingDay);
                    var type = day.GetType();
                    _workingDaysRepository.Create(day);
                }

            return result;
        }

        public DaysWorkingResult EditWeeklyHours(IList<DaysWorkingAPIModel> weekHours, int employeeId, string userId)
        {
            DaysWorkingResult result = new DaysWorkingResult
            {
                EmployeeId = employeeId,
                UserId = userId,
                Succeded = true,
                DayWorkingResult = new List<DayWorkingResult>()

            };
            foreach (var workingDay in weekHours)
            {
                try
                {
                    var depOpeningHours = _openingHourRepository.Table
                        .Where(x => x.DepartmentId == workingDay.DepartmentId)
                        .Where(x => x.Day == workingDay.Day)
                        .FirstOrDefault();
                    workingDay.EmployeeId = employeeId;
                    if (workingDay.IsAssigned && !depOpeningHours.IsOpen)
                    {
                        result.Succeded = false;
                        result.DayWorkingResult.Add(new DayWorkingResult
                        {
                            DepartmentId = depOpeningHours.DepartmentId,
                            Day = workingDay.Day,
                            Message = string.Format("On the required day '{0}' department is closed.", workingDay.Day)
                        });
                    }
                }
                catch (Exception ex)
                {

                }
            }

            if (result.Succeded)
                foreach (var workingDay in weekHours)
                {
                    var entity = _workingDaysRepository.Return(workingDay.Id);
                    entity.DepartmentId = workingDay.DepartmentId;
                    entity.StartTime = workingDay.StartTime;
                    entity.EndTime = workingDay.EndTime;
                    entity.IsAssigned = workingDay.IsAssigned;
                    _workingDaysRepository.Update(entity);
                }

            return result;
        }

        private OperationResult<Employee> ValidateAndCreateEmployee(Employee employee)
        {
            OperationResult<Employee> result = new OperationResult<Employee>
            {
                Object = employee,
                Succeeded = true,
                ErrorMessages = new Dictionary<string, string>()
            };

            var found = _employeeRepository.Table
                .Where(x => string.Equals(x.Barcode, employee.Barcode, StringComparison.InvariantCultureIgnoreCase))
                .Any();

            if (found)
                result.AddOperationError("Barcode", string.Format("Barcode '{0}' is duplicated.", employee.Barcode));

            return result;
        }

        public decimal GetTotalLeaveQuota(int employeeId)
        {
            return _leaveService.GetTotalLeaveQuota(employeeId);
        }

        public decimal GetRemainingLeaveQuota(int employeeId)
        {
            return _leaveService.GetRemainingLeaveQuota(employeeId);
        }

        public Employee GetEmployeeById(int employeeId)
        {
            var employee = _employeeRepository.Return(employeeId);

            var user = Task.Run(() => _userManager.FindByIdAsync(employee.UserId)).Result;
            if (user != null)
                employee.User = user;

            var daysWorking = _workingDaysRepository.Table
                .Where(x => x.EmployeeId == employee.Id)
                .ToList();

            if (daysWorking != null)
                employee.WorkingDays = daysWorking;

            return employee;
        }

        public EmployeeInfoAPIModel GetAsAPIModel(int employeeId)
        {
            var e = GetEmployeeById(employeeId);

            return ToEmployeeInfoAPIModel(e);
        }

        public EmployeeInfoAPIModel GetAsAPIModel(string userId)
        {
            var e = GetEmployeeByApplicationUserId(userId);

            return ToEmployeeInfoAPIModel(e);
        }
        
        public Employee GetEmployeeByApplicationUserId(string userId)
        {
            var employee = _employeeRepository.Table.Where(x => x.UserId == userId).FirstOrDefault();
            if (employee != null)
            {

                var user = Task.Run(() => _userManager.FindByIdAsync(employee.UserId)).Result;
                if (user != null)
                    employee.User = user;

                var daysWorking = _workingDaysRepository.Table
                    .Where(x => x.EmployeeId == employee.Id)
                    .ToList();

                if (daysWorking != null)
                    employee.WorkingDays = daysWorking;
            }

            return employee;
        }

        public int GetEmployeeIdOnApplicationUserId(string userId)
        {
            return _employeeRepository.Table.Where(x => x.UserId == userId).Select(x => x.Id).FirstOrDefault();
        }

        public void CreateEmployee(string userId)
        {
            var employee = new Employee();
            employee.UserId = userId;
            _employeeRepository.Create(employee);

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                var workingDay = new WorkingDay
                {
                    Day = day.ToString(),
                    DepartmentId = -1,
                    EmployeeId = employee.Id,
                    StartTime = new TimeSpan(0),
                    EndTime = new TimeSpan(0),
                    IsAssigned = false
                };
            }
        }

        public async Task<EditEmployeeInfoAPIModel> PrepareUserToEdit(string applicationUserId)
        {
            var employee = GetEmployeeByApplicationUserId(applicationUserId);

            if (employee == null)
            {
                employee = new Employee();
                employee.User = Task.Run(() => _userManager.FindByIdAsync(employee.UserId)).Result;
            }

            EditEmployeeInfoAPIModel model = Mapper.Map<EditEmployeeInfoAPIModel>(employee);
            model.UserId = applicationUserId;

            // Load DAYS WORKING
            List<WorkingDay> daysWorking = _daysWorkingService.GetWeek(model.Id).ToList();

            if (daysWorking != null && daysWorking.Count > 0)
                model.DaysWorking = Mapper.Map<List<DaysWorkingAPIModel>>(daysWorking);
            else
            {
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var workingDay = new WorkingDay
                    {
                        Day = day.ToString(),
                        DepartmentId = -1,
                        EmployeeId = employee.Id,
                        StartTime = new TimeSpan(0),
                        EndTime = new TimeSpan(0),
                        IsAssigned = false
                    };
                }
                model.DaysWorking = Mapper.Map<List<DaysWorkingAPIModel>>(_workingDaysRepository.Table.Where(x => x.EmployeeId == employee.Id).ToList());
            }

            // Load PRIMARY BUSINESS ROLE
            model.PrimaryBusinessRole = _businessRoleService.GetPrimaryBusinessRoleByEmployeeId(employee.Id);

            // Load SYSTEM ROLE
            var systemRoles = await _userManager.GetRolesAsync(model.User);
            var selectedSystemRole = systemRoles.FirstOrDefault();

            model.SystemRole = _roleManager.Roles.FirstOrDefault(r => string.Equals(r.Name, selectedSystemRole));

            // Load SECONDARY BUSINESS ROLES
            var businessRoles = _businessRoleService.GetAll().ToList();
            var userBusinessRoles = _businessRoleService.GetSecondaryBusinessRolesOnEmployeeId(employee.Id);
            model.SecondaryBusinessRoles = new List<SecondaryBusinessRoleAPIModel>();

            foreach (var businessRole in businessRoles)
            {
                SecondaryBusinessRoleAPIModel secondarybr = new SecondaryBusinessRoleAPIModel
                {
                    Id = businessRole.Id,
                    IsChecked = false,
                    Name = businessRole.Name
                };
                if (userBusinessRoles.Any(br => br.Id == businessRole.Id))
                    secondarybr.IsChecked = true;
                model.SecondaryBusinessRoles.Add(secondarybr);
            }

            return model;
        }

        // TODO: Use different Methods and ViewModel for different requests
        // LeaveRequestModel: request info + employee info + is approver or applicant employee

        //public Registration_Admin ToRegistrationAdmin(ApplicationUser user, string applicant_FirstName, string applicant_LastName)
        //{
        //    Registration_Admin model = new Registration_Admin(user.FirstName, user.LastName, user.Email);

        //    model.BusinessRoleName = "Administrator";
        //    model.Applicant_FirstName = applicant_FirstName;
        //    model.Applicant_LastName = applicant_LastName;

        //    return model;
        //}

        //public Registration_Registrant ToRegistrationRegistrant(ApplicationUser user)
        //{
        //    Registration_Registrant model = new Registration_Registrant(user.FirstName, user.LastName, user.Email);

        //    return model;
        //}
                
        /// <summary>
        /// Returns the information about the given department, starting from a specific date, for an entire week.
        /// </summary>
        /// <param name="fromDate">It can be any date from which it's intended the week to start.</param>
        public WeeklyShiftAPIModel GetWeeklyShiftWithAssignments(int departmentId, DateTime fromDate)
        {
            // get all days working where department id, group by EmployeeId
            var daysWorking = _daysWorkingService.GetWeeklyRotaByDepartmentId(departmentId, fromDate);

            var admins = Task.Run(() => _userManager.GetUsersInRoleAsync("SuperAdmin")).Result;
            var adminEmployeeIds = new List<int>();
            if (admins != null && admins.Count > 0)
            {
                foreach (var admin in admins)
                {
                    var employeeId = GetEmployeeIdOnApplicationUserId(admin.Id);
                    adminEmployeeIds.Add(employeeId);
                }
            }
            
            var employeesOnShift = daysWorking
                                .Where(x => adminEmployeeIds.Contains(x.EmployeeId) == false)
                                .GroupBy(d => d.EmployeeId,
                                (id, group) =>
                                ToEmployeeOnShiftAPIModel(id, group.ToList(), fromDate))
                                .ToList();

            var assignments = _assignmentRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.StartDate < fromDate.AddDays(6))
                .Where(x => x.EndDate >= fromDate)
                .GroupBy(a => new
                                {
                                    a.EmployeeId,
                                    a.BusinessRoleId
                                })
                                .Select(g =>
                                ToEmployeeOnShiftAPIModel(g.Key.EmployeeId, g.Key.BusinessRoleId, g.ToList(), fromDate))
                                .ToList();

            employeesOnShift.AddRange(assignments);

            var businessRole_Employees = employeesOnShift
                                        .GroupBy(e => e.BusinessRole,
                                        (key, group) =>
                                        ToBusinessRoleEmployeesOnShiftAPIModel(key, group, departmentId, fromDate))
                                        .ToList();

            var minimumRequired = 0;
            var department = _departmentService.GetDepartmentById(departmentId);
            if (department != null)
            {
                minimumRequired = department.MinimumRequired;
            }

            var weeklyShiftAPIModel = new WeeklyShiftAPIModel
            {
                DepartmentMinRequired = minimumRequired,
                BusinessRole_Employees = businessRole_Employees,
                OpeningHours = _openingHourRepository.Table.Where(x => x.DepartmentId == departmentId).ToList()
            };

            return weeklyShiftAPIModel;
        }

        /// <summary>
        /// Returns the information over a specific department and a given business role, for an entire week.
        /// </summary>
        /// <param name="fromDate">It can be any date from which it's intended the week to start.</param>
        public WeeklyShiftAPIModel GetWeeklyShiftWithAssignments(int departmentId, int businessRoleId, DateTime fromDate)
        {
            // get all days working where department id, group by EmployeeId
            var daysWorking = _daysWorkingService.GetWeeklyRotaByDepartmentId(departmentId, fromDate);

            var employeesOnShift = daysWorking
                                .GroupBy(d => d.EmployeeId,
                                (id, group) =>
                                ToEmployeeOnShiftAPIModel(id, group.ToList(), fromDate))
                                .ToList();

            var assignments = _assignmentRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.StartDate < fromDate.AddDays(6))
                .Where(x => x.EndDate >= fromDate)
                .GroupBy(a => new
                {
                    a.EmployeeId,
                    a.BusinessRoleId
                })
                .Select(g =>
                ToEmployeeOnShiftAPIModel(g.Key.EmployeeId, g.Key.BusinessRoleId, g.ToList(), fromDate))
                .ToList();                                

            employeesOnShift.AddRange(assignments);

            var businessRole_Employees = employeesOnShift
                                        .Where(e => e.BusinessRole.Id == businessRoleId)
                                        .GroupBy(e => e.BusinessRole,
                                        (key, group) =>
                                        ToBusinessRoleEmployeesOnShiftAPIModel(key, group, departmentId, fromDate))
                                        .ToList();

            var weeklyShiftAPIModel = new WeeklyShiftAPIModel
            {
                DepartmentMinRequired = _departmentService.GetDepartmentById(departmentId).MinimumRequired,
                BusinessRole_Employees = businessRole_Employees,
                OpeningHours = _openingHourRepository.Table.Where(x => x.DepartmentId == departmentId).ToList()
            };

            return weeklyShiftAPIModel;
        }

        /// <summary>
        /// Returns the information over a specific department, business role and employee (eventually the employee might be assigned in different roles), for an entire week.
        /// </summary>
        /// <param name="fromDate">It can be any date from which it's intended the week to start.</param>
        public WeeklyShiftAPIModel GetWeeklyShiftWithAssignments(int departmentId, int businessRoleId, int employeeId, DateTime fromDate)
        {
            // get all days working where department id, group by EmployeeId
            var daysWorking = _daysWorkingService.GetWeeklyRotaByDepartmentId(departmentId, fromDate);

            var employeesOnShift = daysWorking
                                    .Where(d => d.EmployeeId == employeeId)
                                    .GroupBy(d => d.EmployeeId,
                                    (id, group) =>
                                    ToEmployeeOnShiftAPIModel(id, group.ToList(), fromDate))
                                    .ToList();

            var assignments = _assignmentRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.StartDate < fromDate.AddDays(6))
                .Where(x => x.EndDate >= fromDate)                
                .GroupBy(a => new
                {
                    a.EmployeeId,
                    a.BusinessRoleId
                })
                .Select(g =>
                ToEmployeeOnShiftAPIModel(g.Key.EmployeeId, g.Key.BusinessRoleId, g.ToList(), fromDate))
                .ToList();

            employeesOnShift.AddRange(assignments);
            var businessRole_Employees = new List<BusinessRoleEmployeesOnShiftAPIModel>();
            if (businessRoleId > 0)
                businessRole_Employees = employeesOnShift
                                            .Where(e => e.BusinessRole.Id == businessRoleId)
                                            .GroupBy(e => e.BusinessRole,
                                            (key, group) =>
                                            ToBusinessRoleEmployeesOnShiftAPIModel(key, group, departmentId, fromDate))
                                            .ToList();
            else
                businessRole_Employees = employeesOnShift
                                            .GroupBy(e => e.BusinessRole,
                                            (key, group) =>
                                            ToBusinessRoleEmployeesOnShiftAPIModel(key, group, departmentId, fromDate))
                                            .ToList();

            var weeklyShiftAPIModel = new WeeklyShiftAPIModel
            {
                DepartmentMinRequired = _departmentService.GetDepartmentById(departmentId).MinimumRequired,
                BusinessRole_Employees = businessRole_Employees,
                OpeningHours = _openingHourRepository.Table.Where(x => x.DepartmentId == departmentId).ToList()
            };

            return weeklyShiftAPIModel;
        }

        public ShiftAPIModel GetDailyShift(int employeeId, DateTime date)
        {
            // get all days working where department id, group by EmployeeId

            ShiftAPIModel shift = null;

            var assignment = _assignmentRepository.Table
                .Where(x => x.EmployeeId == employeeId)
                .Where(x => x.StartDate.Date == date.Date)
                .FirstOrDefault();

            if (assignment == null)
            {
                var businessRole = _businessRoleService.GetPrimaryBusinessRoleByEmployeeId(employeeId);
                var department = Task.Run(() => _departmentService.GetAssignedDepartmentsByEmployeeIdAsync(employeeId)).Result;

                var workingDay = _daysWorkingService.GetDayWorking(employeeId, date);
                shift = new ShiftAPIModel
                {
                    BusinessRoleName = businessRole.Name,
                    Date = date,
                    DepartmentName = department[0].Name,
                    StartTime = workingDay.StartTime.Value,
                    EndTime = workingDay.EndTime.Value,
                    IsAssignment = false,
                    IsDayWorking = true
                };
            }
            else
            {
                var businessRole = _businessRoleService.GetBusinessRoleById(assignment.BusinessRoleId);
                var department = _departmentService.GetDepartmentById(assignment.DepartmentId);

                shift = new ShiftAPIModel
                {
                    BusinessRoleName = businessRole.Name,
                    DepartmentName = department.Name,
                    Date = date,
                    //StartTime = assignment.StartTime,
                    //EndTime = assignment.EndTime,
                    IsAssignment = true,
                    IsDayWorking = false
                };
            }

            return shift;
        }

        public IList<EmployeeInfoAPIModel> GetAllInvitedEmployees(int trainingModuleId)
        {
            var employees = _invitationRepository.Table
                .Join(_employeeRepository.Table, i => i.EmployeeId, e => e.Id, (i, e) => new { i, e })
                .Where(x => x.i.TrainingModuleId == trainingModuleId)
                .Select(x => ToEmployeeInfoAPIModel(x.e))
                .ToList();

            return employees;
        }

        public IList<EmployeeInfoAPIModel> GetInvitableEmployees(int departmentId, int businessRoleId, int trainingModuleId)
        {
            var employees = GetAll(departmentId, businessRoleId);
            if (trainingModuleId > 0)
            {
                var invited = GetAllInvitedEmployees(trainingModuleId);

                employees = employees
                            .Except(invited, new EmployeeInfoComparer())
                            .ToList();
            }

            return employees;
        }

        public OperationResult<EmployeeInfoAPIModel> GetEmployeeByBarcode(string barcode)
        {
            var model = _employeeRepository.Table.Where(x => string.Equals(x.Barcode, barcode, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            OperationResult<EmployeeInfoAPIModel> result = null;
            if (model != null)
                result = new OperationResult<EmployeeInfoAPIModel>(ToEmployeeInfoAPIModel(model));

            if (result == null || result.Succeeded)
                return result;
            else if (result.Object == null)
            {
                result.AddOperationError("Not Found", "Could not found any employee with the given barcode.");
                return result;
            }

            result.AddOperationError("Unknown", "Unknown Error, please try again later.");
            return result;
        }

        #region TIMECLOCKTIMESTAMP

        public TimeclockTimestamp InsertTimeclockTimeStamp(int employeeId)
        {
            var lastTimeStamp = _lastTimeStampEmployeeRepository.Table.Where(x => x.EmployeeId == employeeId).FirstOrDefault();
            var lastTimeclock = _timeclockTimestampRepository.Return(lastTimeStamp.LastTimestampId);

            TimeclockTimestamp newTimeStamp = null;

            if (lastTimeclock != null)
                newTimeStamp = new TimeclockTimestamp(employeeId, !lastTimeclock.IsClockIn);
            else
                newTimeStamp = new TimeclockTimestamp(employeeId, true);

            _timeclockTimestampRepository.Create(newTimeStamp);

            return newTimeStamp;
        }

        public IList<Tuple<TimesheetDetails, TimesheetDetails>> GetTimesheet(int employeeId, DateTime from, DateTime to)
        {
            var employee = ToEmployeeInfoAPIModel(GetEmployeeById(employeeId));

            var timesheet = _timeclockTimestampRepository.Table
                .Where(x => x.EmployeeId == employeeId)
                .Where(x => x.IsClockIn)
                .Where(x => (DateTime.Compare(x.Timestamp.Date, from.Date) >= 0))
                .Where(x => DateTime.Compare(x.Timestamp.Date, to.Date) <= 0)
                .Select(x => ToTimeSheetDetails(x, employee))
                .ToList();

            return timesheet;
        }

        public IList<Tuple<TimesheetDetails, TimesheetDetails>> GetTimesheet(int departmentId, int businessRoleId, DateTime date)
        {
            var employees = GetAll(departmentId, businessRoleId);

            var timesheet = _timeclockTimestampRepository.Table
                .Join(employees, t => t.EmployeeId, e => e.Id, (t, e) => new { t, e })
                .Where(x => x.t.Timestamp.Date == date.Date)
                .Where(x => x.t.IsClockIn == true)
                .Select(x => ToTimeSheetDetails(x.t, x.e))
                .ToList();

            return timesheet;
        }

        //public IList<TimeclockTimestamp> GetTimesheet(int employeeId, DateTime date)
        //{

        //}

        public void UpdateTimeclockTimestamp(TimeclockTimestamp timestamp)
        {
            _timeclockTimestampRepository.Update(timestamp);
        }

        private Tuple<TimesheetDetails, TimesheetDetails> ToTimeSheetDetails(TimeclockTimestamp timeclock, EmployeeInfoAPIModel employee)
        {
            var clockin = new TimesheetDetails
            {
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                TimeclockId = timeclock.Id,
                EmployeeId = employee.Id,
                BusinessRoleId = employee.PrimaryBusinessRole.Id,
                BusinessRoleName = employee.PrimaryBusinessRole.Name,
                DepartmentId = employee.AssignedDepartments[0].Id,
                DepartmentName = employee.AssignedDepartments[0].Name,
                Timestamp = timeclock.Timestamp,
                Shift = GetDailyShift(employee.Id, timeclock.Timestamp.Date),
                IsClockIn = timeclock.IsClockIn,
                IsApproved = timeclock.IsApproved
            };

            var clockoutEntity = _timeclockTimestampRepository.Return(timeclock.ClockOutRefId);
            var clockout = new TimesheetDetails
            {
                FirstName = clockin.FirstName,
                LastName = clockin.LastName,
                TimeclockId = clockoutEntity.Id,
                EmployeeId = clockin.EmployeeId,
                BusinessRoleId = clockin.BusinessRoleId,
                BusinessRoleName = clockin.BusinessRoleName,
                DepartmentId = clockin.DepartmentId,
                DepartmentName = clockin.DepartmentName,
                Timestamp = clockoutEntity.Timestamp,
                Shift = clockin.Shift,
                IsClockIn = clockoutEntity.IsClockIn,
                IsApproved = clockoutEntity.IsApproved
            };

            return Tuple.Create(clockin, clockout);
        }

        #endregion END TIMECLOCK TIMESTAMP

        #region ASSIGNMENTS

        public OperationResult<Assignment> InsertAssignment(AssignmentAPIModel model)
        {
            //var assignment = Mapper.Map<Assignment>(model);
            //var result = new OperationResult<Assignment>(assignment);
            //assignment.Day = assignment.StartDate.ToString("dddd");
            //assignment.CreatedOnDate = DateTime.Now;
            //assignment.EndDate = assignment.StartDate.AddDays(assignment.RecurringWeeks * 7);

            //_assignmentRepository.Create(assignment);
            //return result;

            return null;
        }

        public void InsertWeekAssignment(int employeeId, int departmentId, int businessRoleId, int recurringWeeks, DateTime startDate)
        {
            var daysWorking = _daysWorkingService.GetWeek(employeeId);
            var date = startDate;

            foreach (var day in daysWorking)
            {

                if (day.IsAssigned)
                {
                    //var assignment = new Assignment
                    //{
                    //    CreatedOnDate = DateTime.Now,
                    //    StartDate = date,
                    //    EndDate = startDate.AddDays(recurringWeeks * 7),
                    //    Day = day.Day,
                    //    DepartmentId = departmentId,
                    //    EmployeeId = employeeId,
                    //    BusinessRoleId = businessRoleId,
                    //    RecurringWeeks = recurringWeeks,
                    //    StartTime = day.StartTime.Value,
                    //    EndTime = day.EndTime.Value
                    //};

                    //_assignmentRepository.Create(assignment);
                }

                date.AddDays(1);
            }
        }

        public IList<Assignment> GetAssignments(int employeeId, DateTime fromDate)
        {
            var assignments = _assignmentRepository.Table
                .Where(x => x.EmployeeId == employeeId)
                .Where(x => x.EndDate >= fromDate)
                .ToList();

            return assignments;
        }

        #endregion END ASSIGNMENTS

        #region ACCOUNTANT REPORTS

        public OperationResult<IList<AccountantReport>> GetAccountantReports(DateTime from, DateTime to)
        {
            var operationResult = new OperationResult<IList<AccountantReport>>();
            var validationResult = ValidateGetAccountantReportsRequest(from, to);

            if (validationResult.Succeeded)
            {
                var employees = _employeeRepository.Table.ToList(); ;

                var reports = new List<AccountantReport>();

                foreach (var e in employees)
                {
                    var report = new AccountantReport();
                    report.EmployeeId = e.Id;
                    var user = Task.Run(() => _userManager.FindByIdAsync(e.UserId)).Result;
                    report.EmployeeFirstName = e.FirstName;
                    report.EmployeeLastName = e.LastName;

                    report.ExpectedWorkingHours = GetExpectedWorkingHours(e.Id, from, to);
                    report.WorkedHours = GetWorkedHours(e.Id, from, to);
                    report.ApprovedHours = GetApprovedHours(e.Id, from, to);
                    report.HoursOnLeave = GetLeaveHours(e.Id, from, to);

                    reports.Add(report);
                }
                operationResult.Object = reports;
            }
            else
            {
                operationResult.Succeeded = validationResult.Succeeded;
                operationResult.ErrorMessages = validationResult.ErrorMessages;

                return operationResult;
            }

            return operationResult;
        }

        // PagedList Method
        public OperationResult<PagedList<AccountantReport>> GetAccountantReports(int pageIndex, int pageSize, DateTime from, DateTime to)
        {
            var operationResult = new OperationResult<PagedList<AccountantReport>>();
            var validationResult = ValidateGetAccountantReportsRequest(from, to);

            if (validationResult.Succeeded)
            {
                var employeesPList = GetPagedList(pageIndex, pageSize);

                var reportsPList = new PagedList<AccountantReport>(employeesPList.PageIndex, employeesPList.TotalItems);

                foreach (var e in employeesPList.Items)
                {
                    var report = new AccountantReport();
                    report.EmployeeId = e.Id;
                    var user = Task.Run(() => _userManager.FindByIdAsync(e.UserId)).Result;
                    report.EmployeeFirstName = e.FirstName;
                    report.EmployeeLastName = e.LastName;

                    report.ExpectedWorkingHours = GetExpectedWorkingHours(e.Id, from, to);
                    report.WorkedHours = GetWorkedHours(e.Id, from, to);
                    report.ApprovedHours = GetApprovedHours(e.Id, from, to);
                    report.HoursOnLeave = GetLeaveHours(e.Id, from, to);

                    reportsPList.Items.Add(report);
                }
                operationResult.Object = reportsPList;

            }
            else
            {
                operationResult.Succeeded = validationResult.Succeeded;
                operationResult.ErrorMessages = validationResult.ErrorMessages;

                return operationResult;
            }

            return operationResult;
        }

        #region DATA VALIDTION
        /* Not allowed to request:
         * - To later than today
         * - From earlier than one year ago
         * - Period of Time wider then one month (31 days)
         */
        private OperationResult<DateTime> ValidateGetAccountantReportsRequest(DateTime from, DateTime to)
        {
            var today = DateTime.Now.Date;

            var result = new OperationResult<DateTime>(today);
            if (DateTime.Compare(from.Date, to.Date) >= 0)
            {
                result.Succeeded = false;
                result.AddOperationError("DateCompare", "Cannot request end date earlier than start date.");
            }
            else
            {
                if (DateTime.Compare(from.Date, today.Date.AddYears(-1)) < 0)
                {
                    result.Succeeded = false;
                    result.AddOperationError("FromDate", "Cannot request dates earlier than one year ago.");
                }
                if (DateTime.Compare(to.Date, today.Date) > 0)
                {
                    result.Succeeded = false;
                    result.AddOperationError("ToDate", "Cannot request a future date.");
                }
                if (DateTime.Compare(from.Date, to.Date.AddMonths(-1)) < 0)
                {
                    result.Succeeded = false;
                    result.AddOperationError("Timespan", "Cannot request a time span longer than one month.");
                }
            }

            return result;

        }

        #endregion END DATA VALIDATION

        private HoursOnLeave GetLeaveHours(int employeeId, DateTime from, DateTime to)
        {
            var leaveHours = new HoursOnLeave();
            var currentDate = from.Date;

            while (currentDate.Date <= to.Date)
            {
                var leaveTypeId = _leaveRequestRepositoryNew.Table
                    .GroupJoin(_requestedDateRepository.Table, l => l.Id, r => r.LeaveRequestId, (l, r) => new { l, r = r.DefaultIfEmpty() })
                    .Where(x => x.l.EmployeeId == employeeId)
                    .Where(x => x.r.Any(y => y.Date == currentDate.Date))
                    .Where(x => x.r.All(y => y.StatusCode == (int)RequestStatus.Accepted))
                    .Select(x => x.l.LeaveTypeId)
                    .FirstOrDefault();

                if (leaveTypeId != 0)
                {
                    var job = _daysWorkingService.GetAllocation(employeeId, currentDate);
                    if (job.IsAssigned)
                    {
                        var hours = job.EndTime.Value - job.StartTime.Value;
                        var leaveType = _leaveTypeRepositoryNew.Return(leaveTypeId);
                        if (leaveType.Payable)
                            leaveHours.Payable = leaveHours.Payable.Add(hours);
                        else
                            leaveHours.NonPayable = leaveHours.NonPayable.Add(hours);
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return leaveHours;
        }

        public AccountantReport GetAccountantReport(int employeeId, DateTime from, DateTime to)
        {
            var employee = GetEmployeeById(employeeId);
            var model = new AccountantReport();

            model.EmployeeId = employeeId;
            model.EmployeeFirstName = employee.FirstName;
            model.EmployeeLastName = employee.LastName;

            model.ExpectedWorkingHours = GetExpectedWorkingHours(employeeId, from, to);
            model.WorkedHours = GetWorkedHours(employeeId, from, to);
            model.ApprovedHours = GetApprovedHours(employeeId, from, to);

            return model;
        }

        private TimeSpan GetApprovedHours(int employeeId, DateTime from, DateTime to)
        {
            var timesheet = GetTimesheet(employeeId, from, to);
            var workedHours = new TimeSpan(0);

            foreach (var shift in timesheet)
            {
                var duration = shift.Item2 == null ? new TimeSpan(0) : shift.Item2.Timestamp - shift.Item1.Timestamp;
                if (shift.Item1.IsApproved && shift.Item2.IsApproved)
                    workedHours = workedHours.Add(duration);
            }

            return workedHours;
        }

        private TimeSpan GetWorkedHours(int employeeId, DateTime from, DateTime to)
        {
            var timesheet = GetTimesheet(employeeId, from, to);
            var workedHours = new TimeSpan(0);

            foreach (var shift in timesheet)
            {
                var duration = shift.Item2 == null ? new TimeSpan(0) : shift.Item2.Timestamp - shift.Item1.Timestamp;
                workedHours = workedHours.Add(duration);
            }

            return workedHours;
        }

        private TimeSpan GetExpectedWorkingHours(int employeeId, DateTime from, DateTime to)
        {
            var currentDate = from.Date;
            var expectedHours = new TimeSpan(0);

            while (currentDate.Date <= to.Date)
            {
                var job = _daysWorkingService.GetAllocation(employeeId, currentDate);
                if (job.IsAssigned)
                {
                    var hours = job.EndTime.Value - job.StartTime.Value;
                    expectedHours = expectedHours.Add(hours);
                }

                currentDate = currentDate.AddDays(1);
            }

            return expectedHours;
        }

        #endregion END ACCOUNTANT REPORTS

        #region PRIVATE METHODS
        private BusinessRoleEmployeesOnShiftAPIModel ToBusinessRoleEmployeesOnShiftAPIModel(BusinessRole key, IEnumerable<EmployeeOnShiftAPIModel> group,
            int departmentId)
        {
            var item = _businessRoleDepartmentRepository.Table
                .Where(x => x.BusinessRoleId == key.Id)
                .Where(x => x.DepartmentId == departmentId)
                .FirstOrDefault();

            var busRoleMinRequired = item.MinimumRequired;

            return new BusinessRoleEmployeesOnShiftAPIModel
            {
                BusinessRole = key,
                EmployeesOnShift = group.ToList(),
                BusinessRoleMinRequired = busRoleMinRequired
            };
        }

        private BusinessRoleEmployeesOnShiftAPIModel ToBusinessRoleEmployeesOnShiftAPIModel(BusinessRole key, IEnumerable<EmployeeOnShiftAPIModel> group,
            int departmentId, DateTime fromDate)
        {
            var item = _businessRoleDepartmentRepository.Table
                .Where(x => x.BusinessRoleId == key.Id)
                .Where(x => x.DepartmentId == departmentId)
                .FirstOrDefault();

            var busRoleMinRequired = item.MinimumRequired;
            
            var daysStaffLevel = new List<DailyStaffLevel>();
            for (var i = 0; i < 7; i++)
            {
                var day = fromDate.AddDays(i);
                var daySL = new DailyStaffLevel()
                {
                    Date = day,
                    DayOfWeek = day.DayOfWeek.ToString(),
                    DepartmentStaffLevel = _departmentService.GetStaffCount(departmentId, day),
                    BusinessRoleStaffLevel = _departmentService.GetStaffCount(departmentId, day, key.Id)
                };
                daysStaffLevel.Add(daySL);
            }

            return new BusinessRoleEmployeesOnShiftAPIModel
            {
                BusinessRole = key,
                EmployeesOnShift = group.ToList(),
                BusinessRoleMinRequired = busRoleMinRequired,
                StaffLevel = daysStaffLevel,
                DepartmentId = departmentId,
                DepartmentName = _departmentService.GetDepartmentById(departmentId).Name
            };
        }

        private EmployeeOnShiftAPIModel ToEmployeeOnShiftAPIModel(int key, IList<WorkingDay> group, DateTime fromDate)
        {
            var employee = GetEmployeeById(key);
            var businessRole = _businessRoleService.GetPrimaryBusinessRoleByEmployeeId(employee.Id);
            var leaveDays = _leaveService.GetAllByEmployeeId(employee.Id, (int)RequestStatus.Accepted, fromDate);
            var assignments = _assignmentRepository.Table
                .Where(x => x.EmployeeId == employee.Id)
                .Where(x => x.EndDate >= fromDate)
                .ToList();
            var secondaryBusinessRoles = _businessRoleService.GetSecondaryBusinessRolesOnEmployeeId(employee.Id);

            var daysWorkingAPIModel = new List<DaysWorkingAPIModel>();
            for (var i = 0; i < 7; i++)
            {
                var dayAPI = Mapper.Map<DaysWorkingAPIModel>(group[i]);
                dayAPI.Date = fromDate.AddDays(i);
                var leave = leaveDays.FirstOrDefault(d => d.Date.Date == dayAPI.Date.Value.Date);
                if (leave != null)
                {
                    dayAPI.IsOnHoliday = true;
                    dayAPI.LeaveTypeName = leave.LeaveTypeName;
                }
                if (dayAPI.DepartmentId != null)
                {
                    dayAPI.DepartmentStaffLevel = _departmentService.GetStaffCount(dayAPI.DepartmentId.Value, dayAPI.Date.Value);
                    dayAPI.BusinessRoleStaffLevel = _departmentService.GetStaffCount(dayAPI.DepartmentId.Value, dayAPI.Date.Value, businessRole.Id);
                }
                daysWorkingAPIModel.Add(dayAPI);
            }

            return new EmployeeOnShiftAPIModel
            {
                EmployeeId = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                BusinessRole = businessRole,
                DaysWorking = daysWorkingAPIModel,
                LeaveDays = leaveDays,
                SecondaryBusinessRoles = secondaryBusinessRoles
            };
        }

        private EmployeeOnShiftAPIModel ToEmployeeOnShiftAPIModel(int employeeId, int businessRoleId, List<Assignment> assignments, DateTime fromDate)
        {
            var employee = GetEmployeeById(employeeId);
            var leaveDays = _leaveService.GetAllByEmployeeId(employee.Id, (int)RequestStatus.Accepted, fromDate);
            var businessRole = _businessRoleService.GetBusinessRoleById(businessRoleId);

            var secondaryBusinessRoles = _businessRoleService.GetSecondaryBusinessRolesOnEmployeeId(employee.Id);

            var daysWorkingAPIModel = new List<DaysWorkingAPIModel>();
            for (var i = 0; i < 7; i++)
            {
                var dayAPI = new DaysWorkingAPIModel();
                dayAPI.DepartmentId = assignments.First().DepartmentId;
                dayAPI.Date = fromDate.AddDays(i);
                dayAPI.Day = dayAPI.Date.Value.DayOfWeek.ToString();
                dayAPI.IsAssigned = assignments.Any(a => dayAPI.Day == a.Day);
                if (dayAPI.IsAssigned)
                {
                    var assignment = assignments.Find(a => dayAPI.Day == a.Day);
                    dayAPI.StartTime = assignments.First(a => dayAPI.Day == a.Day).StartTime;
                    dayAPI.EndTime = assignments.First(a => dayAPI.Day == a.Day).EndTime;
                    dayAPI.isAssignment = true;
                    dayAPI.Id = assignment.Id;
                }
                var leave = leaveDays.FirstOrDefault(d => d.Date.Date == dayAPI.Date.Value.Date);
                if (leave != null)
                {
                    dayAPI.IsOnHoliday = true;
                    dayAPI.LeaveTypeName = leave.LeaveTypeName;
                }
                dayAPI.DepartmentStaffLevel = _departmentService.GetStaffCount(dayAPI.DepartmentId.Value, dayAPI.Date.Value);
                dayAPI.BusinessRoleStaffLevel = _departmentService.GetStaffCount(dayAPI.DepartmentId.Value, dayAPI.Date.Value, businessRole.Id);
                daysWorkingAPIModel.Add(dayAPI);
            }

            return new EmployeeOnShiftAPIModel
            {
                EmployeeId = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                BusinessRole = businessRole,
                DaysWorking = daysWorkingAPIModel,
                LeaveDays = leaveDays,
                SecondaryBusinessRoles = secondaryBusinessRoles
            };


        }

        // Eventually Add validation
        private OperationResult<Employee> EditEmployeeInfo(Employee employee)
        {
            var result = new OperationResult<Employee>(employee);
            var eToEdit = GetEmployeeById(employee.Id);

            eToEdit.NIN = employee.NIN;
            eToEdit.EndDate = employee.EndDate;
            eToEdit.StartDate = employee.StartDate;
            eToEdit.DOB = employee.DOB;
            eToEdit.Barcode = employee.Barcode;
            eToEdit.HolidayAllowance = employee.HolidayAllowance;
            eToEdit.HoursRequired = employee.HoursRequired;

            _employeeRepository.Update(eToEdit);

            return result;
        }
        #endregion END PRIVATE METHODS

        //private ApplicationUser GetApplicationUser(string applicationUserId)
        //{
        //    return _userManager.Users.FirstOrDefault(u => string.Equals(u.Id, applicationUserId));
        //}

        public bool ApproveTimesheet(IList<TimesheetDetails> timeclockToEdit)
        {
            var result = new OperationResult<IList<TimesheetDetails>>(timeclockToEdit);
            try
            {
                foreach (var tc in timeclockToEdit)
                {
                    var tcToEdit = _timeclockTimestampRepository.Return(tc.TimeclockId);
                    tcToEdit.IsApproved = tc.IsApproved;

                    _timeclockTimestampRepository.Update(tcToEdit);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public void HandleEvent(EntityDeletedEvent<BusinessRole> eventMessage)
        {
            var role = eventMessage.Entity;
            var employeeRoles = _employeeBusinessRoleRepository.Table.Where(x => x.BusinessRoleId == role.Id).ToList();

            if (employeeRoles.Count() > 0)
            {
                foreach (var employeeRole in employeeRoles)
                {
                    _employeeBusinessRoleRepository.Delete(employeeRole);
                }
            }
        }
    }
}