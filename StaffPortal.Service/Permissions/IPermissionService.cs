using StaffPortal.Common;
using StaffPortal.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StaffPortal.Service.Permissions
{
    public interface IPermissionService
    {
        IList<Permission> GetAll();
        IList<Permission> GetPermissionsByBusinessRoleId(int id);
        OperationResult AllowPermission(int permissionId, int businessRoleId);
        OperationResult DisallowPermission(int permissionId, int businessRoleId);
        Task<bool> HasPermissionAsync(string permission, int employeeId);
    }
}