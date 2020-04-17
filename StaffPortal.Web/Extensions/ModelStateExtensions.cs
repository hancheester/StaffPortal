using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace StaffPortal.Web.Extensions
{
    public static class ModelStateExtensions
    {
        public static IList<string> RetrieveModelStateErrorMessages(this ModelStateDictionary modelState)
        {
            var errorColletions = modelState.Select(x => x.Value.Errors)
                       .Where(y => y.Count > 0)
                       .ToList();

            var errorMessages = new List<string>();
            foreach (var errCollection in errorColletions)
            {
                foreach (var errMessage in errCollection)
                {
                    errorMessages.Add(errMessage.ErrorMessage);
                }
            }

            return errorMessages;
        }

        public static void AddErrors(this ModelStateDictionary modelState, Dictionary<string, string> CustomErrorMessages)
        {
            foreach (var err in CustomErrorMessages)
                modelState.AddModelError(err.Key, err.Value);
        }
    }
}