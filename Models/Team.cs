using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScruMster.Areas.Identity.Data
{
    public class Team
    {
        /// <summary>
        /// DODANO
        /// </summary>
        public int TeamID { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string Name { get; set; }
        public string ownerID { get; set; }
        [NotMapped]
        public virtual ScruMsterUser owner { get; set; }
        public virtual ICollection<Sprint> Sprints { get; set; }
        // w tutorialu nic nie bylo, ale ja bym dodal z poziomu teamu zeby odwolywac sie do userow kotrzy w nim sa?
        public virtual ICollection<ScruMsterUser> ScruMsterUsers { get; set; }
    }
}
