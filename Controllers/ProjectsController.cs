using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Extensions;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using BugTracker.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IBTRolesService _rolesService;
        private readonly IBTLookupService _lookupService;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTNotificationService _notificationService;
        public ProjectsController(IBTRolesService rolesService,
                                  IBTLookupService lookupService,
                                  IBTFileService fileService,
                                  IBTProjectService projectService, UserManager<BTUser> userManager, IBTCompanyInfoService companyInfoService, IBTNotificationService notificationService)
        {
            _rolesService = rolesService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
            _notificationService = notificationService;
        }

        //GET : Projects/MyProjects
        public async Task<IActionResult> MyProjects()
        {
            string userId = _userManager.GetUserId(User);

            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            return View(projects);
        }

        //GET : Projects/AllProjects
        public async Task<IActionResult> AllProjects()
        {
            List<Project> projects = new List<Project>();

            int companyId = User.Identity.GetCompanyId().Value;

            if(User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
            {
                projects = await _companyInfoService.GetAllProjectsAsync(companyId);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);
            }

            return View(projects);
        }

        // GET : Projects/ArchivedProjects
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetArchivedProjectsByCompanyAsync(companyId);

            return View(projects);
        }

        // GET : Projects/AssignPM
        [Authorize(Roles="Admin")]
        [HttpGet]
        public async Task<IActionResult> AssignPM(int projectId)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AssignPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(projectId, companyId);
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(Roles.ProjectManager), companyId), "Id", "FullName");

            return View(model);
        }

        // POST : /Projects/AssignPM
        [Authorize(Roles="Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            if(!string.IsNullOrEmpty(model.PMID))
            {
                await _projectService.AddProjectManagerAsync(model.PMID, model.Project.Id);

                return RedirectToAction(nameof(Details), new { id = model.Project.Id });
            }
            return RedirectToAction(nameof(AssignPM), new { projectId = model.Project.Id });
        }

        // GET : Projects/AssignMembers
        [Authorize(Roles="Admin, ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignMembers(int id)
        {
            ProjectMembersViewModel model = new();
            int companyId = User.Identity.GetCompanyId().Value;

            model.Project = await _projectService.GetProjectByIdAsync(id, companyId);

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(Roles.Developer), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(Roles.Submitter), companyId);

            List<BTUser> companyMembers = developers.Concat(submitters).ToList();

            List<string> projectMembers = model.Project.Members.Select(m => m.Id).ToList();

            model.Users = new MultiSelectList(companyMembers, "Id", "FullName", projectMembers);

            return View(model);
        }

        // POST : Projects/AssignMembers/
        [Authorize(Roles="Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if(model.SelectedUsers is not null)
            {
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id))
                                                               .Select(m => m.Id).ToList();
                //Remove current members
                foreach (string member in memberIds)
                {
                    await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);
                }

                //Add selected members
                foreach (string member in model.SelectedUsers)
                {
                    await _projectService.AddUserToProjectAsync(member, model.Project.Id);
                }

                //Send Notification to Each member being assigned/removed
                int companyId = User.Identity.GetCompanyId().Value;
                BTUser user = await _userManager.GetUserAsync(User);
                List<BTUser> selectedMembers = new();

                foreach (string selectedUser in model.SelectedUsers)
                {
                    BTUser member = await _companyInfoService.GetUserById(selectedUser, companyId);

                    selectedMembers.Add(member);

                    Notification assignedMemberNotification = new();
                    assignedMemberNotification.Title = "Ticket Assigned";
                    assignedMemberNotification.Sender = user;
                    assignedMemberNotification.Recipient = member;
                    assignedMemberNotification.Message = $"{ assignedMemberNotification.Sender.FullName } has added you to a project: <a href='https://localhost:44344/Projects/Details/{ model.Project.Id }'>{ model.Project.Name }</a>";
                    assignedMemberNotification.Created = DateTimeOffset.Now;

                    try
                    {
                        await _notificationService.AddNotificationAsync(assignedMemberNotification);
                        await _notificationService.SendEmailNotificationAsync(assignedMemberNotification, "New Ticket Assigned");
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    //Send if Admin assigned/removed members, notify Project Manager
                    if (User.IsInRole(nameof(Roles.Admin)))
                    {
                        BTUser projectManager = await _projectService.GetProjectManagerAsync(model.Project.Id);
                        Notification notifyProjectManagerNotification = new();
                        notifyProjectManagerNotification.Title = "Ticket Assigned";
                        notifyProjectManagerNotification.Sender = user;
                        notifyProjectManagerNotification.Recipient = projectManager;
                        notifyProjectManagerNotification.Message = $"{ notifyProjectManagerNotification.Sender.FullName } has assigned { member.FullName } to a project: <a href='https://localhost:44344/Projects/Details/{ model.Project.Id }'>{ model.Project.Name }</a>";

                        try
                        {
                            await _notificationService.AddNotificationAsync(assignedMemberNotification);
                            await _notificationService.SendEmailNotificationAsync(assignedMemberNotification, "New Ticket Assigned");
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    //If Project Manager assigned/removed members, notify admin
                    if (User.IsInRole(nameof(Roles.ProjectManager)))
                    {
                        List<BTUser> admins = await _companyInfoService.GetAdminsByCompanyId(companyId);

                        foreach(BTUser admin in admins)
                        {
                            Notification notifyAdminNotification = new();
                            notifyAdminNotification.Title = "Ticket Assigned";
                            notifyAdminNotification.Sender = user;
                            notifyAdminNotification.Recipient = admin;
                            notifyAdminNotification.Message = $"{ notifyAdminNotification.Sender.FullName } has assigned { member.FullName } to a project: <a href='https://localhost:44344/Projects/Details/{ model.Project.Id }'>{ model.Project.Name }</a>";

                            try
                            {
                                await _notificationService.AddNotificationAsync(assignedMemberNotification);
                                await _notificationService.SendEmailNotificationAsync(assignedMemberNotification, "New Ticket Assigned");
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                }
                return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
            }

            return RedirectToAction(nameof(AssignMembers), new { id = model.Project.Id });
        }

        // GET : Projects/UnassignedProjects
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = new List<Project>();

            projects = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles="Admin, ProjectManager")]
        public async Task<IActionResult> CreateAsync()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AddProjectWithPMViewModel model = new();

            model.PmList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");
            
            return View(model);
        }

        // POST: Projects/Create
        [Authorize(Roles="Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(AddProjectWithPMViewModel model)
        {
            if(model is not null)
            {
                int companyId = User.Identity.GetCompanyId().Value;
                try
                {
                    if(model.Project.ImageFormFile is not null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageFileContentType = model.Project.ImageFormFile.ContentType;
                    }

                    model.Project.CompanyId = companyId;

                    await _projectService.AddNewProjectAsync(model.Project);

                    //Add Pm if one was chosen
                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);
                    }

                    //TODO: Redirect to All Projects
                    return RedirectToAction("AllProjects");
                }
                catch (Exception)
                {
                    throw;
                }
            }
           
            return RedirectToAction("Create");
        }

        // GET: Projects/Edit/5
        [Authorize(Roles="Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AddProjectWithPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            model.Project.CompanyId = companyId;

            model.PmList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");

            return View(model);
        }

        // POST: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Project, PMList, PMId, PriorityList")]AddProjectWithPMViewModel model)
        {
            if (model is not null)
            {
                try
                {
                    if (model.Project.ImageFormFile is not null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageFileContentType = model.Project.ImageFormFile.ContentType;
                    }

                    await _projectService.UpdateProjectAsync(model.Project);

                    //Add Pm if one was chosen
                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);
                    }

                    return RedirectToAction("Details", new { id = model.Project.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProjectExists(model.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
            }

            return RedirectToAction("Edit");

        }

        // GET: Projects/Archive/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.ArchiveProjectAsync(project);

            return RedirectToAction(nameof(AllProjects));
        }

        // POST: Projects/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.RestoreProjectAsync(project);

            return RedirectToAction(nameof(AllProjects));
        }
        private async Task<bool> ProjectExists(int id)
        {
            var companyId = User.Identity.GetCompanyId().Value;

            return (await _projectService.GetAllProjectsByCompanyAsync(companyId)).Any(t => t.Id == id);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> ManageProjects()
        {
            List<Project> projects = new List<Project>();

            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
            {
                projects = await _companyInfoService.GetAllProjectsAsync(companyId);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);
            }

            return View(projects);
        }

    }
}
