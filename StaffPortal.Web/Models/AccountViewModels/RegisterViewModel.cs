using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StaffPortal.Web.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string Firstname { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string Lastname { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please fill in your phone number.")]
        [Display(Name = "Phone No*:")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(((\+44\s?\d{4}|\(?0\d{4}\)?)\s?\d{3}\s?\d{3})|((\+44\s?\d{3}|\(?0\d{3}\)?)\s?\d{3}\s?\d{4})|((\+44\s?\d{2}|\(?0\d{2}\)?)\s?\d{4}\s?\d{4}))(\s?\#(\d{4}|\d{3}))?$",
            ErrorMessage = "Not a valid Phone number. Matching formats: +447222555555 | +44 7222 555 555 | (0722) 5555555")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Username { get; set; }


        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public bool TermsAgreed { get; set; }
    }
}
