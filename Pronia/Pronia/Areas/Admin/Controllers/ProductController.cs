using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.Admin.ViewModels;
using Pronia.Areas.Admin.ViewModels.Product;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilities.Extensions;
using System.Collections.Generic;

namespace Pronia.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public ProductController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async  Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                .ToListAsync();
            return View();
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            if(!ModelState.IsValid)
            {
				ViewBag.Categories = await _context.Categories.ToListAsync();
                return View();
            }
            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
				ViewBag.Categories = await _context.Categories.ToListAsync();
                ModelState.AddModelError("CategoryId", "This category does not exist");
                return View();
            }
            Product product = new Product
            {
                Name = productVM.Name,
                Price = productVM.Price,
                Description = productVM.Description,
                SKU = productVM.SKU,
                CategoryId = (int)productVM.CategoryId,
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            Product existed = await _context.Products.FirstOrDefaultAsync(p=> p.Id ==id);
            if (existed == null) return NotFound();

            UpdateProductVM slideVM = new UpdateProductVM()
            {
                Name=existed.Name,
                Price=existed.Price,
                Description=existed.Description,
                SKU=existed.SKU,

                
            };
            return View(slideVM);
        }

        public async Task<IActionResult> Update(int id,UpdateProductVM productVM)
        {
            if(!ModelState.IsValid)
            {
				ViewBag.Categories = await _context.Categories.ToListAsync();

				return View(productVM);
            }
            Product existed= await _context.Products.FirstOrDefaultAsync(p=> p.Id == id);
            if (existed == null) return NotFound();
            if(productVM.Photo is not null)
            {
				if (!productVM.Photo.ValidateType("images/"))
				{
					ViewBag.Categories = await _context.Categories.ToListAsync();
					ModelState.AddModelError("Photo", "The file type is not compatible.");
					return View(productVM);
				}
				if (productVM.Photo.ValidateSize(2 * 1024))
				{
					ViewBag.Categories = await _context.Categories.ToListAsync();
					ModelState.AddModelError("Photo", "File size should not exceed 2 megabytes.");
					return View(productVM);
				}
                string fileName = await productVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "web-images");
				existed.ProductImages.DeleteFile(_env.WebRootPath, "assets", "images", "web-images");
				existed.ProductImages = fileName;

			}
            existed.Name = productVM.Name;
            existed.Price = productVM.Price;
            existed.Description = productVM.Description;
            existed.SKU = productVM.SKU;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            Product existed=await _context.Products.FirstOrDefaultAsync(p=>p.Id==id);
            if (existed==null) return NotFound();

			existed.ProductImages.DeleteFile(_env.WebRootPath, "assets", "images", "web-images");
			_context.Products.Remove(existed);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Detail(int id)
		{
			if (id <= 0) return BadRequest();

			Product detail = await _context.Products
                .FirstOrDefaultAsync(s => s.Id == id);
			if (detail == null) NotFound();

			return View(detail);

		}
	}
}
