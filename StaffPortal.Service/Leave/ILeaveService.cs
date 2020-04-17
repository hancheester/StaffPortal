using StaffPortal.Common;
using StaffPortal.Common.APIModels;
using StaffPortal.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StaffPortal.Service.Leave
{
    public interface ILeaveService
    {
        decimal GetUsed(int employeeId);

        decimal GetAccruedAsPay(int employeeId);

        decimal GetAccruedAsDay(int employeeId);

        decimal GetPending(int employeeId);

        decimal GetNoImpactOnAllowance(int employeeId);

        IList<LeaveAPIModel> GetAllByEmployeeId(int employeeId, int statusCode, DateTime fromDate);

        IList<int> GetHolidayDatesByMonth(int employeeId, int month, int year);

        OperationResult<LeaveRequest> AddLeaveRequest(LeaveRequest leaveRequest);

        Task<IList<PendingLeaveRequest>> GetPendingLeaveRequestsAsync(string applicationUserId, int count = 5);

        decimal GetTotalLeaveQuota(int employeeId);

        decimal GetRemainingLeaveQuota(int employeeId);

        OperationResult<LeaveRequest> ValidateLeaveRequest(LeaveRequest request);
        IList<LeaveRequest> GetLeaveHistory(int employeeId, out int total, int pageNumber = 1, int pageSize = 10);
        OperationResult DisapproveLeaveRequest(string applicationUserId, int leaveRequestId, string reason);
        OperationResult ApproveLeaveRequest(string applicationUserId, int leaveRequestId);
        LeaveQuota GetLeaveQuota(int employeeId);
        IList<CalendarDay> GetPersonalCalendar(int employeeId, DateTime fromDate, DateTime toDate);
        IList<CalendarDay> GetDepartmentalCalendar(int departmentId, DateTime fromDate, DateTime toDate);
    }
}