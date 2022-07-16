using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using BugTracker.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BugTracker.Controllers
{
    public class InvitesController : Controller
    {
        private readonly IBTProjectService _projectsService;
        private readonly IBTInviteService _inviteService;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<BTUser> _userManager;
        public InvitesController(IBTProjectService projectsService, IBTInviteService inviteService, UserManager<BTUser> userManager, IBTCompanyInfoService companyInfoService, IEmailSender emailSender)
        {
            _projectsService = projectsService;
            _inviteService = inviteService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
            _emailSender = emailSender;
        }

        [Authorize(Roles = "Admin")]
        // GET: Invites
        public async Task<IActionResult> Index()
        {
            //var applicationDbContext = _context.Invites.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor).Include(i => i.Project);
            return View(/*await applicationDbContext.ToListAsync()*/);
        }

        [Authorize(Roles = "Admin")]
        // GET: Invites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var invite = await _context.Invites
            //    .Include(i => i.Company)
            //    .Include(i => i.Invitee)
            //    .Include(i => i.Invitor)
            //    .Include(i => i.Project)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (invite == null)
            //{
            //    return NotFound();
            //}

            return View();
        }

        [Route("Invites/ProcessInvite/{companyToken:guid}")]
        public async Task<IActionResult> ProcessInvite(string companyToken)
        {
            Guid companyTokenGuid = new(companyToken);

            bool isInviteTokenValid = await _inviteService.ValidateInviteCodeAsync(companyTokenGuid);

            if(isInviteTokenValid)
            {
                Invite invite = await _inviteService.GetInviteByGuidAsync(companyTokenGuid);
                return View(invite);
            }

            return View();
        }


        // GET: Invites/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            CreateInviteViewModel model = new();

            try
            {
                model.Projects = new SelectList(await _projectsService.GetAllProjectsByCompanyAsync(companyId), "Id", "Name");
            }
            catch (Exception)
            {
                throw;
            }

            return View(model);
        }


        // POST: Invites/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInviteViewModel model)
        {
            //original parameter list: [Bind("Id,CompanyId,ProjectId,InvitorId,InviteDate,InviteeEmail,InviteeFirstName,InviteeLastName")] Invite invite

            if (model.Invite is not null && model.SelectedProjectId >= 0)
            {
                Invite newInvite = new();
                int companyId = User.Identity.GetCompanyId().Value;
                BTUser user = await _userManager.GetUserAsync(User);

                newInvite.Company = await _companyInfoService.GetCompanyInfoByIdAsync(companyId);
                newInvite.InviteDate= DateTime.Now;
                newInvite.InviteeEmail = model.Invite.InviteeEmail;
                newInvite.InviteeFirstName = model.Invite.InviteeFirstName;
                newInvite.InviteeLastName = model.Invite.InviteeLastName;
                newInvite.Project = await _projectsService.GetProjectByIdAsync(model.SelectedProjectId, companyId);
                newInvite.Invitor = user;
                newInvite.CompanyToken = Guid.NewGuid();
                try
                {
                    await _inviteService.AddNewInviteAsync(newInvite);

                    try
                    {
                        string emailAddress = newInvite.InviteeEmail;
                        string message = $"{ newInvite.Invitor.FullName } has invited you to collaborate on { newInvite.Project.Name }. Accept the invitation here: https://localhost:44344/Invites/ProcessInvite/{ newInvite.CompanyToken }";
                        string emailSubject = $"Invitation to Collaborate";
                        await _emailSender.SendEmailAsync(emailAddress, emailSubject, message);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                RedirectToAction("Index", "Invites");
            }
            return View(model);
        }

        // GET: Invites/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var invite = await _context.Invites.FindAsync(id);
            //if (invite == null)
            //{
            //    return NotFound();
            //}
            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", invite.CompanyId);
            //ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            //ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", invite.ProjectId);
            return View();
        }

        // POST: Invites/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,ProjectId,InvitorId,InviteeId,CompanyToken,InviteDate,JoinDate,InviteeEmail,InviteeFirstName,InviteeLastName,IsValid")] Invite invite)
        {
            //if (id != invite.Id)
            //{
            //    return NotFound();
            //}

            //if (ModelState.IsValid)
            //{
            //    try
            //    {
            //        _context.Update(invite);
            //        await _context.SaveChangesAsync();
            //    }
            //    catch (DbUpdateConcurrencyException)
            //    {
            //        if (!InviteExists(invite.Id))
            //        {
            //            return NotFound();
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //    return RedirectToAction(nameof(Index));
            //}
            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", invite.CompanyId);
            //ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            //ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", invite.ProjectId);
            return View(invite);
        }

        // GET: Invites/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var invite = await _context.Invites
            //    .Include(i => i.Company)
            //    .Include(i => i.Invitee)
            //    .Include(i => i.Invitor)
            //    .Include(i => i.Project)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (invite == null)
            //{
            //    return NotFound();
            //}

            return View();
        }

        // POST: Invites/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //var invite = await _context.Invites.FindAsync(id);
            //_context.Invites.Remove(invite);
            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        private bool InviteExists(int id)
        {
            return false;
            //return _context.Invites.Any(e => e.Id == id);
        }
    }
}
