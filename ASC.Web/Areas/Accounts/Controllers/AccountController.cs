using ASC.Utilities;
using ASC.Web.Areas.Accounts.Models;
using ASC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Web.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ServiceEngineers()
        {
            var serviceEngineers = await _userManager.GetUsersInRoleAsync(Roles.Engineer.ToString());

            // Hold all service engineers in session
            HttpContext.Session.SetSession("ServiceEngineers", serviceEngineers);

            return View(new ServiceEngineerViewModel
            {
                ServiceEngineers = serviceEngineers == null ? null : serviceEngineers.ToList(),
                Registration = new ServiceEngineerRegistrationViewModel() { IsEdit = false }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [Route("Accounts/Account/ServiceEngineers")]

        public async Task<IActionResult> ServiceEngineers(ServiceEngineerViewModel serviceEngineer)
        {
            var engineers = HttpContext.Session.GetSession<List<IdentityUser>>("ServiceEngineers");
            serviceEngineer.ServiceEngineers = engineers ?? new List<IdentityUser>();
            if (serviceEngineer.Registration.IsEdit)
            {
                // Loại bỏ validation lỗi cho Password và ConfirmPassword khi đang ở chế độ Edit
                ModelState.Remove("Registration.Password");
                ModelState.Remove("Registration.ConfirmPassword");
            }
            if (!ModelState.IsValid)
            {
                return View(serviceEngineer);
            }

            if (serviceEngineer.Registration.IsEdit)
            {

                // Update User
                var user = await _userManager.FindByEmailAsync(serviceEngineer.Registration.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "User with this email does not exist.");
                    return View(serviceEngineer);
                }
                user.UserName = serviceEngineer.Registration.UserName;
                IdentityResult result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    result.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }
                if(!string.IsNullOrEmpty(serviceEngineer.Registration.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    IdentityResult passwordResult = await _userManager.ResetPasswordAsync(user, token, serviceEngineer.Registration.Password);

                    if (!passwordResult.Succeeded)
                    {
                        passwordResult.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                        return View(serviceEngineer);
                    }
                }
                // Update Password
                // Update claims
                var identity = await _userManager.GetClaimsAsync(user);
                var isActiveClaim = identity.SingleOrDefault(p => p.Type == "IsActive");
                var removeClaimResult = await _userManager.RemoveClaimAsync(user, new System.Security.Claims.Claim(isActiveClaim.Type, isActiveClaim.Value));
                var addClaimResult = await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(isActiveClaim.Type, serviceEngineer.Registration.IsActive.ToString()));
            }
            else
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(serviceEngineer.Registration.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "A user with this email already exists.");
                    return View(serviceEngineer);
                }

                // Create User
                IdentityUser user = new IdentityUser
                {
                    UserName = serviceEngineer.Registration.UserName,
                    Email = serviceEngineer.Registration.Email,
                    EmailConfirmed = true
                };

                IdentityResult result = await _userManager.CreateAsync(user, serviceEngineer.Registration.Password);
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", serviceEngineer.Registration.Email));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", serviceEngineer.Registration.IsActive.ToString()));

                if (!result.Succeeded)
                {
                    result.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }

                // Assign user to Engineer Role
                var roleResult = await _userManager.AddToRoleAsync(user, Roles.Engineer.ToString());
                if (!roleResult.Succeeded)
                {
                    roleResult.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }
            }

            if (serviceEngineer.Registration.IsActive)
            {
                var user = await _userManager.FindByEmailAsync(serviceEngineer.Registration.Email);
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Account", new { token = token, email = user.Email }, protocol: HttpContext.Request.Scheme);
                await _emailSender.SendEmailAsync(serviceEngineer.Registration.Email, "Account Created/Modified",
                    $"Your account has been created/modified. Please reset your password here: {resetLink}");
                //var user = await _userManager.FindByEmailAsync(serviceEngineer.Registration.Email);

                //for (int i = 0; i < 1000; i++)
                //{
                //    try
                //    {
                //        // Tạo token reset password
                //        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                //        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                //        var resetLink = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Identity/Account/ResetPassword?userId={user.Id}&code={encodedCode}";

                //        // Tạo mã ngẫu nhiên để tránh nhóm email
                //        string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
                //        string emailSubject = $"Account Created/Modified - admin{i + 1} ({uniqueId})";

                //        // Gửi email
                //        await _emailSender.SendEmailAsync(
                //            serviceEngineer.Registration.Email,
                //            emailSubject,
                //            $"Your account (admin{i + 1}) has been created/modified. Please reset your password here: {resetLink} (Ref: {uniqueId})");
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine($"Failed to send email admin{i + 1} to {serviceEngineer.Registration.Email}: {ex.Message}");
                //    }
                //}
            }   
            else
            {
                await _emailSender.SendEmailAsync(serviceEngineer.Registration.Email, "Account Deactivated", $"Your account has been deactivated.");
            }

            return RedirectToAction("ServiceEngineers");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Customers()
        {
            var users = await _userManager.GetUsersInRoleAsync(Roles.User.ToString());

            // Hold all customers in session
            HttpContext.Session.SetSession("Customers", users);

            return View(new CustomersViewModel
            {
                Customers = users == null ? null : users.ToList(),
                Registration = new CustomerRegistrationViewModel()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [Route("Accounts/Account/Customers")]
        public async Task<IActionResult> Customers(CustomersViewModel customer)
        {
            var customers = HttpContext.Session.GetSession<List<IdentityUser>>("Customers");
            customer.Customers = customers ?? new List<IdentityUser>();
            if (!ModelState.IsValid)
            {
                return View(customer);
            }
            var user = await _userManager.FindByEmailAsync(customer.Registration.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User with this email does not exist.");
                return View(customer);
            }

            // Update username if it has changed
            if (user.UserName != customer.Registration.UserName)
            {
                user.UserName = customer.Registration.UserName;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(customer);
                }
            }

            // Update claims
            var identity = await _userManager.GetClaimsAsync(user);
            var isActiveClaim = identity.SingleOrDefault(p => p.Type == "IsActive");
            var removeClaimResult = await _userManager.RemoveClaimAsync(user,
                new System.Security.Claims.Claim(isActiveClaim.Type, isActiveClaim.Value));
            var addClaimResult = await _userManager.AddClaimAsync(user,
                new System.Security.Claims.Claim(isActiveClaim.Type, customer.Registration.IsActive.ToString()));

            if (!customer.Registration.IsActive)
            {
                await _emailSender.SendEmailAsync(customer.Registration.Email,
                    "Account Deactivated",
                    $"Your account has been Deactivated!!!");
            }
            else
            {
                await _emailSender.SendEmailAsync(customer.Registration.Email,
                    "Account Modified",
                    $"Your account has been {(user.UserName != customer.Registration.UserName ? "Updated" : "Activated")} Email: {customer.Registration.Email}");
            }

            return RedirectToAction("Customers");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = HttpContext.User.GetCurrentUserDetails();
            return View(new ProfileViewModel() { Username = user.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Accounts/Account/Profile")]
        public async Task<IActionResult> Profile(ProfileViewModel profile)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Update userName
            var user = await _userManager.FindByEmailAsync(HttpContext.User.GetCurrentUserDetails().Email);
            user.UserName = profile.Username;
            IdentityResult result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                return View();
            }

            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Profile", "Account", new { area = "Accounts" });
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }

    // Enum definition
    public enum Roles
    {
        Admin,
        Engineer,
        User
    }
}