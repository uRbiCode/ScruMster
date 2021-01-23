using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScruMster.Areas.Identity.Data
{
    public class Sprint
    {
        /// <summary>
        /// DODANO
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Number")]
        public int SprintID { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string Name { get; set; }
        [Required]
        [StringLength(250, ErrorMessage = "Description cannot be longer than 250 characters.")]
        public string Description { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Deadline Date")]
        public DateTime Deadline { get; set; }
        [Required]
        public bool IsDone { get; set; }
        [Required]
        public int? TeamID { get; set; }
        public virtual Team Team { get; set; }
        public virtual ICollection<ScruMsterUser> ScruMsterUsers { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
