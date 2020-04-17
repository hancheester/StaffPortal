using Microsoft.AspNetCore.Identity;
using StaffPortal.Common;
using StaffPortal.Common.APIModels;
using StaffPortal.Common.Models;
using StaffPortal.Common.Settings;
using StaffPortal.Data;
using StaffPortal.Service.Departments;
using StaffPortal.Service.Errors;
using StaffPortal.Service.Events;
using StaffPortal.Service.Message;
using StaffPortal.Service.Staff;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Service.Leave
{
    public class LeaveService : ILeaveService, IConsumer<EntityDeletedEvent<Employee>>
    {
        private readonly IDepartmentService _departmentService;
        private readonly IWorkingDaysService _daysWorkingService;
        private readonly IAttendanceService _attendanceService;
        private readonly IEventPublisher _eventPubliser;
        private readonly IErrorService _errorService;
        private readonly IEmailSender _emailSender;                
        private readonly IRepository<LeaveRequest> _leaveRequestRepository;
        private readonly IRepository<LeaveType> _leaveTypeRepository;
        private readonly IRepository<RequestedDate> _requestedDateRepository;
        private readonly IRepository<RejectionReason> _rejectionReasonRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<WorkingDay> _daysWorkingRepository;
        private readonly IRepository<BusinessRole> _businessRoleRepository;
        private readonly IRepository<Employee_BusinessRole> _employeeBusinessRoleRepository;
        private readonly IRepository<BusinessRole_Department> _businessRoleDepartmentRepository;
        private readonly IRepository<BusinessRole_Permission> _businessRolePermissionRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CompanySettings _companySettings;
        private readonly LeaveSettings _leaveSettings;

        public LeaveService(
            IDepartmentService departmentService,
            IEmailSender emailSender,
            IWorkingDaysService daysWorkingService,
            IAttendanceService attendanceService,
            IEventPublisher eventPubliser,
            IErrorService errorService,            
            IRepository<LeaveRequest> leaveRequestRepository,
            IRepository<LeaveType> leaveTypeRepositoryNew,
            IRepository<RequestedDate> requestedDateRepository,
            IRepository<RejectionReason> rejectionReasonRepository,
            IRepository<Employee> employeeRepository,
            IRepository<WorkingDay> daysWorkingRepository,
            IRepository<BusinessRole> businessRoleRepositoryNew,
            IRepository<Employee_BusinessRole> employeeBusinessRoleRepository,
            IRepository<BusinessRole_Department> businessRoleDepartmentRepository,
            IRepository<BusinessRole_Permission> businessRolePermissionRepository,
            UserManager<IdentityUser> userManager,
            LeaveSettings leaveSettings,
            CompanySettings companySettings)
        {
            _userManager = userManager;
            _leaveRequestRepository = leaveRequestRepository;
            _leaveTypeRepository = leaveTypeRepositoryNew;
            _requestedDateRepository = requestedDateRepository;
            _rejectionReasonRepository = rejectionReasonRepository;
            _employeeRepository = employeeRepository;
            _daysWorkingRepository = daysWorkingRepository;
            _businessRoleRepository = businessRoleRepositoryNew;
            _employeeBusinessRoleRepository = employeeBusinessRoleRepository;
            _businessRoleDepartmentRepository = businessRoleDepartmentRepository;
            _businessRolePermissionRepository = businessRolePermissionRepository;            
            _departmentService = departmentService;
            _daysWorkingService = daysWorkingService;
            _attendanceService = attendanceService;
            _eventPubliser = eventPubliser;
            _errorService = errorService;
            _emailSender = emailSender;
            _leaveSettings = leaveSettings;
            _companySettings = companySettings;
        }

        public IList<CalendarDay> GetDepartmentalCalendar(int departmentId, DateTime fromDate, DateTime toDate)
        {
            var calendar = new List<CalendarDay>();
            var date = fromDate;
            var department = _departmentService.GetDepartmentById(departmentId);

            while (date.Date <= toDate.Date)
            {
                var openingDay = department.OpeningHours.FirstOrDefault(x => x.Day == date.DayOfWeek.ToString());

                CalendarDay calendarDay = null;

                if (openingDay.IsOpen)
                {
                    calendarDay = new CalendarDay
                    {
                        Date = date,
                        DayOfTheWeekName = openingDay.Day,
                        DepartmentName = department == null ? string.Empty : department.Name,
                        StatusCode = (int)_attendanceService.GetDepartmentalAttendanceStatus(departmentId, date),
                        DepartmentId = department == null ? 0 : department.Id
                    };

                    var onHolidayEmployeeIds = _departmentService.GetEmployeeIdsOnHolidays(department.Id, date);

                    if (onHolidayEmployeeIds.Count() > 0)
                    {
                        var onHolidayEmployees = _employeeRepository.Table
                        .Where(x => onHolidayEmployeeIds.Contains(x.Id))
                        .Select(x => new OnHolidayEmployee
                        {
                            Name =( x.FirstName + " " + x.LastName).Trim()
                        })
                        .ToList();

                        calendarDay.EmployeesOnHolidays = onHolidayEmployees;
                    }                    
                }
                else
                {
                    calendarDay = new CalendarDay
                    {
                        Date = date,
                        DayOfTheWeekName = openingDay.Day,
                        IsDisabled = true
                    };
                }

                calendar.Add(calendarDay);
                date = date.AddDays(1);
            }
            return calendar;
        }

        public IList<CalendarDay> GetPersonalCalendar(int employeeId, DateTime fromDate, DateTime toDate)
        {
            var calendar = new List<CalendarDay>();
            var date = fromDate;
            var workingDays = _daysWorkingRepository.Table.Where(x => x.EmployeeId == employeeId).ToList();

            while (date.Date <= toDate.Date)
            {
                var workingDay = workingDays.FirstOrDefault(d => string.Equals(d.Day, date.DayOfWeek.ToString()));

                CalendarDay calendarDay = null;
                
                if (workingDay.IsAssigned)
                {
                    var department = _departmentService.GetDepartmentById(workingDay.DepartmentId.Value);
                    var allocation = _daysWorkingService.GetAllocation(employeeId, date);

                    calendarDay = new CalendarDay
                    {
                        Date = date,
                        DayOfTheWeekName = workingDay.Day,
                        IsDisabled = false,
                        DepartmentName = department == null ? string.Empty : department.Name,
                        StatusCode = (int)_attendanceService.GetDepartmentalAttendanceStatus(department.Id, date, allocation.RoleId),
                        DepartmentId = department == null ? 0 : department.Id
                    };

                    if (department != null)
                    {
                        var opening = department.OpeningHours.FirstOrDefault(x => x.Day == workingDay.Day);
                        if (!opening.IsOpen)
                        {
                            calendarDay.IsDisabled = true;
                            calendarDay.DepartmentId = -1;
                        }

                        var onHolidayEmployeeIds = _departmentService.GetEmployeeIdsOnHolidays(department.Id, date);

                        if (onHolidayEmployeeIds.Count() > 0)
                        {
                            var onHolidayEmployees = _employeeRepository.Table
                                .Where(x => onHolidayEmployeeIds.Contains(x.Id))
                                .Select(x => new OnHolidayEmployee
                                {
                                    Name = (x.FirstName + " " + x.LastName).Trim()
                                })
                                .ToList();

                            calendarDay.EmployeesOnHolidays = onHolidayEmployees;
                        }
                    }
                }
                else
                {
                    calendarDay = new CalendarDay
                    {
                        Date = date,
                        DayOfTheWeekName = workingDay.Day,
                        IsDisabled = true
                    };
                }

                calendar.Add(calendarDay);
                date = date.AddDays(1);
            }
            return calendar;
        }

        public async Task<IList<PendingLeaveRequest>> GetPendingLeaveRequestsAsync(string applicationUserId, int count = 5)
        {
            var employeeId = _employeeRepository.Table.Where(x => x.UserId == applicationUserId).Select(x => x.Id).FirstOrDefault();
            var applicationUser = await _userManager.FindByIdAsync(applicationUserId);
            var isAdmin = await _userManager.IsInRoleAsync(applicationUser, "SuperAdmin");
            var departments = isAdmin 
                ? await _departmentService.GetAllDepartmentsAsync() 
                : await _departmentService.GetAssignedDepartmentsByEmployeeIdAsync(employeeId);
            var departmentIds = departments.Select(x => x.Id).ToList();

            var primaryRole = _employeeBusinessRoleRepository.Table
                .Where(x => x.EmployeeId == employeeId)
                .Where(x => x.IsPrimary == true)
                .FirstOrDefault();

            Func<int, IList<BusinessRole>> AppendChildrenRole = null;
            AppendChildrenRole = (parentRoleId) =>
            {
                var roles = _businessRoleRepository.Table
                .Where(x => x.ParentBusinessRoleId == parentRoleId)
                .ToList();

                if (roles.Count > 0)
                {
                    var fetchedRoles = new List<BusinessRole>();
                    foreach (var role in roles)
                    {
                        var list = AppendChildrenRole(role.Id);
                        if (list != null) fetchedRoles.AddRange(list);
                    }
                    roles.AddRange(fetchedRoles);
                }
                
                return roles;                
            };

            var childrenRoles = isAdmin 
                ? new List<BusinessRole>()
                : AppendChildrenRole(primaryRole.BusinessRoleId);
            var childrenRoleIds = childrenRoles.Select(x => x.Id).ToList();

            var query = _leaveRequestRepository.Table
                .GroupJoin(_requestedDateRepository.Table, l => l.Id, r => r.LeaveRequestId, (l, r) => new { l, r = r.Where(x => departmentIds.Contains(x.DepartmentId)).DefaultIfEmpty() })
                .Join(_employeeBusinessRoleRepository.Table, x => x.l.EmployeeId, eb => eb.EmployeeId, (x, eb) => new { l_r = x, eb })
                .Join(_employeeRepository.Table, x => x.l_r.l.EmployeeId, e => e.Id, (x, e) => new { x.l_r, x.eb, e })
                .Join(_businessRoleRepository.Table, x => x.eb.BusinessRoleId, br => br.Id, (x, br) => new { x.l_r, x.eb, x.e, br })
                .Where(x => x.eb.IsPrimary == true)
                .Where(x => x.l_r.r.Where(y => y.StatusCode == (int)RequestStatus.Pending).Any());
                
            if (childrenRoles.Count > 0)
                query = query.Where(x => childrenRoleIds.Contains(x.eb.BusinessRoleId));

            query = query.Take(count);

            var pendingRequests = query.Select(x => new PendingLeaveRequest
            {
                Id = x.l_r.l.Id,
                EmployeeId = x.l_r.l.EmployeeId,
                EmployeeName = (x.e.FirstName + " " + x.e.LastName).Trim(),
                BusinessRoleId = x.eb.BusinessRoleId,
                BusinessRole = x.br.Name,
                LeaveTypeId = x.l_r.l.LeaveTypeId,
                LeaveTypeName = _leaveTypeRepository.Return((object)x.l_r.l.LeaveTypeId).Name,
                DateCreated = x.l_r.l.DateCreated,
                IsEmergency = x.l_r.l.IsEmergency,
                Note = x.l_r.l.Note,
                RequestedDates = x.l_r.r.ToList(),
                PendingRequestedDates = x.l_r.r.Select(y => new PendingRequestedDate
                {
                    Id = y.Id,
                    Date = y.Date.ToString("dddd, dd MMMM yyyy"),
                    Department = _departmentService.GetDepartmentById(y.DepartmentId).Name,
                    Role = _businessRoleRepository.Return(x.eb.BusinessRoleId).Name,
                    StaffLevel = _attendanceService.GetDepartmentalAttendanceStatus(y.DepartmentId, y.Date, (int?)x.eb.BusinessRoleId).ToString()
                }).ToList()
            }).ToList();

            return pendingRequests;            
        }

        public LeaveQuota GetLeaveQuota(int employeeId)
        {
            var noImpact = GetNoImpactOnAllowance(employeeId);
            var accruedAsPay = GetAccruedAsPay(employeeId);
            var accruedAsDay = GetAccruedAsDay(employeeId);
            var allowance = _employeeRepository.Table
                .Where(x => x.Id == employeeId)
                .Select(x => x.HolidayAllowance)
                .FirstOrDefault();
            var total = allowance + accruedAsDay;
            var used = GetUsed(employeeId);
            var pending = GetPending(employeeId);
            var remaining = total - used - pending;

            return new LeaveQuota
            {
                Total = total,
                AccruedAsDay = accruedAsDay,
                AccruedAsPay = accruedAsPay,
                Remaining = remaining,
                Used = used,
                Pending = pending,
                NoImpact = noImpact
            };
        }

        public decimal GetAccruedAsDay(int employeeId)
        {
            var financialYears = CalculateFinancialYears();

            var query = _requestedDateRepository.Table
                .GroupJoin(_leaveRequestRepository.Table, r => r.LeaveRequestId, l => l.Id, (r, l) => new { r, l = l.DefaultIfEmpty() })
                .SelectMany(x => x.l.DefaultIfEmpty(), (x, l) => new { x.r, l })
                .GroupJoin(_leaveTypeRepository.Table, x => x.l.LeaveTypeId, t => t.Id, (x, t) => new { x.r, x.l, t = t.DefaultIfEmpty() })
                .SelectMany(x => x.t.DefaultIfEmpty(), (x, t) => new { x.r, x.l, t })
                .Where(x => x.l.EmployeeId == employeeId)
                .Where(x => x.t.Accruable == true)
                .Where(x => x.r.StatusCode == (int)RequestStatus.Accepted)
                .Where(x => x.r.AccruableAs == (int)AccruableAs.Day)
                .Where(x => x.r.Date.CompareTo(financialYears[0]) >= 0)
                .Where(x => x.r.Date.CompareTo(financialYears[1]) <= 0)
                .Select(x => new { x.r.Id, x.r.IsFullDay });

            var fullDayCount = query.Where(x => x.IsFullDay == true).Count();
            var halfDayCount = query.Where(x => x.IsFullDay == false).Count() / 2M;

            return fullDayCount + halfDayCount;
        }

        public decimal GetAccruedAsPay(int employeeId)
        {
            var query = _requestedDateRepository.Table
                .GroupJoin(_leaveRequestRepository.Table, r => r.LeaveRequestId, l => l.Id, (r, l) => new { r, l = l.DefaultIfEmpty() })
                .SelectMany(r_l => r_l.l.DefaultIfEmpty(), (x, l) => new { x.r, l })
                .GroupJoin(_leaveTypeRepository.Table, x => x.l.LeaveTypeId, t => t.Id, (x, t) => new { x.r, x.l, t = t.DefaultIfEmpty() })
                .SelectMany(x => x.t.DefaultIfEmpty(), (x, t) => new { x.r, x.l, t })
                .Where(x => x.l.EmployeeId == employeeId)
                .Where(x => x.t.Accruable == true)
                .Where(x => x.r.StatusCode == (int)RequestStatus.Accepted)
                .Where(x => x.r.AccruableAs == (int)AccruableAs.Pay)
                .Select(x => new { x.r.Id, x.r.IsFullDay });

            var fullDayCount = query.Where(x => x.IsFullDay == true).Count();
            var halfDayCount = query.Where(x => x.IsFullDay == false).Count() / 2M;

            return fullDayCount + halfDayCount;            
        }

        public IList<LeaveAPIModel> GetAllByEmployeeId(int employeeId, int statusCode, DateTime fromDate)
        {
            var requests = _requestedDateRepository.Table
                .GroupJoin(_leaveRequestRepository.Table, r => r.LeaveRequestId, l => l.Id, (r, l) => new { r, l = l.DefaultIfEmpty() })
                .SelectMany(r_l => r_l.l.DefaultIfEmpty(), (x, l) => new { x.r, l })
                .GroupJoin(_leaveTypeRepository.Table, x => x.l.LeaveTypeId, t => t.Id, (x, t) => new { x.r, x.l, t = t.DefaultIfEmpty() })
                .SelectMany(x => x.t.DefaultIfEmpty(), (x, t) => new { x.r, x.l, t })
                .Where(x => x.l.EmployeeId == employeeId)
                .Where(x => x.r.StatusCode == statusCode)
                .Where(x => x.r.Date >= fromDate)
                .Select(x => ToLeaveAPIModel(x.l, x.r))
                .ToList();

            return requests;
        }

        public decimal GetUsed(int employeeId)
        {
            var financialYears = CalculateFinancialYears();

            var query = _requestedDateRepository.Table
                .GroupJoin(_leaveRequestRepository.Table, r => r.LeaveRequestId, l => l.Id, (r, l) => new { r, l = l.DefaultIfEmpty() })
                .SelectMany(x => x.l.DefaultIfEmpty(), (x, l) => new { x.r, l })
                .GroupJoin(_leaveTypeRepository.Table, x => x.l.LeaveTypeId, t => t.Id, (x, t) => new { x.r, x.l, t = t.DefaultIfEmpty() })
                .SelectMany(x => x.t.DefaultIfEmpty(), (x, t) => new { x.r, x.l, t })
                .Where(x => x.l.EmployeeId == employeeId)
                .Where(x => x.t.ImpactOnAllowance == true)
                .Where(x => x.r.StatusCode == (int)RequestStatus.Accepted)
                .Where(x => x.r.Date.CompareTo(financialYears[0]) >= 0)
                .Where(x => x.r.Date.CompareTo(financialYears[1]) <= 0)
                .Select(x => new { x.r.Id, x.r.IsFullDay });

            var fullDayCount = query.Where(x => x.IsFullDay == true).Count();
            var halfDayCount = query.Where(x => x.IsFullDay == false).Count() / 2M;

            return fullDayCount + halfDayCount;
        }

        public decimal GetNoImpactOnAllowance(int employeeId)
        {
            var financialYears = CalculateFinancialYears();
            
            var query = _requestedDateRepository.Table
                .GroupJoin(_leaveRequestRepository.Table, r => r.LeaveRequestId, l => l.Id, (r, l) => new { r, l = l.DefaultIfEmpty() })
                .SelectMany(x => x.l.DefaultIfEmpty(), (x, l) => new { x.r, l })
                .GroupJoin(_leaveTypeRepository.Table, x => x.l.LeaveTypeId, t => t.Id, (x, t) => new { x.r, x.l, t = t.DefaultIfEmpty() })
                .SelectMany(x => x.t.DefaultIfEmpty(), (x, t) => new { x.r, x.l, t})
                .Where(x => x.l.EmployeeId == employeeId)
                .Where(x => x.t.ImpactOnAllowance == false)                
                .Where(x => x.r.StatusCode == (int)RequestStatus.Accepted)
                .Where(x => x.r.Date.CompareTo(financialYears[0]) >= 0)
                .Where(x => x.r.Date.CompareTo(financialYears[1]) <= 0)
                .Select(x => new { x.r.Id, x.r.IsFullDay });

            var fullDayCount = query.Where(x => x.IsFullDay == true).Count();
            var halfDayCount = query.Where(x => x.IsFullDay == false).Count() / 2M;

            return fullDayCount + halfDayCount;            
        }

        public decimal GetPending(int employeeId)
        {
            var financialYears = CalculateFinancialYears();

            var query = _requestedDateRepository.Table
                .GroupJoin(_leaveRequestRepository.Table, r => r.LeaveRequestId, l => l.Id, (r, l) => new { r, l = l.DefaultIfEmpty() })
                .SelectMany(x => x.l.DefaultIfEmpty(), (x, l) => new { x.r, l })
                .GroupJoin(_leaveTypeRepository.Table, x => x.l.LeaveTypeId, t => t.Id, (x, t) => new { x.r, x.l, t = t.DefaultIfEmpty() })
                .SelectMany(x => x.t.DefaultIfEmpty(), (x, t) => new { x.r, x.l, t })
                .Where(x => x.l.EmployeeId == employeeId)
                .Where(x => x.t.ImpactOnAllowance == true)
                .Where(x => x.r.StatusCode == (int)RequestStatus.Pending)
                .Where(x => x.r.Date.CompareTo(financialYears[0]) >= 0)
                .Where(x => x.r.Date.CompareTo(financialYears[1]) <= 0)
                .Select(x => new { x.r.Id, x.r.IsFullDay });

            var fullDayCount = query.Where(x => x.IsFullDay == true).Count();
            var halfDayCount = query.Where(x => x.IsFullDay == false).Count() / 2M;

            return fullDayCount + halfDayCount;
        }

        public OperationResult<LeaveRequest> AddLeaveRequest(LeaveRequest leaveRequest)
        {
            var result = new OperationResult<LeaveRequest>(leaveRequest);

            try
            {
                _leaveRequestRepository.Create(leaveRequest);

                foreach (var date in leaveRequest.RequestedDates)
                {
                    date.LeaveRequestId = leaveRequest.Id;
                    date.StatusCode = (int)RequestStatus.Pending;
                    date.DateProcessed = DateTime.Now;
                    _requestedDateRepository.Create(date);
                }

                //_eventPubliser.EntityInserted(leaveRequest);

                var departmentIds = leaveRequest.RequestedDates.Select(x => x.DepartmentId).Distinct().ToList();

                var approvers = _businessRoleDepartmentRepository.Table
                    .Join(_businessRolePermissionRepository.Table, bd => bd.BusinessRoleId, bp => bp.BusinessRoleId, (bd, bp) => new { bd, bp })
                    .Join(_employeeBusinessRoleRepository.Table, x => x.bd.BusinessRoleId, eb => eb.BusinessRoleId, (x, eb) => new { x.bd, x.bp, eb })
                    .Join(_employeeRepository.Table, x => x.eb.EmployeeId, e => e.Id, (x, e) => new { x.bd, x.bp, x.eb, e })
                    .Join(_businessRoleRepository.Table, x => x.bd.BusinessRoleId, b => b.Id, (x, b) => new { x.bd, x.bp, x.eb, x.e, b })
                    .Join(_userManager.Users, x => x.e.UserId, u => u.Id, (x, u) => new { x.bd, x.bp, x.eb, x.e, x.b, u })
                    .Where(x => departmentIds.Contains(x.bd.DepartmentId))
                    .Where(x => x.bp.PermissionId == (int)PermissionsCode.LeaveRequestApprover)
                    .Select(x => new {x.u, x.b })
                    .Distinct()
                    .ToList();

                foreach (var approver in approvers)
                {
                    _emailSender.SendEmailAsync(approver.u.Email, "Leave Approval", $"Pending leave request. Your role is { approver.b.Name }.");
                }

                var employee = _employeeRepository.Return(leaveRequest.EmployeeId);
                var applicationUser = Task.Run(() => _userManager.FindByIdAsync(employee.UserId)).Result;

                _emailSender.SendEmailAsync(applicationUser.Email, "Leave Request", "Your pending leave request.");
            }
            catch(Exception ex)
            {
                result.AddOperationError("E1", "Failed to create leave request.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public IList<int> GetHolidayDatesByMonth(int employeeId, int month, int year)
        {
            return _leaveRequestRepository.Table
                .GroupJoin(_requestedDateRepository.Table, l => l.Id, r => r.LeaveRequestId, (l, r) => new { l, r = r.DefaultIfEmpty() })
                .Where(x => x.l.EmployeeId == employeeId)
                .Where(x => x.r.All(y => y.StatusCode == (int)RequestStatus.Accepted))
                .SelectMany(x => x.r)
                .Where(x => x.Date.Month == month)
                .Where(x => x.Date.Year == year)
                .Select(x => x.Date.Day)
                .ToList();
        }

        public IList<LeaveRequest> GetLeaveHistory(int employeeId, out int total, int pageNumber = 1, int pageSize = 10)
        {
            total = 0;

            try
            {
                var query = _leaveRequestRepository.Table
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.DateCreated);

                total = query.Count();

                var items = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .GroupJoin(_requestedDateRepository.Table, lr => lr.Id, rd => rd.LeaveRequestId, (lr, rd) => new { lr, rd })
                    .ToList();

                var history = items.Select(x =>
                {
                    foreach (var request in x.rd)
                    {
                        if (request.RejectionReasonId.HasValue)
                        {
                            request.Reason = _rejectionReasonRepository.Return(request.RejectionReasonId.Value);
                        }
                    }

                    x.lr.RequestedDates = x.rd.ToList();
                    return x.lr;
                })
                .ToList();

                return history;
            }
            catch (Exception ex)
            {
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return new List<LeaveRequest>();
        }

        public decimal GetTotalLeaveQuota(int employeeId)
        {
            var employee = _employeeRepository.Return(employeeId);

            return employee.HolidayAllowance + GetAccruedAsDay(employeeId);
        }

        public decimal GetRemainingLeaveQuota(int employeeId)
        {
            decimal used = GetUsed(employeeId);
            decimal total = GetTotalLeaveQuota(employeeId);
            decimal pending = GetPending(employeeId);

            //TODO: We need to include accruable days in the formula too.

            return total + decimal.Negate(used) + decimal.Negate(pending);
        }

        public OperationResult DisapproveLeaveRequest(string applicationUserId, int leaveRequestId, string comment)
        {
            var result = new OperationResult();

            var applicationUser = Task.Run(() => _userManager.FindByIdAsync(applicationUserId)).Result;
            var isAdmin = Task.Run(() => _userManager.IsInRoleAsync(applicationUser, "SuperAdmin")).Result;

            var query = _requestedDateRepository.Table
                .Where(x => x.LeaveRequestId == leaveRequestId);

            if (isAdmin == false)
            {
                var employee = _employeeRepository.Table.Where(x => x.UserId == applicationUserId).FirstOrDefault();
                var businessRoleIds = _employeeBusinessRoleRepository.Table
                .Where(x => x.EmployeeId == employee.Id)
                .Select(x => x.BusinessRoleId)
                .ToList();

                var departmentIds = _businessRoleDepartmentRepository.Table
                    .Where(x => businessRoleIds.Contains(x.BusinessRoleId))
                    .Select(x => x.DepartmentId)
                    .ToList();

                query = query.Where(x => departmentIds.Contains(x.DepartmentId));
            }

            var leaveDates = query.ToList();
            var reason = new RejectionReason
            {
                Message = comment
            };
            reason.Id = _rejectionReasonRepository.Create(reason);

            leaveDates.ForEach(x =>
            {
                x.StatusCode = (int)RequestStatus.Rejected;
                x.ApproverId = applicationUserId;
                x.DateProcessed = DateTime.Now;
                x.RejectionReasonId = reason.Id;

                _requestedDateRepository.Update(x);
            });

            return result;
        }

        public OperationResult ApproveLeaveRequest(string applicationUserId, int leaveRequestId)
        {
            var result = new OperationResult();
            var leaveRequest = _leaveRequestRepository.Return(leaveRequestId);

            var quota = GetRemainingLeaveQuota(leaveRequest.EmployeeId);
            if (quota <= 0)
            {
                result.AddOperationError("E3", "Not enough leave quota.");
                return result;
            }

            var query = _requestedDateRepository.Table
                .Where(x => x.LeaveRequestId == leaveRequestId);

            var applicationUser = Task.Run(() => _userManager.FindByIdAsync(applicationUserId)).Result;
            var isAdmin = Task.Run(() => _userManager.IsInRoleAsync(applicationUser, "SuperAdmin")).Result;

            if (isAdmin == false)
            {
                var employee = _employeeRepository.Table.Where(x => x.UserId == applicationUserId).FirstOrDefault();
                var businessRoleIds = _employeeBusinessRoleRepository.Table
                .Where(x => x.EmployeeId == employee.Id)
                .Select(x => x.BusinessRoleId)
                .ToList();

                var departmentIds = _businessRoleDepartmentRepository.Table
                    .Where(x => businessRoleIds.Contains(x.BusinessRoleId))
                    .Select(x => x.DepartmentId)
                    .ToList();

                query = query.Where(x => departmentIds.Contains(x.DepartmentId));
            }
            
            var leaveDates = query.ToList();

            if (leaveDates.Count == 0)
            {
                result.AddOperationError("E2", "Requested dates not found.");
                return result;
            }

            var leaveType = _leaveTypeRepository.Return(leaveRequest.LeaveTypeId);
            if (leaveType.Requestable == false)
            {
                result.AddOperationError("E2", "Leave type is not requestable.");
                return result;
            }

            if (leaveType.ImpactOnAllowance == true)
            {
                decimal fullDays = leaveDates.Where(d => d.IsFullDay).Count();
                decimal halfDays = leaveDates.Where(d => !d.IsFullDay).Count() / 2M;
                var leavePeriod = fullDays + halfDays;
                
                if (leavePeriod > quota)
                {
                    result.AddOperationError("E4", "Required leave period exceeds remaining leave quota.");
                    return result;
                }
            }

            if (leaveType.Accruable == true)
            {
                var accruableAsPaidMonths = _leaveSettings.MonthsToAccrue
                    .Select(x => DateTime.ParseExact(x, "MMM", CultureInfo.InvariantCulture).Month);

                leaveDates.ForEach(x =>
                {
                    if (accruableAsPaidMonths.Any(m => m == x.Date.Month))
                        x.AccruableAs = (int)AccruableAs.Pay;
                    else
                        x.AccruableAs = (int)AccruableAs.Day;
                });
            }

            foreach (var leaveDate in leaveDates)
            {
                leaveDate.ApproverId = applicationUserId;
                leaveDate.DateProcessed = DateTime.Now;
                leaveDate.StatusCode = (int)RequestStatus.Accepted;

                _requestedDateRepository.Update(leaveDate);
            }

            if (result.Succeeded)
            {
                var employee = _employeeRepository.Return(leaveRequest.EmployeeId);
                var user = _userManager.Users.First(u => employee.UserId == u.Id);
                _emailSender.SendEmailAsync(user.Email, "Staff Portal - Leave Request has been accepted!", "Leave Request has been accepted!");
            }
            else
            {
                _errorService.Insert(new ErrorLog(result.ErrorSummary, string.Empty));
            }

            return result;
        }

        public OperationResult<LeaveRequest> ValidateLeaveRequest(LeaveRequest request)
        {
            var type = _leaveTypeRepository.Return(request.LeaveTypeId);
            OperationResult<LeaveRequest> result = new OperationResult<LeaveRequest>(request);
            
            if (type.Requestable == false)
                result.AddOperationError("IsLeaveRequestable", "Leave type is not requestable.");

            /*
            if leave request is not Accruable
                >>> and has impact on allowance (then the request has a negative impact on the leave quota)
                >>> does the employee has enough leave quota left?
            */
            if (type.Accruable == false && type.ImpactOnAllowance == true)
            {
                // Ge leave period time
                var leavePeriod = CalculateLeavePeriod(request.RequestedDates);

                // Get remaining leave quota
                var quota = GetRemainingLeaveQuota(request.EmployeeId);
                if (quota < 0)
                {
                    result.AddOperationError("LeaveQuota", "The leave period required exceeds the reamining quota.");
                    result.Succeeded = false;
                }
            }

            return result;
        }

        private decimal CalculateLeavePeriod(ICollection<RequestedDate> requestedDates)
        {
            decimal fullDays = requestedDates
                        .Where(d => d.IsFullDay)
                        .Count();

            decimal halfDays = requestedDates
                        .Where(d => !d.IsFullDay)
                        .Count() / 2M;

            return fullDays + halfDays;
        }

        private LeaveAPIModel ToLeaveAPIModel(LeaveRequest request, RequestedDate date)
        {
            var employee = _employeeRepository.Return(request.EmployeeId);
            var name = $"{employee.FirstName} {employee.LastName}";
            var primaryBusinessRole = _employeeBusinessRoleRepository.Table
                    .Join(_businessRoleRepository.Table, e => e.BusinessRoleId, b => b.Id, (e, b) => new { e, b })
                    .Where(x => x.e.EmployeeId == employee.Id)
                    .Where(x => x.e.IsPrimary == true)
                    .Select(x => x.b)
                    .FirstOrDefault();
            var leaveType = _leaveTypeRepository.Return(request.LeaveTypeId);
            var department = _departmentService.GetDepartmentById(date.DepartmentId);

            var leaveDay = new LeaveAPIModel
            {
                EmployeeId = employee.Id,
                EmployeeName = name,
                BusinessRoleId = primaryBusinessRole.Id,
                BusinessRole = primaryBusinessRole.Name,
                LeaveTypeName = leaveType.Name,
                Date = date.Date,
                IsFullDay = date.IsFullDay,
                DepartmentName = department.Name
            };

            return leaveDay;
        }

        private DateTime[] CalculateFinancialYears()
        {
            var currentMonth = DateTime.Now.Month;            
            var startMonth = Convert.ToInt32(_companySettings.FinancialYearStart.Split("/")[1]);            
            var startingYear = DateTime.Now.Year;
            var endingYear = DateTime.Now.Year;
            
            if (currentMonth > startMonth)
            {
                endingYear = DateTime.Now.AddYears(1).Year;
            }
            else if (currentMonth == startMonth)
            {
                var currentDay = DateTime.Now.Day;
                var startDay = Convert.ToInt32(_companySettings.FinancialYearStart.Split("/")[0]);

                if (currentDay >= startDay)
                {
                    endingYear = DateTime.Now.AddYears(1).Year;
                }
                else
                {
                    startingYear = DateTime.Now.AddYears(-1).Year;
                }
            }
            else
            {
                startingYear = DateTime.Now.AddYears(-1).Year;
            }

            var financialStartYear = DateTime.ParseExact(_companySettings.FinancialYearStart + "/" + startingYear, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var financialEndYear = DateTime.ParseExact(_companySettings.FinancialYearEnd + "/" + endingYear, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            return new DateTime[] { financialStartYear, financialEndYear }; 
        }

        public void HandleEvent(EntityDeletedEvent<Employee> eventMessage)
        {
            var employee = eventMessage.Entity;
            var leaveRequests = _leaveRequestRepository.Table
                .Where(x => x.EmployeeId == employee.Id)
                .ToList();

            if (leaveRequests.Count() > 0)
            {
                var reasonIds = new List<int>();
                foreach (var leaveRequest in leaveRequests)
                {
                    var dates = _requestedDateRepository.Table
                        .Where(x => x.LeaveRequestId == leaveRequest.Id)
                        .ToList();

                    if(dates.Count() > 0)
                    {
                        foreach (var date in dates)
                        {
                            if (date.RejectionReasonId.HasValue && !reasonIds.Contains(date.RejectionReasonId.Value))
                            {
                                reasonIds.Add(date.RejectionReasonId.Value);
                            }

                            _requestedDateRepository.Delete(date);
                        }
                    }

                    _leaveRequestRepository.Delete(leaveRequest);
                }

                if (reasonIds.Count > 0)
                {
                    foreach (var reasonId in reasonIds)
                    {
                        var reason = _rejectionReasonRepository.Return(reasonId);
                        if (reason != null) _rejectionReasonRepository.Delete(reason);
                    }
                }
            }
        }
    }
}