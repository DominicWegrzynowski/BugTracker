using BugTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTracker.Services.Interfaces
{
    public interface IBTLookupService
    {
        public Task<List<TicketPriority>> GetTicketPrioritiesAsync();
        public Task<List<TicketStatus>> GetTicketStatusesAsync();
        public Task<List<TicketType>> GetTicketTypesAsync();
        public Task<List<ProjectPriority>> GetProjectPrioritiesAsync();

    }
}
