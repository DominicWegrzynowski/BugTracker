﻿using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTCompanyInfoService : IBTCompanyInfoService
    {
        private readonly ApplicationDbContext _context;

        public BTCompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BTUser>> GetAllMembersAsync(int companyId)
        {
            return await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
        }

        public async Task<List<Project>> GetAllProjectsAsync(int companyId)
        {
            return await _context.Projects.Where(p => p.CompanyId == companyId)
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
        }

        public async Task<List<Ticket>> GetAllTicketsAsync(int companyId)
        {
            List<Ticket> result = new List<Ticket>();
            List<Project> projects = new List<Project>();

            projects = await GetAllProjectsAsync(companyId);

            result = projects.SelectMany(p => p.Tickets).ToList();

            return result;
        }

        public async Task<Company> GetCompanyInfoByIdASync(int? companyId)
        {
            Company result = new();

            if (companyId != null)
            {
                result = await _context.Companies
                                       .Include(c => c.Members)
                                       .Include(c => c.Projects)
                                       .Include(c => c.Invites)
                                       .FirstOrDefaultAsync(c => c.Id == companyId);
            }
            return result;
        }
    }
}
