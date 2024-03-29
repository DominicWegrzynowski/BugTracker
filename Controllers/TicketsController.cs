﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BugTracker.Extensions;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.ViewModels;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTLookupService _lookupService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketHistoryService _historyService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTRolesService _rolesService;
        public TicketsController(UserManager<BTUser> userManager, IBTProjectService projectService, IBTLookupService lookupService, IBTTicketService ticketService, IBTFileService fileService, IBTTicketHistoryService historyService, IBTNotificationService notificationService, IBTRolesService rolesService)
        {
            _userManager = userManager;
            _projectService = projectService;
            _lookupService = lookupService;
            _ticketService = ticketService;
            _fileService = fileService;
            _historyService = historyService;
            _notificationService = notificationService;
            _rolesService = rolesService;
        }

        // GET: MyTickets
        public async Task<IActionResult> MyTickets()
        {
            BTUser user = await _userManager.GetUserAsync(User);
            
            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(user.Id, user.CompanyId);

            return View(tickets);
        }

        // POST 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                try
                {
                    ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                    ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                    ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

                    ticketAttachment.Created = DateTimeOffset.Now;
                    ticketAttachment.UserId = _userManager.GetUserId(User);
                    ticketAttachment.User = await _ticketService.GetUserById(ticketAttachment.UserId);
                    await _ticketService.AddTicketAttachmentAsync(ticketAttachment);

                    await _historyService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.UserId);

                }
                catch (Exception)
                {
                    throw;
                }

                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }
        // POST: 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,TicketId,Comment,User")]TicketComment ticketComment)
		{
			if (ModelState.IsValid)
			{
				try
				{
                    ticketComment.UserId = _userManager.GetUserId(User);
                    ticketComment.User = await _ticketService.GetUserById(ticketComment.UserId);
                    ticketComment.Created = DateTimeOffset.Now;
                    
                    await _ticketService.AddTicketCommentAsync(ticketComment);

                    await _historyService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);
				}
				catch (Exception)
				{
					throw;
				}
			}

            return RedirectToAction("Details", new { id = ticketComment.TicketId });
		}

        // GET AllTickets
        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);

            if(User.IsInRole(nameof(Roles.Developer)) || User.IsInRole(nameof(Roles.Submitter)))
            {
                return View(tickets.Where(t => t.Archived == false));
            }
            else
            {
                return View(tickets);
            }
        }

        // GET ArchivedTickets
        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(companyId);

            return View(tickets);
        }

        // GET UnassignedTickets
        [Authorize(Roles="Admin,ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            string userId = _userManager.GetUserId(User);

            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                return View(tickets);
            }
            else
            {
                List<Ticket> pmTickets = new();

                foreach (Ticket ticket in tickets)
                {
                    if (await _projectService.IsAssignedProjectManagerAsync(userId, ticket.ProjectId))
                    {
                        pmTickets.Add(ticket);
                    }
                }
                return View(pmTickets);
            }
        }

        // GET Tickets/AssignDeveloper
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignDeveloper(int id)
        {
            AssignDeveloperViewModel model = new();

            model.Ticket = await _ticketService.GetTicketByIdAsync(id);
            try
            {
                model.Developers = new SelectList(await _projectService.GetProjectMembersByRoleAsync(model.Ticket.Project.Id, nameof(Roles.Developer)),
                                              "Id", "FullName");
            }
            catch(Exception)
            {
                throw;
            }
            
            return View(model);
        }

        // POST Tickets/AssignDeveloper
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel model)
        {
            if(model.DeveloperId is not null)
            {
                BTUser user = await _userManager.GetUserAsync(User);
                Ticket oldTicket = await _ticketService.GetTicketByIdAsync(model.Ticket.Id);
                
                try
                {

                    await _ticketService.AssignTicketAsync(model.Ticket.Id, model.DeveloperId);
                    
                }
                catch (Exception)
                {
                    throw;
                }

                Ticket newTicket = await _ticketService.GetTicketByIdAsync(model.Ticket.Id);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, user.Id);

                Notification developerNotification = new();
                developerNotification.Title = "Ticket Assigned";
                developerNotification.Sender = await _ticketService.GetUserById(user.Id);
                developerNotification.Recipient = await _ticketService.GetUserById(model.DeveloperId);
                developerNotification.Ticket = newTicket;
                developerNotification.Message = $"{ developerNotification.Sender.FullName } has assigned you a new Ticket: <a href='https://bugtrackerdw.herokuapp.com/Tickets/Details/{ developerNotification.Ticket.Id }'>{ developerNotification.Ticket.Title }</a>";
                developerNotification.Created = DateTimeOffset.Now;

                try
                {
                    await _notificationService.AddNotificationAsync(developerNotification);
                    await _notificationService.SendEmailNotificationAsync(developerNotification, "New Ticket Assigned");
                }
                catch (Exception)
                {
                    throw;
                }

                Ticket ticket = await _ticketService.GetTicketByIdAsync(model.Ticket.Id);

                Project project = ticket.Project;



                return RedirectToAction(nameof(Details), "Projects", new { id = newTicket.ProjectId});
            }

            return RedirectToAction(nameof(AssignDeveloper), new { id = model.Ticket.Id });
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            BTUser user = await _userManager.GetUserAsync(User);

            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(user.Id),  "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");

            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketStatusId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            BTUser user = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.Created = DateTimeOffset.Now;
                    ticket.OwnerUserId = user.Id;

                    ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(nameof(BTTicketStatus.New))).Value;

                    await _ticketService.AddNewTicketAsync(ticket);

                    Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                    await _historyService.AddHistoryAsync(null, newTicket, user.Id);

                    //TODO: Ticket Notification
                    return RedirectToAction(nameof(Details), new { Id = ticket.Id});
                }
                catch (Exception)
                {
                    throw;
                }
                    
            }

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(user.CompanyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(user.Id), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            if (ticket == null)
            {
                return NotFound();
            }
            
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketStatusId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,TicketStatusId,TicketTypeId,TicketPriorityId,OwnerUserId,DeveloperUserId,Title,Description,Created,Updated,Archived")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                BTUser user = await _userManager.GetUserAsync(User);
                //"AsNoTracking stops entity from tracking the ticket, allowing me to notice the changes later on" 
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                
                try
                {
                    ticket.Updated = DateTimeOffset.Now;
                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                //TODO: Add Ticket History
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, user.Id);

                //Notify Project manager + team members on project of ticket update. 

                return RedirectToAction(nameof(Details), new { Id = ticket.Id });
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketStatusId);

            return View(ticket);
        }

        // GET: Tickets/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            ticket.Archived = true;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction(nameof(AllTickets));
        }

        // GET: Tickets/Restore/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        public async Task<IActionResult> ShowFile(int id)
		{
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");

            return File(fileData, $"application/{ext}");
		}

        // POST: Tickets/Restore/5
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            ticket.Archived = false;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction(nameof(AllTickets));
        }

        private async Task<bool> TicketExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            return (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).Any(t => t.Id == id);
        }
    }
}
