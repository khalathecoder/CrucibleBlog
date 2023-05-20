using System.ComponentModel.DataAnnotations;

namespace CrucibleBlog.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Comment")]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Body { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime UpdatedDate { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? UpdateReason { get; set; }

     
        public int BlogPostId { get; set; }
        public virtual BlogPost? BlogPost { get; set; }

       
        [Required]
        public string? AuthorId { get; set; }

        public virtual BlogUser? Author { get; set; }
    }
}
