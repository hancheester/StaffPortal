using StaffPortal.Common;
using StaffPortal.Common.Models;
using System;
using System.Collections.Generic;

namespace StaffPortal.Service.Shift
{
    public interface IShiftService
    {
        IList<Rota> GetRotas(int departmentId, int roleId, DateTime date, DateTime currentDate);

        IList<ShiftRole> GetRolesInRota(int departmentId, DateTime fromDate, DateTime toDate, int? roleId = null);

        OperationResult<Tuple<int, int>> GetMinimumStaffStatus(int departmentId, DateTime date);

        OperationResult<Assignment> CreateAssignment(Assignment assignment);

        OperationResult RemoveAssignment(int departmentId, int employeeId);
    }
}
