using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public BTUser Member { get; set; }
        public MultiSelectList Roles { get; set; }
        public List<string> SelectedRoles { get; set; }
        //For a string version of the roles to filter the loop
        public IEnumerable<string> AssignedRoles { get; set; }

    }
}
