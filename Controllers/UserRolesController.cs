using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    public class UserRolesController : Controller
    {
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyInfoService _companyInfoService;

        public UserRolesController(IBTRolesService rolesService, IBTCompanyInfoService companyInfoService)
        {
            _rolesService = rolesService;
            _companyInfoService = companyInfoService;
        }

        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();
            
            int companyId = User.Identity.GetCompanyId().Value;
            
            List<BTUser> users = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach(BTUser user in users)
            {
                ManageUserRolesViewModel viewModel = new();
                viewModel.BTUser = user;
                IEnumerable<string> selectedRoles = await _rolesService.GetUserRolesAsync(user);
                viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", selectedRoles);

                model.Add(viewModel);
            }

            return View(model);
        }
    }
}
