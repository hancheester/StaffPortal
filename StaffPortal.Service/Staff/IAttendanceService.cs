using StaffPortal.Common;
using System;

namespace StaffPortal.Service.Staff
{
    public interface IAttendanceService
    {
        StaffLevelStatus GetDepartmentalAttendanceStatus(int departmentId, DateTime date, int? roleId = null);
        int GetDepartmentalStaffCount(int departmentId, DateTime date, DateTime currentDate, int? roleId = null);
    }
}
