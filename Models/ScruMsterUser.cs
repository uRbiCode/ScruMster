﻿using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ScruMster.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the ScruMsterUser class
    public class ScruMsterUser : IdentityUser
    {
        [PersonalData]
        public string FirstName { get; set; }
        [PersonalData]
        public string LastName { get; set; }
        [PersonalData]
        public int? TeamID { get; set; }
        public virtual Team Team { get; set; }
        public virtual ICollection<Sprint> Sprints { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public bool Assigned { get; set; }
    }
}
