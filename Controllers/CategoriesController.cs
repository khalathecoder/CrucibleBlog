﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CrucibleBlog.Data;
using CrucibleBlog.Models;
using X.PagedList;
using CrucibleBlog.Services.Interfaces;

namespace CrucibleBlog.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;

        public CategoriesController(ApplicationDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
			//int pageSize = 4;
			//int page = pageNum ?? 1;

			//BlogPost blogPosts = await _context.BlogPosts.Include(b => b.Category).ToListAsync();

			//ViewData["ActionName"] = "Details";

			//return View(blogPosts);

            return _context.Categories != null ?
                                   View(await _context.Categories.ToListAsync()) :
                                   Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id, int? pageNum)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            Category? category = await _context.Categories
                                                .Include(c=>c.BlogPosts)                                                 
                                                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

			int pageSize = 4;
			int page = pageNum ?? 1;  //if pageNum is null, set page to 1

			IPagedList<BlogPost> blogPosts = await (category.BlogPosts)
                                                                    .ToPagedListAsync(page, pageSize);

			return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageData,ImageType")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.ImageFile != null)
                {
                    category.ImageData = await _imageService.ConvertFileToByteArrayAsync(category.ImageFile);
                    category.ImageType = category.ImageFile.ContentType;
                }

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

           
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageData,ImageType")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (category.ImageFile != null)
                    {
                        category.ImageData = await _imageService.ConvertFileToByteArrayAsync(category.ImageFile);
                        category.ImageType = category.ImageFile.ContentType;
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
          return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
