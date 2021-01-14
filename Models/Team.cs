using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ScruMster.Models
{
    public class Team
    {
        // if you want to implement your own value generation strategy and override the default behaviour, you will use the None option
        // [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TeamID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<Sprint> Sprints { get; set; }
    }
}
