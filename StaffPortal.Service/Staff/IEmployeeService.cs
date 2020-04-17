using StaffPortal.Common;
using StaffPortal.Common.APIModels;
using StaffPortal.Common.Models;
using StaffPortal.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StaffPortal.Service.Staff
{
    public interface IEmployeeService
    { 
        Task<OperationResult<Employee>> Register(Employee employee, string username, string email, string password, string phoneNumber, int primaryBusinessRoleId, int[] secondaryBusinessRoleIds);
        Task<OperationResult<Employee>> Update(Employee employee, string username, string email, string password, int primaryBusinessRoleId, int[] secondaryBusinessRoleIds);
        Task<OperationResult> UpdateMyAccount(int employeeId, string firstName, string lastName, string phoneNumber, string gender, string nin, string email, string password);
        IList<Employee> GetEmployees(string userId, string firstName, string lastName, string email, out int total, int pageNumber = 1, int pageSize = 10);
        Task<OperationResult> DeleteAsync(int employeeId);
        Employee GetEmployeeById(int employeeId);



        decimal GetTotalLeaveQuota(int employeeId);

        decimal GetRemainingLeaveQuota(int employeeId);

        OperationResult<EmployeeInfoAPIModel> GetEmployeeByBarcode(string barcode);

        EmployeeInfoAPIModel GetAsAPIModel(int employeeId);

        EmployeeInfoAPIModel GetAsAPIModel(string userId);

        Employee GetEmployeeByApplicationUserId(string userId);

        IList<Employee> GetAll();

        IList<EmployeeInfoAPIModel> GetAll(int departmentId, int businessRoleId);

        IList<EmployeeInfoAPIModel> GetAll(AnnouncementRequest query);

        PagedList<Employee> GetPagedList(int pageIndex, int numberOfItems);

        void Update(Employee employee);

        int GetEmployeeIdOnApplicationUserId(string userId);

        void CreateEmployee(string userId);

        Task<EditEmployeeInfoAPIModel> PrepareUserToEdit(string applicationUserId);
        
        WeeklyShiftAPIModel GetWeeklyShiftWithAssignments(int departmentId, DateTime fromDate);

        WeeklyShiftAPIModel GetWeeklyShiftWithAssignments(int departmentId, int businessRoleId, DateTime fromDate);

        WeeklyShiftAPIModel GetWeeklyShiftWithAssignments(int departmentId, int businessRoleId, int employeeId, DateTime fromDate);

        OperationResult<Assignment> InsertAssignment(AssignmentAPIModel assignment);

        void InsertWeekAssignment(int employeeId, int departmentId, int businessRoleId, int recurringWeeks, DateTime startDate);

        IList<Assignment> GetAssignments(int employeeId, DateTime fromDate);

        TimeclockTimestamp InsertTimeclockTimeStamp(int employeeId);

        IList<EmployeeInfoAPIModel> GetAllInvitedEmployees(int trainingModuleId);

        IList<EmployeeInfoAPIModel> GetInvitableEmployees(int departmentId, int businessRoleId, int trainingModuleId);

        IList<Tuple<TimesheetDetails, TimesheetDetails>> GetTimesheet(int departmentId, int businessRoleId, DateTime date);

        void UpdateTimeclockTimestamp(TimeclockTimestamp timestamp);

        #region ACCOUNTANT REPORT
        AccountantReport GetAccountantReport(int employeeId, DateTime from, DateTime to);

        OperationResult<IList<AccountantReport>> GetAccountantReports(DateTime from, DateTime to);

        OperationResult<PagedList<AccountantReport>> GetAccountantReports(int pageIndex, int requestedItems, DateTime from, DateTime to);

        bool ApproveTimesheet(IList<TimesheetDetails> timeclockToEdit);

        #endregion ACCOUNTANT REPORT
    }
}