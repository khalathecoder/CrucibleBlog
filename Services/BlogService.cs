using System.ComponentModel;
using CrucibleBlog.Data;
using CrucibleBlog.Helpers;
using CrucibleBlog.Models;
using CrucibleBlog.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;

namespace CrucibleBlog.Services
{
	public class BlogService : IBlogService
	{
		private readonly ApplicationDbContext _context;

		public BlogService(ApplicationDbContext context)
		{
			_context = context;

		}

		
		public Task AddTagsToBlogPostAsync(IEnumerable<int>? tagIds, int? blogPostId)
		{
			throw new NotImplementedException();
		}

		public async Task AddTagsToBlogPostAsync(string tagNames, int? blogPostId)
		{
			try
			{
				BlogPost? blogPost = await _context.BlogPosts.FirstOrDefaultAsync(b=>b.Id == blogPostId);
				if (blogPost == null) //if blogpost doesnt exist, dont bother doing any more work
				{
					return; //break is similar but specifically for loops; return is for a method
				}

				//string of tag names and split it into more strings
				// tagNames = "C#", ".Net", "Crucible"
				// tags = ["C#", ".Net", "Crucible"]
				List<string> tags = tagNames.Split(',').DistinctBy(s => s.Trim()).ToList(); //returns array without commas; distantby allows us to ensure there are no duplicate tags

				// foreach loop through the tags
				foreach (string? tagName in tags)
				{
					if (string.IsNullOrWhiteSpace(tagName)) continue; //continue goes to next item in loop

                    // check to see if we already have tag in db
                    // normalize tag name
                    Tag? tag = await _context.Tags.FirstOrDefaultAsync(b => b.Name!.Trim().ToLower() == tagName.Trim().ToLower());

                    // if no tag in db, create a new one
					if(tag == null)
					{
						tag = new Tag()
						{
							Name = tagName.Trim(),
						};

						_context.Tags.Add(tag);
					}
                    // either way, add the tag to the blog post
					blogPost.Tags.Add(tag);                   
                }
                // save to the db
                await _context.SaveChangesAsync();

            }
			catch (Exception) //handle exceptions as they arise
			{

				throw;
			}
		}

