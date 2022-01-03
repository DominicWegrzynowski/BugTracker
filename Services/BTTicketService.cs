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
        public BTTicketService(ApplicationDbContext context, IBTRolesService bTRolesService, IBTCompanyInfoService bTCompanyInfoService)
        {
            _context = context;
            _BTRolesService = bTRolesService;
            _BTCompanyInfoService = bTCompanyInfoService;
        }

        public async Task AddNewTicketAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            Ticket archivedTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticket.Id);

            if(archivedTicket != null)
            {
                try
                {
                    archivedTicket.Archived = true;
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
                return await _BTCompanyInfoService.GetAllTicketsAsync(companyId);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"***ERROR*** Error Getting tickets - {ex.Message}");
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);

            tickets = tickets.Where(t => t.TicketPriority.Name == priorityName).ToList();

            return tickets;

        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);

             return tickets = tickets.Where(t => t.TicketStatus.Name == statusName).ToList();
        }

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);

            return tickets = tickets.Where(t => t.TicketType.Name == typeName).ToList();

        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);

            return tickets.Where(t => t.Archived == true).ToList();
        }

        public Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            throw new System.NotImplementedException();
        }

        public Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            throw new System.NotImplementedException();
        }

        public Task<BTUser> GetTicketDeveloperAsync(int ticketId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            throw new System.NotImplementedException();
        }

        public Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            throw new System.NotImplementedException();
        }

        public Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            throw new System.NotImplementedException();
        }

        public Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateTicketAsync(Ticket ticket)
        {
            throw new System.NotImplementedException();
        }
    }
}
