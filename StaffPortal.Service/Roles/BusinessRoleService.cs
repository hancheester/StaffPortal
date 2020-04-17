using StaffPortal.Common;
using StaffPortal.Common.Models;
using StaffPortal.Data;
using StaffPortal.Service.Errors;
using StaffPortal.Service.Events;
using StaffPortal.Service.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Service.Roles
{
    public class BusinessRoleService : IBusinessRoleService
    {
        private readonly IPermissionService _permissionService;
        private readonly IErrorService _errorService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<BusinessRole> _businessRoleRepository;
        private readonly IRepository<Employee_BusinessRole> _employeeBusinessRoleRepository;
        private readonly IRepository<WorkingDay> _daysWorkingRepositoryNew;
        private readonly IRepository<Permission> _permissionRepository;
        private readonly IRepository<BusinessRole_Permission> _businessRolePermissionRepository;

        public BusinessRoleService(
            IPermissionService permissionService,
            IErrorService errorService,
            IEventPublisher eventPublisher,
            IRepository<BusinessRole> businessRoleRepository,
            IRepository<Employee_BusinessRole> employeeBusinessRoleRepository,
            IRepository<WorkingDay> daysWorkingRepository,
            IRepository<Permission> permissionRepository,
            IRepository<BusinessRole_Permission> businessRolePermissionRepository)
        {
            _permissionService = permissionService;
            _errorService = errorService;
            _eventPublisher = eventPublisher;
            _businessRoleRepository = businessRoleRepository;
            _employeeBusinessRoleRepository = employeeBusinessRoleRepository;
            _daysWorkingRepositoryNew = daysWorkingRepository;
            _permissionRepository = permissionRepository;
            _businessRolePermissionRepository = businessRolePermissionRepository;
        }

        public OperationResult DeleteRole(int businessRoleId)
        {
            var result = new OperationResult();

            try
            {
                var role = _businessRoleRepository.Return(businessRoleId);

                if (role != null)
                {
                    _businessRoleRepository.Delete(role);

                    _eventPublisher.EntityDeleted(role);
                }
            }
            catch (Exception ex)
            {
                result.AddOperationError("E2", "Failed to delete department.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public OperationResult<BusinessRole> AddBusinessRole(BusinessRole role)
        {
            var result = new OperationResult<BusinessRole>(role);

            try
            {
                role.Id = _businessRoleRepository.Create(role);
            }
            catch (Exception ex)
            {
                result.AddOperationError("E1", "Failed to create business role.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public OperationResult<BusinessRole> UpdateBusinessRole(BusinessRole role)
        {
            var result = new OperationResult<BusinessRole>(role);

            try
            {
                if (role.Id == role.ParentBusinessRoleId)
                {
                    result.AddOperationError("E1", "Cannot be parent of itself.");
                    return result;
                }

                var foundRole = _businessRoleRepository.Return(role.Id);

                if(foundRole == null)
                {
                    result.AddOperationError("E1", "No such role.");
                    return result;
                }
                                
                var hasThisChild = HasThisChild(role.Id, role.ParentBusinessRoleId);

                if (hasThisChild)
                {
                    result.AddOperationError("E1", "Cannot have parent role under it's children.");
                    return result;
                }

                foundRole.Name = role.Name;
                foundRole.ParentBusinessRoleId = role.ParentBusinessRoleId;
                _businessRoleRepository.Update(foundRole);
                
            }
            catch (Exception ex)
            {
                result.AddOperationError("E1", "Failed to update business role.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;
        }

        public BusinessRole GetBusinessRoleById(int id)
        {
            var role = _businessRoleRepository.Return(id);
            var permissions = _businessRolePermissionRepository.Table
                .Join(_permissionRepository.Table, b => b.PermissionId, p => p.Id, (b, p) => new { b, p })
                .Where(x => x.b.BusinessRoleId == id)
                .Select(x => x.p)
                .ToList();
            role.Permissions = permissions;

            return role;
        }

        public async Task<OperationResult<IList<BusinessRole>>> GetBusinessRolesByEmployeeIdAsync(int employeeId)
        {
            var result = new OperationResult<IList<BusinessRole>>();

            try
            {
                var isAdmin = await _permissionService.HasPermissionAsync(PermissionKeys.ADMINISTRATIVE_ACCESS, employeeId);

                var roles = await Task.Run(() =>
                {
                    if (isAdmin) return _businessRoleRepository.GetAll();

                    return _employeeBusinessRoleRepository.Table
                        .Join(_businessRoleRepository.Table, e => e.BusinessRoleId, b => b.Id, (e, b) => new { e, b })
                        .Where(x => x.e.EmployeeId == employeeId)
                        .Select(x => x.b)
                        .ToList();

                });

                result.Object = roles;
            }
            catch (Exception ex)
            {
                result.AddOperationError("E1", "Failed to get business roles by employee.");
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));
            }

            return result;            
        }

        public IList<BusinessRole> GetRoleHierarchy(int parentId = 0)
        {
            var roles = _businessRoleRepository.Table
                .Where(x => x.ParentBusinessRoleId == parentId)
                .ToList();

            foreach (var role in roles)
            {
                var permissions = _businessRolePermissionRepository.Table
                    .Join(_permissionRepository.Table, b => b.PermissionId, p => p.Id, (b, p) => new { b, p })
                   .Where(x => x.b.BusinessRoleId == role.Id)
                   .Select(x => x.p)
                   .ToList();

                role.Permissions = permissions;
                role.Children = GetRoleHierarchy(role.Id);
            }

            return roles;
        }

        public BusinessRole GetPrimaryBusinessRoleByEmployeeId(int employeeId)
        {
            var primaryRole = _employeeBusinessRoleRepository.Table
                .Join(_businessRoleRepository.Table, e => e.BusinessRoleId, b => b.Id, (e, b) => new { e, b })
                .Where(x => x.e.EmployeeId == employeeId)
                .Where(x => x.e.IsPrimary == true)
                .Select(x => x.b)
                .FirstOrDefault();

            return primaryRole;
        }

        public IList<BusinessRole> GetSecondaryBusinessRolesOnEmployeeId(int employeeId)
        {
            var secondaryRoles = _employeeBusinessRoleRepository.Table
                .Join(_businessRoleRepository.Table, e => e.BusinessRoleId, b => b.Id, (e, b) => new { e, b })
                .Where(x => x.e.EmployeeId == employeeId)
                .Where(x => x.e.IsPrimary == false)
                .Select(x => x.b)
                .ToList();

            return secondaryRoles;
        }

        public void Insert(BusinessRole model)
        {
            _businessRoleRepository.Create(model);
        }

        public void Delete(BusinessRole model)
        {
            _businessRoleRepository.Delete(model);
        }

        public IList<BusinessRole> GetAll()
        {
            return _businessRoleRepository.GetAll();
        }

        private bool HasThisChild(int parentId, int childId)
        {
            var children = _businessRoleRepository.Table
                        .Where(x => x.ParentBusinessRoleId == parentId)
                        .ToList();

            if (children.Count > 0)
            {
                var found = children.Any(x => x.Id == childId);
                if (found) return found;

                foreach (var role in children)
                {
                    found = HasThisChild(role.Id, childId);
                    if (found) return found;
                }
            }

            return false;                        
        }
    }
}