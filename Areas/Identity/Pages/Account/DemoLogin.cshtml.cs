using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace BugTracker.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class DemoLoginModel : PageModel
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly SignInManager<BTUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IConfiguration _config;

        public DemoLoginModel(SignInManager<BTUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<BTUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _config = config;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            //Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        public async Task<IActionResult> OnPostAsync(string user)
        {
            string returnUrl = Url.Content("~/Home/Dashboard");
            string email = "";
            string password = "";
        
            if(user == "admin")
            {
                email = _config["DemoLogin:DemoAdminUsername"];
                password = _config["DemoLogin:DemoAdminPassword"];
            }

            if (user == "pm")
            {
                email = _config["DemoLogin:DemoProjectManagerUsername"];
                password = _config["DemoLogin:DemoProjectManagerPassword"];
            }

            if (user == "dev")
            {
                email = _config["DemoLogin:DemoDeveloperUsername"];
                password = _config["DemoLogin:DemoDeveloperPassword"];
            }

            if (user == "submitter")
            {
                email = _config["DemoLogin:DemoSubmitterUsername"];
                password = _config["DemoLogin:DemoSubmitterPassword"];
            }

            var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = false });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }
    }
}
