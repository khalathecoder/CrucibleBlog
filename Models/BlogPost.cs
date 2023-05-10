using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace CrucibleBlog.Models
{
    public class BlogPost 
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Title { get; set; }

        [StringLength(600, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Abstract { get; set; }

        [Required]
        public string? Content { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public string? Created { get; set; }

        [DataType(DataType.DateTime)]
        public string? Updated { get; set; }

        //TODO: MAKE THIS REQUIRED LATER
        public string? Slug { get; set; }

        [Display(Name = "Deleted?")] //remove items from db
        public bool IsDeleted { get; set; }

        [Display(Name = "Published?")]
        public bool IsPublished { get; set; }
        public int? CategoryId { get; set; } //foreign key

        //Image Properties
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        [NotMapped]
        public virtual IFormFile? ImageFile { get; set; }

        //Navigation Properties
        //category
        public virtual Category? Category { get; set; }
            

        //Navigation Collections
            //comments
            public virtual ICollection<Comment> Comment { get; set; } = new HashSet<Comment>();
            //tags
            public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();
    }
}
