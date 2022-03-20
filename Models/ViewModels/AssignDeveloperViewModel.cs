using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class AssignDeveloperViewModel
    {
        public Ticket Ticket { get; set; }
        public SelectList Developers { get; set; }
        public string DeveloperId { get; set; }

    }
}
