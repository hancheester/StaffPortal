using StaffPortal.Common;
using StaffPortal.Common.Models;
using System.Collections.Generic;

namespace StaffPortal.Service.Leave
{
    public interface ILeaveTypeService
    {
        IList<LeaveType> GetAllLeaveTypes();
        IList<LeaveType> GetAllRequestable();
        OperationResult<LeaveType> InsertLeaveType(LeaveType leaveType);
        LeaveType GetLeaveTypeById(int id);
        OperationResult DeleteLeaveType(int id);
        void UpdateLeaveType(LeaveType leaveType);        
    }
}