		public async Task RemoveAllBlogPostTagsAsync(int? blogPostId)
		{
			try
			{
				BlogPost? blogPost = await _context.BlogPosts
													.Include(c => c.Tags)
													.FirstOrDefaultAsync(c => c.Id == blogPostId);
				if(blogPost == null) return;

				blogPost!.Tags.Clear();

				//_context.Update(blogPost); //notifies entity fw that we made changes to model but doesnt yet save
				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<IEnumerable<Category>> GetCategoriesAsync()
		{
			try
			{
				IEnumerable<Category> categories = await _context.Categories.ToListAsync();
				return categories;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync()
		{
			throw new NotImplementedException();
		}

		public async Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync(int? count)
		{
			try
			{ IEnumerable<BlogPost> blogPosts = await _context.BlogPosts
															.Where(b=>b.IsDeleted == false && b.IsPublished == true)
															.Include(b=>b.Category)
															.Include(b => b.Comments)
																.ThenInclude(c=>c.Author)
															.Include(b => b.Tags)
															.ToListAsync();

				return blogPosts.OrderByDescending(b => b.Comments.Count).Take(count!.Value);
			}
			catch (Exception)
			{

				throw;
			}
		}

		public Task<IEnumerable<BlogPost>> GetRecentBlogPostsAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<BlogPost>> GetRecentBlogPostsAsync(int? count)
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsTagOnBlogPostAsync(int? tagId, int? blogPostId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<BlogPost> SearchBlogPosts(string? searchString)
		{
			try
			{
				//Intermediate variable
				IEnumerable<BlogPost> blogPosts = new List<BlogPost>(); //doing this because cannot instantiate an Ienumerable; list can be instantiated instead

				if (string.IsNullOrEmpty(searchString))
				{
					return blogPosts; //if null or empty, return empty list of blogs
				}
				else
				{
					//normalize search text (best practice)
					searchString = searchString.Trim().ToLower();

					//load blogPosts from data from the db
					//where clause can find multiple queries as opposed to Find() or Firstordefault which only looks for 1 item in db
					blogPosts = _context.BlogPosts
												.Where(b => b.Title!.ToLower().Contains(searchString) ||
														b.Abstract!.ToLower().Contains(searchString) ||
														b.Content!.ToLower().Contains(searchString) ||
														b.Category!.Name!.ToLower().Contains(searchString) ||
														b.Comments.Any(c => c.Body!.ToLower().Contains(searchString) || //new predicate delgate because Comments has its own list of items
																	c.Author!.FirstName!.ToLower().Contains(searchString) ||
																	c.Author!.LastName!.ToLower().Contains(searchString)) ||
														b.Tags.Any(t => t.Name!.ToLower().Contains(searchString)))
												.Include(b => b.Comments)
													.ThenInclude(c => c.Author)
												.Include(b => b.Category)
												.Include(b => b.Tags)
												.Where(b => b.IsDeleted == false && b.IsPublished == true)
												.AsNoTracking() //takes snapchat of db as it is
												.OrderByDescending(b => b.CreatedDate)
												.AsEnumerable();


                    return blogPosts;
				}
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<bool> ValidSlugAsync(string? title, int blogPostId)
		{
			try
			{

				string? newSlug = StringHelper.BlogPostSlug(title); //because StringHelper is static, do not need to instantiate StringHelper, can use dot notation as is without instantiation "static is the blueprint and the house"

				if (blogPostId == null || blogPostId == 0)
				{
					//Creating BlogPost
					return !await _context.BlogPosts.AnyAsync(b=>b.Slug == newSlug); //check if any slug in db is == to slug that i just created
				} 
				else
				{
					//Editing BlogPost
					BlogPost? blogPost = await _context.BlogPosts.AsNoTracking().FirstOrDefaultAsync(b=>b.Id == blogPostId); //Asnotracking: keep track of data but dont let entity framework know what I am doing
					string? oldSlug = blogPost?.Slug; //hold that info above here

					if(!string.Equals(oldSlug, newSlug)) //compare oldSlug to newSlug
					{
						return await _context.BlogPosts.AnyAsync(b => b.Id != blogPost!.Id && b.Slug == newSlug); //check all other blogposts for this particular slug
					}

				}

				return true;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<List<Tag>> GetTagsAsync()
		{

			return await _context.Tags.ToListAsync();
		}

		public async Task<bool> UserLikedBlogAsync(int blogPostId, string blogUserId)
		{
			try
			{
				return await _context.BlogLikes
									 .AnyAsync(bl=>bl.BlogPostId == blogPostId && bl.IsLiked == true && bl.BlogUserId == blogUserId);
			}
			catch (Exception)
			{

				throw;
			}
		}

        public async Task<IEnumerable<BlogPost>> GetFavoriteBlogPostsAsync(string? blogUserId)
        {
            try
            {
                List<BlogPost> blogPosts = new();
                if (!string.IsNullOrEmpty(blogUserId))
                {
                    BlogUser? blogUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == blogUserId);
                    if (blogUser != null)
                    {
                        //List<int> blogPostIds = _context.BlogLikes.Where(bl => bl.BlogUserId == blogUserId && bl.IsLiked == true).Select(b => b.BlogPostId).ToList();
                        blogPosts = await _context.BlogPosts.Where(b => b.Likes.Any(l => l.BlogUserId == blogUserId && l.IsLiked == true) &&
                                                                                    b.IsPublished == true &&
                                                                                    b.IsDeleted == false)
                                                            .Include(b => b.Likes)
                                                            .Include(b => b.Comments)
                                                            .Include(b => b.Category)
                                                            .Include(b => b.Tags)
                                                            .OrderByDescending(b => b.CreatedDate)
                                                            .ToListAsync();
                    }
                }
                return blogPosts;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
