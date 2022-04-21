using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using BugTracker.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BugTracker.Models.ChartModels;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBTCompanyInfoService _companyService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;

        public HomeController(ILogger<HomeController> logger, IBTCompanyInfoService companyService, IBTProjectService projectService, IBTRolesService rolesService)
        {
            _logger = logger;
            _companyService = companyService;
            _projectService = projectService;
            _rolesService = rolesService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel model = new();
            int companyId = User.Identity.GetCompanyId().Value;

            model.Company = await _companyService.GetCompanyInfoByIdAsync(companyId);
            model.Projects = (await _companyService.GetAllProjectsAsync(companyId)).Where(p => p.Archived == false).ToList();
            model.Tickets = model.Projects
                                    .SelectMany(p => p.Tickets)
                                    .Where(t => t.Archived == false)
                                    .ToList();
            model.Members = model.Company.Members.ToList();

            foreach(BTUser member in model.Members)
            {
                if(await _rolesService.IsUserInRoleAsync(member, nameof(Roles.Admin)) == true)
                {
                    model.Roles?.Add("Admin");
                }
                if (await _rolesService.IsUserInRoleAsync(member, nameof(Roles.ProjectManager)) == true)
                {
                    model.Roles?.Add("Project Manager");
                }
                if (await _rolesService.IsUserInRoleAsync(member, nameof(Roles.Developer)) == true)
                {
                    model.Roles?.Add("Developer");
                }
                if (await _rolesService.IsUserInRoleAsync(member, nameof(Roles.Submitter)) == true)
                {
                    model.Roles?.Add("Submitter");
                }
                if (await _rolesService.IsUserInRoleAsync(member, nameof(Roles.DemoUser)) == true)
                {
                    model.Roles?.Add("Demo User");
                }
            }

            
            

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> GglProjectTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Project> projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);

            List<object> chartData = new();

            chartData.Add(new object[] { "ProjectName", "TicketCount" });

            foreach(Project project in projects)
            {
                chartData.Add(new object[] { project.Name, project.Tickets.Count() });
            }

            return Json(chartData);
        }

        [HttpPost]
        public async Task<JsonResult> GglProjectPriority()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "Priority", "Count" });

            foreach(string priority in Enum.GetNames(typeof(BTProjectPriority)))
            {
                int priorityCount = (await _projectService.GetAllProjectsByPriorityAsync(companyId, priority)).Count();
                chartData.Add(new object[] { priority, priorityCount });
            }

            return Json(chartData);
        }

        [HttpPost]
        public async Task<JsonResult> AmCharts()
        {
            AmChartData amChartData = new();
            List<AmItem> amItems = new();

            int company = User.Identity.GetCompanyId().Value;

            List<Project> projects = (await _companyService.GetAllProjectsAsync(company)).Where(p => p.Archived == false).ToList(); 

            foreach(Project project in projects)
            {
                AmItem item = new();

                item.Project = project.Name;
                item.Tickets = project.Tickets.Count;
                item.Developers = (await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(Roles.Developer))).Count();

                amItems.Add(item);
            }

            amChartData.Data = amItems.ToArray();

            return Json(amChartData.Data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
