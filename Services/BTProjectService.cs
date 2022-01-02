using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly BTRolesService _rolesService;
        public BTProjectService(ApplicationDbContext context, UserManager<BTUser> userManager, RoleManager<IdentityRole> roleManager, BTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _rolesService = rolesService;
        }

        public async Task AddNewProjectAsync(Project project)
        {
            _context.Add(project);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            //Find project
            Project project = await _context.Projects.Include(p => p.CompanyId).FirstOrDefaultAsync(p => p.Id == projectId);
            if(project != null)
            {
                //Check if it has a project manager
                List<BTUser> projectMangagers = await _rolesService.GetUsersInRoleAsync("Project Manager", project.CompanyId);
                if(projectMangagers is null)
                {
                    //If not find user
                    BTUser projectManagerUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                    //Add user to project manager role
                    bool addedUser = await _rolesService.AddUserToRoleAsync(projectManagerUser, "Project Manager");
                    return await AddUserToProjectAsync(userId, projectId);
                }
            }
            
           


            //Call AddUserToProjectAsync method

        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                if (!await IsUserOnProjectAsync(userId, projectId))
                {
                    try
                    {
                        project.Members.Add(user);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public async Task ArchiveProjectAsync(Project project)
        {
            project.Archived = true;
            _context.Update(project);
            await _context.SaveChangesAsync();
        }

        public Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            List<Project> projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == false)
                                                            .Include(p => p.Members) //Eager loading these properties
                                                            .Include(p => p.Tickets)
                                                                .ThenInclude(t => t.Comments)
                                                            .Include(p => p.Tickets)
                                                                .ThenInclude(t => t.Attachments)
                                                            .Include(p => p.Tickets)
                                                                .ThenInclude(t => t.Histroy)
                                                            .Include(p => p.Tickets)
                                                                .ThenInclude(t => t.DeveloperUser)
                                                            .Include(p => p.Tickets)
                                                                .ThenInclude(t => t.OwnerUser)
                                                            .Include(p => p.Tickets)
                                                                .ThenInclude(t => t.TicketStatus)
                                                            .Include(p => p.Tickets)
                                                                .ThenInclude(t => t.TicketPriority)
                                                            .Include(p => p.Tickets)
                                                                .ThenInclude(t => t.TicketType)
                                                            .Include(p => p.ProjectPriority)
                                                            .ToListAsync();

            return projects;
        }

        public async Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            List<Project> projects = await GetAllProjectsByCompany(companyId);
            int priorityId = await LookupProjectPriorityId(priorityName);

            return projects.Where(p => p.ProjectPriorityId == priorityId).ToList();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        {
            List<Project> projects = await GetAllProjectsByCompany(companyId);
            return projects.Where(p => p.Archived == true).ToList();
        }

        public Task<List<BTUser>> GetDevelopersOnProjectAsync(int projectId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            Project project = await _context.Projects
                                            .Include(p => p.Tickets)
                                            .Include(p => p.Members)
                                            .Include(p => p.ProjectPriority)
                                            .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

            return project;
        }

        public Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<BTUser>> GetSubmittersOnProjectAsync(int projectId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project> userProjects = (await _context.Users
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Company)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Members)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.DeveloperUser)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.OwnerUser)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.TicketPriority)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.TicketStatus)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.TicketType)
                                                           .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();

                return userProjects;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"*** ERROR *** Error getting user projects list. ---> {ex.Message}");
                throw;
            }   
        }

        public Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

            bool result = false;

            if(project != null)
            {
                result = project.Members.Any(m => m.Id == userId);
            }

            return result;
        }

        public async Task<int> LookupProjectPriorityId(string priorityName)
        {
            int priorityId = (await _context.ProjectPriorities.FirstOrDefaultAsync(p => p.Name == priorityName)).Id;
            return priorityId;
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                try
                {
                    if (await IsUserOnProjectAsync(user.Id, projectId))
                    {
                        project.Members.Remove(user);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                

            }
            catch(Exception ex)
            {
                Console.WriteLine($"*** ERROR *** - Error removing user from project. ---> {ex.Message}");
                throw;
            }
        }

        public Task RemoveUsersFromProjectByRoleAsync(string userId, int projectId)
        {
            throw new System.NotImplementedException();
        }

        public async Task UpdateProjectAsync(Project project)
        {
            _context.Update(project);
            await _context.SaveChangesAsync();
        }
    }
}
