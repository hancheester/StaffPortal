using StaffPortal.Common.APIModels;
using System;
using System.Collections.Generic;

namespace StaffPortal.Common.Models
{
    public class EmployeeInfoComparer : IEqualityComparer<EmployeeInfoAPIModel>
    {
        public bool Equals(EmployeeInfoAPIModel x, EmployeeInfoAPIModel y)
        {
            if (string.Equals(x.ApplicationUserId, y.ApplicationUserId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(EmployeeInfoAPIModel obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}