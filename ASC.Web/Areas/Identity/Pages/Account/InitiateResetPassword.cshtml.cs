using ASC.Utilities;
using ASC.Web.Services;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Policy;
using System.Text;
namespace ASC.Web.Areas.Identity.Pages.Account
{
    public class InitiateResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public InitiateResetPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Find User
            var userEmail = HttpContext.User.GetCurrentUserDetails().Email;
            var user = await _userManager.FindByEmailAsync(userEmail);

            // Generate User code
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Encode the token for URL safety
            var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { userId = user.Id, code = encodedCode },
                protocol: Request.Scheme);

            // Send Email
            var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style>
        body {{
            font-family: Arial, Helvetica, sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #f9f9f9;
            padding: 30px;
            border-radius: 8px;
            border: 1px solid #dddddd;
        }}
        h1 {{
            color: #333333;
            margin-top: 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 24px;
            background-color: #d9232d; /* Màu đỏ */
            color: white !important;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
            text-align: center;
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
        }}
        .button:hover {{
            background-color: #b91c24; /* Đỏ đậm hơn khi hover */
        }}
        .footer {{
            margin-top: 30px;
            font-size: 12px;
            color: #666666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Password Reset</h1>
        <p>Hello,</p>
        <p>We received a request to reset your password. Please click the button below to create a new password:</p>
        <div style='text-align: center;'>
            <a href='{callbackUrl}' class='button'>Reset Password</a>
        </div>
        <p>If you didn't request a password reset, you can safely ignore this email.</p>
        <p>This link will expire in 24 hours for security reasons.</p>
        <p>Best regards,<br/>The Support Team</p>
        <div class='footer'>
            <p>If you're having trouble clicking the button, copy and paste the URL below into your web browser:</p>
            <p>{callbackUrl}</p>
        </div>
    </div>
</body>
</html>";

            await _emailSender.SendEmailAsync(userEmail, "Reset Password", emailBody);
            TempData["Message"] = "Please check your email to reset your password.";
            return Page();
        }
    }
}