using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Areas.Accounts.Models
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

    }
}
