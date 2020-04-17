using StaffPortal.Common;

namespace StaffPortal.Web.Models
{
    public partial class CheckableBusinessRole : BusinessRole
    {
        public bool IsChecked { get; set; }
        public int MinimumRequired { get; set; }
    }
}