using StaffPortal.Common;
using StaffPortal.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StaffPortal.Service.Roles
{
    public interface IBusinessRoleService
    {
        OperationResult<BusinessRole> AddBusinessRole(BusinessRole role);
        OperationResult<BusinessRole> UpdateBusinessRole(BusinessRole role);
        IList<BusinessRole> GetRoleHierarchy(int parentId = 0);
        IList<BusinessRole> GetAll();
        OperationResult DeleteRole(int businessRoleId);
        Task<OperationResult<IList<BusinessRole>>> GetBusinessRolesByEmployeeIdAsync(int employeeId);
        void Insert(BusinessRole businessRole);

        

        BusinessRole GetBusinessRoleById(int id);

        BusinessRole GetPrimaryBusinessRoleByEmployeeId(int employeeId);

        IList<BusinessRole> GetSecondaryBusinessRolesOnEmployeeId(int employeeId);
    }
}