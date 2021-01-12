using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScruMster.Areas.Identity.Data
{
    public class Team
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TeamID { get; set; }
        public string Name { get; set; }
        public string Creator { get; set; }
        public virtual ICollection<ScruMsterUser> Users { get; set; }
    }
}
