using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
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
            //Add an instance of the ViewModel as a list 
            List<ManageUserRolesViewModel> model = new();
            //Get CompanyId

            //Get all company users
            
            //Loop over the users to populate the ViewModel
            //  -instantiate ViewModel
            //  -user _rolesService
            //  -Create multi-selects

            //Return the model to the view

            return View();
        }
    }
}
