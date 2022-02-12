using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTracker.Services.Interfaces
{
    public interface IBTRolesService
    {
        Task<bool> IsUserInRoleAsync(BTUser user, string roleName);

        Task<List<IdentityRole>> GetRolesAsync();
        Task<IEnumerable<string>> GetUserRolesAsync(BTUser user);
        Task<bool> AddUserToRoleAsync(BTUser user, string roleName);
        Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName);
        Task<bool> RemoveUserFromRolesASync(BTUser user, IEnumerable<string> roles);
        Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);
        Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId);
        Task<string> GetRoleNameByIdAsync(string roleId);
        
    }
}
