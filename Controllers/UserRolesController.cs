﻿using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    [Authorize]
    public class UserRolesController : Controller
    {
        #region Fields
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyInfoService _companyInfoService;
        #endregion

        #region Constructor
        public UserRolesController(IBTRolesService rolesService, IBTCompanyInfoService companyInfoService)
        {
            _rolesService = rolesService;
            _companyInfoService = companyInfoService;
        }
        #endregion

        #region Manage User Roles (GET)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            List<BTUser> users = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach (BTUser user in users)
            {
                ManageUserRolesViewModel viewModel = new();
                viewModel.BTUser = user;
                IEnumerable<string> selectedRoles = await _rolesService.GetUserRolesAsync(user);
                viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", selectedRoles);
                viewModel.AssignedRoles = await _rolesService.GetUserRolesAsync(user);

                model.Add(viewModel);
            }

            return View(model);
        }
        #endregion

        #region Manage User Roles (POST)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            if (member.BTUser is not null || member.SelectedRoles is not null)
            {


                int companyId = User.Identity.GetCompanyId().Value;

                BTUser btUser = (await _companyInfoService.GetAllMembersAsync(companyId)).FirstOrDefault(u => u.Id == member.BTUser.Id);

                IEnumerable<string> roles = await _rolesService.GetUserRolesAsync(btUser);

                string userRole = member.SelectedRoles.FirstOrDefault();

                if (!string.IsNullOrEmpty(userRole))
                {
                    if (await _rolesService.RemoveUserFromRolesASync(btUser, roles))
                    {
                        await _rolesService.AddUserToRoleAsync(btUser, userRole);
                    }
                }

                return RedirectToAction(nameof(ManageUserRoles));
            }
            else
            {
                return View(member);
            }
        }
        #endregion
    }
}
