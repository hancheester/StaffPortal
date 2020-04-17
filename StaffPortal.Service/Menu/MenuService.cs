using StaffPortal.Common;
using StaffPortal.Data;
using System.Collections.Generic;
using System.Linq;

namespace StaffPortal.Service.Menu
{
    public class MenuService : IMenuService
    {
        private readonly IRepository<MenuItem> _menuItemRepository;
        private readonly IRepository<BusinessRole_Permission> _businessRolePermissionRepository;
        private readonly IRepository<Permission_MenuItem> _permissionMenuItemRepository;
        
        public MenuService(
            IRepository<MenuItem> menuItemRepository,
            IRepository<BusinessRole_Permission> businessRolePermissionRepository,
            IRepository<Permission_MenuItem> permissionMenuItemRepository)
        {
            _menuItemRepository = menuItemRepository;
            _businessRolePermissionRepository = businessRolePermissionRepository;
            _permissionMenuItemRepository = permissionMenuItemRepository;
        }

        public IList<MenuItem> GetMenuItemsByBusinessRoleId(int businessRoleId)
        {
            var permissionIds = _businessRolePermissionRepository.Table
                .Where(x => x.BusinessRoleId == businessRoleId)
                .Select(x => x.PermissionId)
                .ToList();

            var menuItemIds = _permissionMenuItemRepository.Table
                .Where(x => permissionIds.Contains(x.PermissionId))
                .Select(x => x.MenuItemId)
                .ToList();

            var items = _menuItemRepository.Table
                .Where(x => x.ParentId == 0)
                .Where(x => menuItemIds.Contains(x.Id))
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            foreach (var item in items)
            {
                item.Children = _menuItemRepository.Table
                    .Where(x => x.ParentId == item.Id)
                    .Where(x => menuItemIds.Contains(x.Id))
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();
            }

            return items;
        }
    }
}
