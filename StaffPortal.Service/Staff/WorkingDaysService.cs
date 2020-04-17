using StaffPortal.Common;
using StaffPortal.Common.Models;
using StaffPortal.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StaffPortal.Service.Staff
{
    public class WorkingDaysService : IWorkingDaysService
    {
        private readonly IRepository<LeaveRequest> _leaveRequestRepository;
        private readonly IRepository<RequestedDate> _requestedDateRepository;
        private readonly IRepository<WorkingDay> _daysWorkingRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<Assignment> _assignmentRepository;
        private readonly IRepository<Employee_BusinessRole> _employeeBusinessRoleRepository;
        private readonly IRepository<BusinessRole> _businessRoleRepository;
        private readonly IRepository<OpeningHour> _openingHourRepository;

        public WorkingDaysService(
            IRepository<LeaveRequest> leaveRequestRepository,
            IRepository<RequestedDate> requestedDateRepository,
            IRepository<WorkingDay> daysWorkingRepository,
            IRepository<Department> departmentRepository,
            IRepository<Assignment> assignmentRepository,
            IRepository<Employee_BusinessRole> employeeBusinessRoleRepository,
            IRepository<BusinessRole> businessRoleRepository,
            IRepository<OpeningHour> openingHourRepository)
        {
            _departmentRepository = departmentRepository;
            _assignmentRepository = assignmentRepository;
            _employeeBusinessRoleRepository = employeeBusinessRoleRepository;
            _businessRoleRepository = businessRoleRepository;
            _openingHourRepository = openingHourRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _requestedDateRepository = requestedDateRepository;
            _daysWorkingRepository = daysWorkingRepository;
        }

        // TODO: Consider to convert this method in accepting as parameter a DaysWorking Array instead of IList
        public DaysWorkingResult InsertWeeklyHours(IList<WorkingDay> weekHours, int employeeId, string userId)
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
                var depOpeningHours = _openingHourRepository.Table
                    .Where(x => x.DepartmentId == workingDay.DepartmentId)
                    .Where(x => x.Day == workingDay.Day)
                    .FirstOrDefault();
                    
                if (workingDay.IsAssigned && !depOpeningHours.IsOpen)
                {
                    result.Succeded = false;
                    result.DayWorkingResult.Add(new DayWorkingResult
                    {
                        DepartmentId = depOpeningHours.DepartmentId,
                        Message = string.Format("On the required day {0} department is closed.", workingDay.Day)
                    });
                }
            }
            if (result.Succeded)
                foreach (var workingDay in weekHours)
                {
                    _daysWorkingRepository.Create(workingDay);
                }

            return result;
        }

        // TODO: Consider to convert this method in returning an IList of DaysWorking Arrays: IList<DaysWorking[]>
        public IList<WorkingDay> GetWeeklyRotaByDepartmentId(int departmentId, DateTime fromDate)
        {
            var rota = _daysWorkingRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .ToList();

            //var rota = _repository.GetWeeklyRotaByDepartmentId(departmentId);
            //From old code base
            //public IList<DaysWorking> GetWeeklyRotaByDepartmentId(int departmentId)
            //{
            //    return GetAll()
            //            .Where(d => d.DepartmentId == departmentId)
            //            .Select(e => e.EmployeeId)
            //            .Distinct()
            //            .Join(_context.DaysWorking,
            //            e => e,
            //            d => d.EmployeeId,
            //            (e, d) => d)
            //            .ToList();
            //}

            // Old code
            //return _context.Assignments
            //            .Where(a => a.DepartmentId == departmentId && fromDate <= a.EndDate && !(toDate < a.StartDate))
            //            .ToList();

            var assignments = _assignmentRepository.Table
                .Where(x => x.DepartmentId == departmentId)
                .Where(x => x.StartDate < fromDate.AddDays(7))
                .Where(x => x.EndDate >= fromDate)
                .ToList();

            foreach (var assignment in assignments)
            {
                var index = rota.IndexOf(rota.FirstOrDefault(d => assignment.EmployeeId == d.EmployeeId && d.Day == assignment.Day));
                if (index >= 0)
                    rota[index].IsAssigned = false;
            }

            return rota;
        }

        public WorkingDay GetDayWorking(int employeeId, DateTime date)
        {
            var dayName = date.DayOfWeek.ToString();
            var shift = _daysWorkingRepository.Table
                .Where(x => x.EmployeeId == employeeId)
                .Where(x => x.Day == dayName)
                .FirstOrDefault();

            return shift;
        }

        // TODO: Consider to convert this method in returning a DaysWorking Array: DaysWorking[7]
        public IList<WorkingDay> GetWeek(int employeeId)
        {
            return _daysWorkingRepository.Table.Where(x => x.EmployeeId == employeeId).ToList();
        }

        public void Insert(WorkingDay model)
        {
            _daysWorkingRepository.Create(model);
        }

        public void Update(WorkingDay model)
        {
            if (!model.IsAssigned)
            {
                model.StartTime = new TimeSpan(0);
                model.EndTime = new TimeSpan(0);
                model.DepartmentId = -1;
            }

            _daysWorkingRepository.Update(model);
        }

        public void Delete(WorkingDay model)
        {
            _daysWorkingRepository.Delete(model);
        }

        public IList<WorkingDay> GetAll()
        {
            return _daysWorkingRepository.Table.ToList();
        }
        
        //TODO: Just don't like where Allocation class here is not deterministic.
        public Allocation GetAllocation(int employeeId, DateTime date)
        {
            var assignment = _assignmentRepository.Table
                .Where(x => x.EmployeeId == employeeId)
                .Where(x => x.StartDate.Date == date.Date)
                .FirstOrDefault();
            
            Allocation allocation = null;
            
            if (assignment != null)
            {
                allocation = new Allocation(assignment, date);
                var role = _businessRoleRepository.Return(allocation.RoleId);
                allocation.RoleName = role.Name;
                allocation.RoleId = role.Id;                
            }
            else
            {
                var dayWorking = GetDayWorking(employeeId, date);
                allocation = new Allocation(dayWorking, date);
                var role = _employeeBusinessRoleRepository.Table
                    .Join(_businessRoleRepository.Table, e => e.BusinessRoleId, b => b.Id, (e, b) => new { e, b })
                    .Where(x => x.e.EmployeeId == employeeId)
                    .Where(x => x.e.IsPrimary == true)
                    .Select(x => x.b)
                    .FirstOrDefault();

                if (role != null)
                {
                    allocation.RoleName = role.Name;
                    allocation.RoleId = role.Id;
                }
            }
            if (allocation.IsAssigned && allocation.DepartmentId > 0)
            {
                var department = _departmentRepository.Return(allocation.DepartmentId.Value);
                if (department != null)
                {
                    allocation.DepartmentName = department.Name;
                }                
            }

            //allocation.IsOnHoliday = _leaveRequestRepository.IsOnLeave(employeeId, date);
            var leaveRequestIds =_leaveRequestRepository.Table
                .Where(x => x.EmployeeId == employeeId)
                .Select(x => x.Id)
                .ToList();

            var isHoliday = _requestedDateRepository.Table
                .Where(x => leaveRequestIds.Contains(x.LeaveRequestId))
                .Where(x => x.Date == date.Date)
                .Where(x => x.StatusCode == (int)RequestStatus.Accepted)
                .Any();

            allocation.IsOnHoliday = isHoliday;

            return allocation;
        }
    }
}