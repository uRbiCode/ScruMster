using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ScruMster.Models
{
    public class Sprint
    {
        public int SprintID { get; set; }
        public int TeamID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DeadlineDate { get; set; }
    }
}
