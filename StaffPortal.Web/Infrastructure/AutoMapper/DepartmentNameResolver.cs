using AutoMapper;
using StaffPortal.Service.Departments;

namespace StaffPortal.Web.Infrastructure.AutoMapper
{
    public class DepartmentNameResolver : IMemberValueResolver<object, object, int, string>
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentNameResolver(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        public string Resolve(object source, object destination, int sourceMember, string destMember, ResolutionContext context)
        {
            var department = _departmentService.GetDepartmentById(sourceMember);
            return department.Name;
        }
    }
}
