using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CrucibleBlog.Data;
using CrucibleBlog.Models;
using CrucibleBlog.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using CrucibleBlog.Services;
using CrucibleBlog.Helpers;
using X.PagedList;

namespace CrucibleBlog.Controllers
{
    [Authorize(Roles="Admin")]
    public class BlogPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IBlogService _blogService;

        public BlogPostsController(ApplicationDbContext context, UserManager<BlogUser> userManager, IImageService imageService, IBlogService blogService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _blogService = blogService;
        }

        // GET: BlogPosts
        public async Task<IActionResult> Index()
        {

            var applicationDbContext = _context.BlogPosts.Include(b => b.Category);
            return View(await applicationDbContext.ToListAsync());
        }



        // GET: BlogPosts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

          BlogPost? blogPost = await _context.BlogPosts
                                            .Include(b => b.Category)
                                            .Include(b => b.Comments)
                                                .ThenInclude(c=>c.Author)
                                            .Include(b=>b.Tags)
											.Include(b => b.Likes)
											.FirstOrDefaultAsync(m => m.Slug == slug);

            if (blogPost == null)
            {
                return NotFound();
            }



            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            return View(blogPost);
        }

        // GET: BlogPosts/Create //get method says hey just show me that page
        public IActionResult Create()   
        {
            BlogPost blogPost = new BlogPost();
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");

            return View(blogPost);
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost] //post means we are receiving new info 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Abstract,Content,CreatedDate,IsDeleted,IsPublished,CategoryId,ImageFile")] BlogPost blogPost, string? stringTags)
        {
            ModelState.Remove("Slug"); //removes requirement for slug 
            
            if (ModelState.IsValid)

            {

				if (blogPost.ImageFile != null)
				{
					blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.ImageFile);
					blogPost.ImageType = blogPost.ImageFile.ContentType;
				}

				if (!await _blogService.ValidSlugAsync(blogPost.Title, blogPost.Id))
                {
                    ModelState.AddModelError("Title", "A similar Title/Slug is already in use.");

					ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
					return View(blogPost);
				}             

				blogPost.Slug = StringHelper.BlogPostSlug(blogPost.Title); //turning title based on stringhelper info and logging it to blogpost.Slug in db
              
                blogPost.CreatedDate = DateTime.UtcNow;          

                _context.Add(blogPost);
                await _context.SaveChangesAsync();

				if (!string.IsNullOrEmpty(stringTags)) //blogpost needs to be saved first before we can add the tags, reason why this if statement is placed after savechanges async
				{
					await _blogService.AddTagsToBlogPostAsync(stringTags, blogPost.Id); // add the tags!
				}

				return RedirectToAction(nameof(Index));
            }
			ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
			return View(blogPost);
		}

        // GET: BlogPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            BlogPost? blogPost = await _context.BlogPosts
                                               .Include(b=>b.Tags)
                                               .FirstOrDefaultAsync(b=>b.Id == id);

            if (blogPost == null)
            {
                return NotFound();
            }

            List<string> tagNames = blogPost.Tags.Select(t=>t.Name!).ToList();
            ViewData["Tags"] = string.Join(", ", tagNames) + ", ";


            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Abstract,Content,Slug,Title,CreatedDate, UpdatedDate,IsDeleted,IsPublished,CategoryId,ImageFile,ImageData,ImageType")] BlogPost blogPost, string? stringTags)
        {		
			if (id != blogPost.Id) //security purposes, works with ValidatAntiForgeryToken, when form submitted the blogpostId submitted must match id in db
            {
                return NotFound();
            }

			ModelState.Remove("Slug");

			if (ModelState.IsValid)
            {
				
				try
                {
					if (!await _blogService.ValidSlugAsync(blogPost.Title, blogPost.Id))
					{
						ModelState.AddModelError("Title", "A similar Title/Slug is already in use.");

                        ViewData["Tags"] = stringTags != null && stringTags.Trim().EndsWith(",") ? stringTags : stringTags + ", ";
						ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
						return View(blogPost);
					}

					blogPost.Slug = StringHelper.BlogPostSlug(blogPost.Title);

					if (blogPost.ImageFile != null)
					{
						blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.ImageFile);
						blogPost.ImageType = blogPost.ImageFile.ContentType;
					}

					blogPost.UpdatedDate = DateTime.UtcNow;
					blogPost.CreatedDate = DateTime.SpecifyKind(blogPost.CreatedDate, DateTimeKind.Utc); //takes created date and converts it back too utc since submitted form changes the format

					_context.Update(blogPost); //we need this here because the info came from the form not from the db
					await _context.SaveChangesAsync();

                    //TAG TIME
					await _blogService.RemoveAllBlogPostTagsAsync(blogPost.Id);
					if (!string.IsNullOrEmpty(stringTags)) //blogpost needs to be saved first before we can add the tags, reason why this if statement is placed after savechanges async
					{
						await _blogService.AddTagsToBlogPostAsync(stringTags, blogPost.Id); // add the tags!
					}								
				}
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { slug = blogPost.Slug});
            }

			ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);

            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BlogPosts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BlogPosts'  is null.");
            }
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost != null)
            {
                _context.BlogPosts.Remove(blogPost);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> LikeBlogPost(int blogPostId, string blogUserId)
        {
            //check if user has already liked the blog
            //get the user
            BlogUser? blogUser = await _context.Users
                                                .Include(u => u.BlogLikes)
                                                .FirstOrDefaultAsync(u => u.Id == blogUserId);
            bool result = false;
            BlogLike? blogLike = new();

            if (blogUser != null)
            {
                if (!blogUser.BlogLikes.Any(bl=>bl.BlogPostId == blogPostId))
                {
                    blogLike = new BlogLike()
                    {
                        BlogPostId = blogPostId,
                        IsLiked = true
                    };

                    blogUser.BlogLikes.Add(blogLike);
                }
                //if a like already exists on this blogpost for this user
                else
                {
                    blogLike = await _context.BlogLikes.FirstOrDefaultAsync(bl=>bl.BlogPostId == blogPostId && bl.BlogUserId == blogUserId);

                    //modify the isliked to the inverse of its current state (t/f)
                    blogLike!.IsLiked = !blogLike.IsLiked;
                }
                result = blogLike.IsLiked;
                await _context.SaveChangesAsync();
            }
            return Json(new
            {
                isLiked = result,
                count = _context.BlogLikes.Where(bl => bl.BlogPostId == blogPostId && bl.IsLiked == true).Count()
            }); ;
        }

        private bool BlogPostExists(int id)
        {
            return (_context.BlogPosts?.Any(e => e.Id == id)).GetValueOrDefault();
        }

	}
}
