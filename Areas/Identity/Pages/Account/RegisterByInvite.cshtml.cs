﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using BugTracker.Services.Interfaces;
using BugTracker.Models.Enums;

namespace BugTracker.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<BTUser> _signInManager;
        private readonly UserManager<BTUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IBTInviteService _inviteService;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyInfoService _companyInfoService;

        public RegisterModel(
            UserManager<BTUser> userManager,
            SignInManager<BTUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, IBTInviteService inviteService, IBTProjectService projectService, IBTCompanyInfoService companyInfoService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _inviteService = inviteService;
            _projectService = projectService;
            _companyInfoService = companyInfoService;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name ="Company")]
            public int CompanyId { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string id, string returnUrl = null)
        {

            Guid companyTokenGuid = new(id);

            bool isInviteTokenValid = await _inviteService.ValidateInviteCodeAsync(companyTokenGuid);

            if(isInviteTokenValid)
            {
                Invite invite = await _inviteService.GetInviteByGuidAsync(companyTokenGuid);
                ViewData["Invite"] = invite;
            }
        }

        public async Task<IActionResult> OnPostAsync(string id, string returnUrl = null)
        {
            //Keep default values passed from get method if user doesn't change them. 
            //Assign User to project that they are being invited to work on.
            //Set the user's company id to the incoming company id

            Guid companyTokenGuid = new(id);

            bool isInviteTokenValid = await _inviteService.ValidateInviteCodeAsync(companyTokenGuid);
            Invite invite = new();
            if (isInviteTokenValid)
            {
                invite = await _inviteService.GetInviteByGuidAsync(companyTokenGuid);
                ViewData["Invite"] = invite;
            }


            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new BTUser { UserName = Input.Email, Email = Input.Email, FirstName = Input.FirstName, LastName = Input.LastName, EmailConfirmed = true };
                user.Company = await _companyInfoService.GetCompanyInfoByIdAsync(invite.CompanyId);
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    try
                    { 
                        await _userManager.AddToRoleAsync(user, nameof(Roles.Developer));
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                  
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
