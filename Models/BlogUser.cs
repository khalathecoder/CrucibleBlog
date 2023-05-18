using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CrucibleBlog.Models
{
    public class BlogUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        [NotMapped] 
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        //Image Properties
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        [NotMapped]
        public virtual IFormFile? ImageFile { get; set; }

        //Navigation Properties
        public virtual ICollection<Comment> Comment { get; set; } = new HashSet<Comment>();

        public virtual ICollection<BlogLike> BlogLikes { get; set; } = new HashSet<BlogLike>();
    }
}
