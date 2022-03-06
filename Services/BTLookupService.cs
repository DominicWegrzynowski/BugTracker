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
        public async Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            try
            {
                return await _context.TicketPriorities.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Get Ticket Statuses
        public async Task<List<TicketStatus>> GetTicketStatusesAsync()
        {
            try
            {
                return await _context.TicketStatuses.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Get Ticket Types
        public async Task<List<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                return await _context.TicketTypes.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        } 
        #endregion
    }
}
