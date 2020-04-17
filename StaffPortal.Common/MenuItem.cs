using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StaffPortal.Common
{
    public class MenuItem : BaseEntity
    {
        public string Title { get; set; }
        public string UrlSlug { get; set; }
        public int ParentId { get; set; }
        public int DisplayOrder { get; set; }

        [NotMapped]
        public IList<MenuItem> Children { get; set; }
    }
}