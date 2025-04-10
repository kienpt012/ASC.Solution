using ASC.Web.Configuration;
using Microsoft.AspNetCore.Identity;

namespace ASC.Web.Areas.Accounts.Models
{
    public class CustomersViewModel
    {
        public List<IdentityUser>? Customers { get; set; }
        public CustomerRegistrationViewModel Registration { get; set; }
    }
}