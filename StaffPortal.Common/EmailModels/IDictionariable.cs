using System.Collections.Generic;

namespace StaffPortal.Common.EmailModels
{
    public interface IDictionariable
    {
        IDictionary<string, string> ToDictionary(string keyPrefix = "", string keySuffix = "");
    }
}