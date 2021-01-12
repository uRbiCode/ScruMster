using Microsoft.AspNetCore.Identity;

namespace ScruMster.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the ScruMsterUser class
    public class ScruMsterUser : IdentityUser
    {
        [PersonalData]
        public string FirstName { get; set; }
        [PersonalData]
        public string LastName { get; set; }
        public bool IsBoss { get; set; }
        public virtual Team Team { get; set; }
        //public virtual ICollection<Sprint> Sprints { get; set; }
    }
}
