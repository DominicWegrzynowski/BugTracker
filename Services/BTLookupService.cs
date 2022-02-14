using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTLookupService : IBTLookupService
    {
        #region Fields
        private readonly ApplicationDbContext _context; 
        #endregion

        #region Constructor
        public BTLookupService(ApplicationDbContext context)
        {
            _context = context;
        } 
        #endregion

        #region Get Project Priorities
        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                return await _context.ProjectPriorities.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Get Ticket Priorities
        public Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Get Ticket Statuses
        public Task<List<TicketStatus>> GetTicketStatusesAsync()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Get Ticket Types
        public Task<List<TicketType>> GetTicketTypesAsync()
        {
            throw new System.NotImplementedException();
        } 
        #endregion
    }
}
