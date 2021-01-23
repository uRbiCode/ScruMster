using System;
using System.ComponentModel.DataAnnotations;

namespace ScruMster.Areas.Identity.Data
{
    public class Comment
    {
        public int CommentId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yy HH:mm:ss} ", ApplyFormatInEditMode = true)]
        public DateTime AddTime { get; set; }

        [Required]
        [StringLength(250, ErrorMessage = "Comment cannot be longer than 250 characters.")]
        public string Text { get; set; }

        public int SprintID { get; set; }
        public virtual Sprint Sprint { get; set; }

      //  public int AuthorId { get; set; }
        public virtual ScruMsterUser Author { get; set; }

        public Comment()
        {
            AddTime = DateTime.Now;
           

        }
    }
}
