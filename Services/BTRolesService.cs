using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManager;
        public BTRolesService(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<BTUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        { 
            return (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
        }

        public async Task<string> GetRoleNameByIdAsync(string roleId)
        { 
             return await _roleManager.GetRoleNameAsync(_context.Roles.Find(roleId));  
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            return (await _userManager.GetUsersInRoleAsync(roleName)).ToList().Where(u => u.CompanyId == companyId).ToList();

        }

        public async Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
        {
            List<string> userIds = (await _userManager.GetUsersInRoleAsync(roleName)).Select(u => u.Id).ToList();
            List<BTUser> roleUsers = _context.Users.Where(u => !userIds.Contains(u.Id)).ToList();

            List<BTUser> result = roleUsers.Where(u => u.CompanyId == companyId).ToList();

            return result;
        }

        public async Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            return (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
        }

        public async Task<bool> RemoveUserFromRolesASync(BTUser user, IEnumerable<string> roles)
        {
            return (await _userManager.RemoveFromRolesAsync(user, roles)).Succeeded;
        }
    }
}
