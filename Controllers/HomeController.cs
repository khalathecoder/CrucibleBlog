using System.Diagnostics;
using CrucibleBlog.Data;
using CrucibleBlog.Models;
using CrucibleBlog.Services;
using CrucibleBlog.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IBlogService _blogService;
        private readonly IEmailSender _emailService;
        private readonly IConfiguration _configuration;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<BlogUser> userManager, IImageService imageService, IBlogService blogService, IEmailSender emailService, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _blogService = blogService;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int? pageNum)
        {
            int pageSize = 4;
            int page = pageNum ?? 1;  //if pageNum is null, set page to 1

            IPagedList<BlogPost> blogPosts = await _context.BlogPosts.Include(b => b.Category).ToPagedListAsync(pageNum, pageSize);

            ViewData["ActionName"] = "Index";

            return View(blogPosts);
        }

        public async Task<IActionResult> SearchIndex(string? searchString, int? pageNum)
        {
            int pageSize = 4;
            int page = pageNum ?? 1;  //if pageNum is null, set page to 1

            IPagedList<BlogPost> blogPosts = await _blogService.SearchBlogPosts(searchString).ToPagedListAsync(page, pageSize);

            ViewData["ActionName"] = "SearchIndex";

            return View(nameof(Index), blogPosts);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ContactMe()
        {
            string? blogUserId = _userManager.GetUserId(User);

            if (blogUserId == null)
            {
                return NotFound();
            }

            BlogUser? blogUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == blogUserId);
            return View(blogUser);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactMe([Bind("FirstName,LastName,Email")] BlogUser blogUser, string? message)
        {
            string? swalMessage = string.Empty;

            if (ModelState.IsValid)
            {
                try
                {
                    string? adminEmail = _configuration["AdminLoginEmail"] ?? Environment.GetEnvironmentVariable("AdminLoginEmail");
                    await _emailService.SendEmailAsync(adminEmail!, $"Contact Me Message From - {blogUser.FullName}", message!);
                    swalMessage = "Email sent successfully!";
                }
                catch (Exception)
                {

                    throw;
                }

                swalMessage = "Error: Unable to Send email.";
            }
            return RedirectToAction("Index", new { swalMessage });
        }

        public IActionResult Privacy()
        {
            return View();
        }

		public async Task<IActionResult> Favorites(int? pageNum)
		{
            int pageSize = 4;
            int page = pageNum ?? 1;  //if pageNum is null, set page to 1

            IPagedList<BlogPost> blogPosts = await _context.BlogPosts.Include(b => b.Category).ToPagedListAsync(pageNum, pageSize);

            ViewData["ActionName"] = "Index";

            return View(blogPosts);
        }
		public IActionResult Recent()
		{
			return View();
		}
		public IActionResult Popular()
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