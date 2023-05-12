using System.Diagnostics;
using CrucibleBlog.Data;
using CrucibleBlog.Models;
using CrucibleBlog.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace CrucibleBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly UserManager<BlogUser> _userManager;
		private readonly IImageService _imageService;


		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<BlogUser> userManager, IImageService imageService)
        {
            _logger = logger;
            _context = context;
			_userManager = userManager;
			_imageService = imageService;
		}
        
        public async Task<IActionResult> Index(int? pageNum)
        {
            int pageSize = 4;
            int page = pageNum ?? 1;  //if pageNum is null, set page to 1

            IPagedList<BlogPost> blogPosts = await _context.BlogPosts.Include(b => b.Category).ToPagedListAsync(pageNum, pageSize);


			return View(blogPosts);
		}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}