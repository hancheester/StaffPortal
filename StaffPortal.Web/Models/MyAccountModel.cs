using System.ComponentModel.DataAnnotations;

namespace StaffPortal.Web.Models
{
    public class MyAccountModel
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public string Email { get; set; }        
        [Required]
        public string NIN { get; set; }
        
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string RepeatPassword { get; set; }        
    }
}
