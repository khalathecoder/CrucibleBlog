using CrucibleBlog.Models;

namespace CrucibleBlog.Services.Interfaces
{
    public interface IBlogService
    {
		//TODO: BlogPost CRUD
		//TODO: Comment CRUD
		//TODO: Category CRUD
		//TODO: Tag CRUD

		public Task<IEnumerable<Category>> GetCategoriesAsync();
		public Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync();
		public Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync(int? count);
		public Task<IEnumerable<BlogPost>> GetRecentBlogPostsAsync();
		public Task<IEnumerable<BlogPost>> GetRecentBlogPostsAsync(int? count);

		public Task AddTagsToBlogPostAsync(IEnumerable<int>? tagIds, int? blogPostId); //passing tagids to blogpost by id
		public Task AddTagsToBlogPostAsync(string tagNames, int? blogPostId); //passing tag string to blogpost

		public Task<bool> IsTagOnBlogPostAsync(int? tagId, int? blogPostId); //pass bool back if this tagid is on this blogpost by id
		public Task RemoveAllBlogPostTagsAsync(int? blogPostId);

		public IEnumerable<BlogPost> SearchBlogPosts(string? searchString);
		public Task<bool> ValidSlugAsync(string? title, int blogPostId); //slug is readable url for user

		public Task<List<Tag>> GetTagsAsync();

		public Task<bool> UserLikedBlogAsync(int blogPostId, string blogUserId);

		public Task<IEnumerable<BlogPost>> GetFavoriteBlogPostsAsync(string? blogUserId);

    }
}
