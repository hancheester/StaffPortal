using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace StaffPortal.Web.Extensions
{
    public static class ClaimsExtensions
    {
        public static string GetApplicationUserId(this IEnumerable<Claim> claims)
        {
            var applicationUserId = claims
                .Where(x => x.Type == ClaimTypes.NameIdentifier)
                .Select(x => x.Value)
                .SingleOrDefault();

            return applicationUserId;
        }

        public static int GetEmployeeId(this IEnumerable<Claim> claims)
        {
            var employeeId = claims
                .Where(x => x.Type == "EmployeeId")
                .Select(x => x.Value)
                .SingleOrDefault();

            return string.IsNullOrEmpty(employeeId) ? 0 : Convert.ToInt32(employeeId);
        }

        public static int GetPrimaryBusinessRoleId(this IEnumerable<Claim> claims)
        {
            var primaryRoleId = claims
                .Where(x => x.Type == "PrimaryBusinessRoleId")
                .Select(x => x.Value)
                .SingleOrDefault();

            return string.IsNullOrEmpty(primaryRoleId) ? 0 : Convert.ToInt32(primaryRoleId);
        }
    }
}
