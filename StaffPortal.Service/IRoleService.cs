using StaffPortal.Common;
using System;

namespace StaffPortal.Service
{
    public interface IRoleService
    {
        AssignedRole GetRoleSpecificDate(int employeeId, DateTime date);
    }
}
