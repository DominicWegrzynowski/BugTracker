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
    public class BTRolesService : IBTRolesService
    {
        #region Fields
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManager; 
        #endregion

        #region Constructor
        public BTRolesService(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<BTUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        #endregion

        #region Add User To Role
        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            return (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
        }
        #endregion

        #region Get Role Name By Id
        public async Task<string> GetRoleNameByIdAsync(string roleId)
        {
            return await _roleManager.GetRoleNameAsync(_context.Roles.Find(roleId));
        }

        #endregion

        #region Get Roles
        public async Task<List<IdentityRole>> GetRolesAsync()
        {
            try
            {
                List<IdentityRole> result = new();
                result = await _context.Roles.ToListAsync();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Get User Roles
        public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        #endregion

        #region Get Users In Role
        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            return (await _userManager.GetUsersInRoleAsync(roleName)).ToList().Where(u => u.CompanyId == companyId).ToList();

        }

        #endregion

        #region Get Users Not In Role
        public async Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
        {
            List<string> userIds = (await _userManager.GetUsersInRoleAsync(roleName)).Select(u => u.Id).ToList();
            List<BTUser> roleUsers = _context.Users.Where(u => !userIds.Contains(u.Id)).ToList();

            List<BTUser> result = roleUsers.Where(u => u.CompanyId == companyId).ToList();

            return result;
        }

        #endregion

        #region Is User In Role
        public async Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        #endregion

        #region Remove User From Role
        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            return (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
        }

        #endregion

        #region Remove User From Roles
        public async Task<bool> RemoveUserFromRolesASync(BTUser user, IEnumerable<string> roles)
        {
            return (await _userManager.RemoveFromRolesAsync(user, roles)).Succeeded;
        } 
        #endregion
    }
}
