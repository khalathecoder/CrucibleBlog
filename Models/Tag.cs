using System.ComponentModel.DataAnnotations;

namespace CrucibleBlog.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tag Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Name { get; set; }

        public virtual ICollection<BlogPost> BlogPosts { get; set;} = new HashSet<BlogPost>();
    }
}
