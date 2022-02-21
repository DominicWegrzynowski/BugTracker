using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class AddProjectWithPMViewModel
    {
        public Project Project { get; set; }
        public SelectList PmList { get; set; }
        public string PmId { get; set; }
        public SelectList PriorityList { get; set; }
        public int ProjectPriority { get; set; }
    }
}
