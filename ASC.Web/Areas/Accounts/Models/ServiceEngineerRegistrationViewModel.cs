using System.ComponentModel.DataAnnotations;
using ASC.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace ASC.Web.Areas.Accounts.Models
{
    public class ServiceEngineerRegistrationViewModel : IValidatableObject
    {
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public bool IsActive { get; set; }
        public bool IsEdit { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Nếu đang ở chế độ tạo mới (không phải edit), thì password là bắt buộc
            if (!IsEdit && string.IsNullOrEmpty(Password))
            {
                yield return new ValidationResult("Password is required when creating new account", new[] { nameof(Password) });
            }

            // Kiểm tra nếu Password không rỗng thì ConfirmPassword cũng không được rỗng và phải khớp
            if (!string.IsNullOrEmpty(Password) && string.IsNullOrEmpty(ConfirmPassword))
            {
                yield return new ValidationResult("Confirm Password is required", new[] { nameof(ConfirmPassword) });
            }
        }
    }
}