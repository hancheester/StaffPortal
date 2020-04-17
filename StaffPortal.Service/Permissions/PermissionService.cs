using StaffPortal.Service.Errors;
using StaffPortal.Service.Events;
using StaffPortal.Common;
using StaffPortal.Common.Models;
using StaffPortal.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Service.Permissions
{
    public class PermissionService : IPermissionService, IConsumer<EntityDeletedEvent<BusinessRole>>
    {
        private readonly IErrorService _errorService;
        private readonly IRepository<Permission> _permissionRepository;
        private readonly IRepository<Employee_BusinessRole> _employeeBusinessRoleRepository;
        private readonly IRepository<BusinessRole_Permission> _businessRolePermissionRepository;

        public PermissionService(
            IErrorService errorService,
            IRepository<Permission> permissionRepository,
            IRepository<Employee_BusinessRole> employeeBusinessRoleRepository,
            IRepository<BusinessRole_Permission> businessRolePermissionRepository)
        {
            _errorService = errorService;
            _permissionRepository = permissionRepository;
            _employeeBusinessRoleRepository = employeeBusinessRoleRepository;
            _businessRolePermissionRepository = businessRolePermissionRepository;
        }

        public IList<Permission> GetAll()
        {
            return _permissionRepository.GetAll();
        }
        
        public IList<Permission> GetPermissionsByBusinessRoleId(int businessRoleId)
        {
            var permissions = _businessRolePermissionRepository.Table
                .Join(_permissionRepository.Table, b => b.PermissionId, p => p.Id, (b, p) => new { b, p })
                .Where(x => x.b.BusinessRoleId == businessRoleId)
                .Select(x => x.p)
                .ToList();

            return permissions;
        }

        public async Task<bool> HasPermissionAsync(string permission, int employeeId)
        {
            bool found = await Task.FromResult(_businessRolePermissionRepository.Table
                .Join(_employeeBusinessRoleRepository.Table, b => b.BusinessRoleId, e => e.BusinessRoleId, (b, e) => new { b, e })
                .Join(_permissionRepository.Table, x => x.b.PermissionId, p => p.Id, (x, p) => new { x.b, x.e, p })
                .Where(x => x.e.EmployeeId == employeeId)
                .Where(x => x.p.Name.ToLower() == permission.ToLower())
                .Any());

            return found;
        }

        public OperationResult AllowPermission(int permissionId, int businessRoleId)
        {
            var result = new OperationResult();

            try
            {
                var foundItem = _businessRolePermissionRepository.Table
                    .Where(x => x.PermissionId == permissionId)
                    .Where(x => x.BusinessRoleId == businessRoleId)
                    .FirstOrDefault();

                if (foundItem == null)
                {
                    var newItem = new BusinessRole_Permission
                    {
                        PermissionId = permissionId,
                        BusinessRoleId = businessRoleId
                    };

                    _businessRolePermissionRepository.Create(newItem);
                }
            }
            catch (Exception ex)
            {
                result.AddOperationError("E1", "Failed to allow permission.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public OperationResult DisallowPermission(int permissionId, int businessRoleId)
        {
            var result = new OperationResult();

            try
            {
                var foundItem = _businessRolePermissionRepository.Table
                    .Where(x => x.PermissionId == permissionId)
                    .Where(x => x.BusinessRoleId == businessRoleId)
                    .FirstOrDefault();

                if (foundItem != null)
                {
                    _businessRolePermissionRepository.Delete(foundItem);
                }
            }
            catch (Exception ex)
            {
                result.AddOperationError("E1", "Failed to disallow permission.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public void HandleEvent(EntityDeletedEvent<BusinessRole> eventMessage)
        {
            var role = eventMessage.Entity;
            var permissions = _businessRolePermissionRepository.Table.Where(x => x.BusinessRoleId == role.Id).ToList();

            if (permissions.Count() > 0)
            {
                foreach (var permission in permissions)
                {
                    _businessRolePermissionRepository.Delete(permission);
                }
            }
        }
    }
}
