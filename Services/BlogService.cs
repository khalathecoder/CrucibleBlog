using CrucibleBlog.Data;
using CrucibleBlog.Helpers;
using CrucibleBlog.Models;
using CrucibleBlog.Services.Interfaces;
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


		public Task AddTagsToBlogPost(IEnumerable<int>? tagIds, int? blogPostId)
		{
			throw new NotImplementedException();
		}

		public Task AddTagsToBlogPost(IEnumerable<string>? tags, int? blogPostId)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync(int? count)
		{
			throw new NotImplementedException();
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

		public Task RemoveAllBlogPostTagsAsync(int? blogPostId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<BlogPost> SearchBlogPosts(string? searchString)
		{
			throw new NotImplementedException();
		}

		public async Task<bool> ValidSlugAsync(string? title, int blogPostId)
		{
			try
			{

				string? newSlug = StringHelper.BlogPostSlug(title); //because StringHelper is static, do not need to instantiate StringHelper, can use dot notation as is without instantiation "static is the blueprint and the house"

				if (blogPostId == null)
				{
					//Creating BlogPost
					return !await _context.BlogPosts.AnyAsync(b=>b.Slug == newSlug); //check if any slug in db is == to slug that i just created
				} 
				else
				{
					//Editing BlogPost
					BlogPost? blogPost = await _context.BlogPosts.AsNoTracking().FirstOrDefaultAsync(b=>b.Id == blogPostId); //Asnotracking: keep track of data but dont let entity framework know what I am doing
					string? oldSlug = blogPost?.Slug; //hold that info above here

					if(string.Equals(oldSlug, newSlug)) //compare oldSlug to newSlug
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
	}
}
