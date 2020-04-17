using StaffPortal.Common;
using StaffPortal.Common.Models;
using System;
using System.Collections.Generic;

namespace StaffPortal.Service.Staff
{
    public interface IWorkingDaysService
    {
        DaysWorkingResult InsertWeeklyHours(IList<WorkingDay> weekHours, int employeeId, string userId);

        IList<WorkingDay> GetWeeklyRotaByDepartmentId(int departmentId, DateTime fromDate);

        WorkingDay GetDayWorking(int employeeId, DateTime date);

        IList<WorkingDay> GetWeek(int employeeId);

        Allocation GetAllocation(int employeeId, DateTime date);
    }
}