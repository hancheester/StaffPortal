using StaffPortal.Common;
using StaffPortal.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StaffPortal.Service.Departments
{
    public interface IDepartmentService
    {
        OperationResult<Department> AddDepartment(Department department);
        OperationResult DeleteDepartment(int departmentId);
        OperationResult<Department> UpdateDepartment(Department department);
        Task<IList<Department>> GetAllDepartmentsAsync();
        Task<IList<Department>> GetAssignedDepartmentsByEmployeeIdAsync(int employeeId);
        Department GetDepartmentById(int departmentId);
        Task<int> GetMinimumRequiredStaffAsync(int departmentId);
        int GetMinimumRequiredStaff(int departmentId);





        int GetStaffCount(int departmentId, DateTime date, int businessRoleId);

        int GetStaffCount(int departmentId, DateTime date);

        int GetStaffLevelStatus(int departmentId, int businessRoleId, DateTime date);

        // Retrieves all the Ids of the employees working on the given date, whether they are assigned to the shift 
        // based on Assignments or DaysWorking
        int[] GetEmployeeIds(int departmentId, DateTime date);

        int[] GetEmployeeIdsOnHolidays(int departmentId, DateTime date);
    }
}