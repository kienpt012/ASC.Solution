using System.ComponentModel.DataAnnotations;
using ASC.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace ASC.Web.Areas.Accounts.Models
{
    public class ServiceEngineerRegistrationViewModel
    {
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public bool IsActive { get; set; }
        public bool IsEdit { get; set; }
    }
}