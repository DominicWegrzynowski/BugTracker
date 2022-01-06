using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using BugTracker.Models.Enums;
using System.Linq;

namespace BugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {

        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _BTRolesService;
        private readonly IBTCompanyInfoService _BTCompanyInfoService;
        private readonly IBTProjectService _BTProjectService;
        public BTTicketService(ApplicationDbContext context, IBTRolesService bTRolesService, IBTCompanyInfoService bTCompanyInfoService, IBTProjectService bTProjectService)
        {
            _context = context;
            _BTRolesService = bTRolesService;
            _BTCompanyInfoService = bTCompanyInfoService;
            _BTProjectService = bTProjectService;
        }

        public async Task AddNewTicketAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            if(ticket != null)
            {
                try
                {
                    ticket.Archived = true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"*** ERROR *** - Error archiving ticket - {ex.Message}");
                }
            }
        }

        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            BTUser developer = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            Ticket ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);

            if(developer != null && ticket != null)
            {
                if (await _BTRolesService.IsUserInRoleAsync(developer, Roles.Developer.ToString()))
                {
                    try
                    {
                        ticket.DeveloperUserId = userId;
                        _context.Tickets.Update(ticket);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"*** ERROR *** - Error Assigning ticket - {ex.Message}");
                    }

                }
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Projects
                                                     .Where(p => p.CompanyId == companyId)
                                                     .SelectMany(p => p.Tickets)
                                                         .Include(t => t.Attachments)
                                                         .Include(t => t.Comments)
                                                         .Include(t => t.History)
                                                         .Include(t => t.DeveloperUser)
                                                         .Include(t => t.OwnerUser)
                                                         .Include(t => t.TicketPriority)
                                                         .Include(t => t.TicketStatus)
                                                         .Include(t => t.TicketType)
                                                         .Include(t => t.TicketType)
                                                     .ToListAsync();
                return tickets;
            }
            catch(Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            int priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;

            try
            {
                List<Ticket> tickets = await _context.Projects
                                                     .Where(p => p.CompanyId == companyId)
                                                     .SelectMany(p => p.Tickets)
                                                         .Include(t => t.Attachments)
                                                         .Include(t => t.Comments)
                                                         .Include(t => t.History)
                                                         .Include(t => t.DeveloperUser)
                                                         .Include(t => t.OwnerUser)
                                                         .Include(t => t.TicketPriority)
                                                         .Include(t => t.TicketStatus)
                                                         .Include(t => t.TicketType)
                                                         .Include(t => t.TicketType)
                                                     .Where(t => t.TicketPriorityId == priorityId)
                                                     .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            int statusId = (await LookupTicketStatusIdAsync(statusName)).Value;
            try
            {
                List<Ticket> tickets = await _context.Projects
                                                     .Where(p => p.CompanyId == companyId)
                                                     .SelectMany(p => p.Tickets)
                                                         .Include(t => t.Attachments)
                                                         .Include(t => t.Comments)
                                                         .Include(t => t.History)
                                                         .Include(t => t.DeveloperUser)
                                                         .Include(t => t.OwnerUser)
                                                         .Include(t => t.TicketPriority)
                                                         .Include(t => t.TicketStatus)
                                                         .Include(t => t.TicketType)
                                                         .Include(t => t.TicketType)
                                                     .Where(t => t.TicketStatusId == statusId)
                                                     .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            int typeId = (await LookupTicketTypeIdAsync(typeName)).Value;
            try
            {
                List<Ticket> tickets = await _context.Projects
                                                     .Where(p => p.CompanyId == companyId)
                                                     .SelectMany(p => p.Tickets)
                                                         .Include(t => t.Attachments)
                                                         .Include(t => t.Comments)
                                                         .Include(t => t.History)
                                                         .Include(t => t.DeveloperUser)
                                                         .Include(t => t.OwnerUser)
                                                         .Include(t => t.TicketPriority)
                                                         .Include(t => t.TicketStatus)
                                                         .Include(t => t.TicketType)
                                                         .Include(t => t.TicketType)
                                                     .Where(t => t.TicketTypeId == typeId)
                                                     .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = (await GetAllTicketsByCompanyAsync(companyId))
                                              .Where(t => t.Archived == true)
                                              .ToList();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            List<Ticket> companyTickets = await GetAllTicketsByCompanyAsync(companyId);
            List<Ticket> projectTickets = companyTickets.Where(c => c.ProjectId == projectId).ToList();

            return projectTickets.Where(t => t.TicketPriority.Name == priorityName).ToList();
            
        }

        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {
            List<Ticket> companyTickets = await GetAllTicketsByCompanyAsync(companyId);
            List<Ticket> projectTickets = companyTickets.Where(c => c.ProjectId == projectId).ToList();
            //GetUsersInRole, 
            //Come back to this
            
        }

        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            List<Ticket> companyTickets = await GetAllTicketsByCompanyAsync(companyId);
            List<Ticket> projectTickets = companyTickets.Where(c => c.ProjectId == projectId).ToList();
            
            return projectTickets.Where(t => t.TicketStatus.Name == statusName).ToList();
        }

        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            List<Ticket> companyTickets = await GetAllTicketsByCompanyAsync(companyId);
            List<Ticket> projectTickets = companyTickets.Where(c => c.ProjectId == projectId).ToList();

            return projectTickets.Where(t => t.TicketType.Name == typeName).ToList();
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            return await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId, int companyId)
        {
            BTUser developer = new BTUser();

            try
            {
                Ticket ticket = (await GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t => t.Id == ticketId);

                if(ticket?.DeveloperUserId is not null)
                {
                    developer = ticket.DeveloperUser; 
                }

                return developer;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
        {
            try
            {
                List<Ticket> tickets = new List<Ticket>();

                if (role == Roles.Admin.ToString())
                {
                    tickets = await GetAllTicketsByCompanyAsync(companyId);
                }

                if (role == Roles.Developer.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId))
                              .Where(t => t.DeveloperUserId == userId).ToList();
                }

                if(role == Roles.Submitter.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId))
                              .Where(t => t.OwnerUserId == userId).ToList();
                }

                if(role == Roles.ProjectManager.ToString())
                {
                    tickets = await GetTicketsByUserIdAsync(userId, companyId);
                }

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket> tickets = new List<Ticket>();

            try
            {
                if(await _BTRolesService.IsUserInRoleAsync(user, Roles.Admin.ToString()))
                {
                    tickets = (await _BTProjectService.GetAllProjectsByCompany(companyId))
                                                      .SelectMany(t => t.Tickets)
                                                      .ToList();
                }

                if(await _BTRolesService.IsUserInRoleAsync(user, Roles.Developer.ToString()))
                {
                    tickets = (await _BTProjectService.GetAllProjectsByCompany(companyId))
                                                      .SelectMany(t => t.Tickets)
                                                      .Where(t => t.DeveloperUserId == userId)
                                                      .ToList();
                }

                if(await _BTRolesService.IsUserInRoleAsync(user, Roles.Submitter.ToString()))
                {
                    tickets = (await _BTProjectService.GetAllProjectsByCompany(companyId))
                                                      .SelectMany(t => t.Tickets)
                                                      .Where(t => t.OwnerUserId == userId)
                                                      .ToList();
                }

                if (await _BTRolesService.IsUserInRoleAsync(user, Roles.ProjectManager.ToString()))
                {
                    tickets = (await _BTProjectService.GetUserProjectsAsync(userId))
                                                      .SelectMany(t => t.Tickets)
                                                      .ToList();
                }
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            try
            {
                TicketPriority priority = await _context.TicketPriorities.FirstOrDefaultAsync(t => t.Name == priorityName);
                return priority?.Id;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error finding priority id : {ex.Message}");
                throw;
            }
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            try
            {
                TicketStatus status = await _context.TicketStatuses.FirstOrDefaultAsync(t => t.Name == statusName);
                return status?.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding status id : {ex.Message}");
                throw;
            }
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            try
            {
                TicketType type = await _context.TicketTypes.FirstOrDefaultAsync(t => t.Name == typeName);
                return type?.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding type id : {ex.Message}");
                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            _context.Update(ticket);
            await _context.SaveChangesAsync();
        }
    }
}
