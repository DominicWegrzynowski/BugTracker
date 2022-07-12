using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class CreateInviteViewModel
    {
        public Invite Invite { get; set; }
        public SelectList Projects { get; set; }
        public int? SelectedProjectId { get; set; }
    }
}
