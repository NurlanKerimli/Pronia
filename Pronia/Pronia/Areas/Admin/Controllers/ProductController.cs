using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.Admin.ViewModels;

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
                .Include(p=>p.ProductTags)
                .ThenInclude(pt=>pt.Tag)
                .ToListAsync();
            return View();
        }

        public async Task<IActionResult> Create()
        {
            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Tags= await _context.Tags.ToListAsync();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            if(!ModelState.IsValid)
            {
				productVM.Categories = await _context.Categories.ToListAsync();
                return View();
            }
            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
				productVM.Categories = await _context.Categories.ToListAsync();
                ModelState.AddModelError("CategoryId", "This category does not exist");
                return View();
            }
            foreach(int tagId in productVM.TagIds)
            {
                bool tagResult= await _context.Tags.AnyAsync(t => t.Id == tagId);
                if (!tagResult)
                {
                    ModelState.AddModelError("TagIds", "Tag details don't exist.");
                    return View();
                }
            }
            Product product = new Product
            {
                Name = productVM.Name,
                Price = productVM.Price,
                Description = productVM.Description,
                SKU = productVM.SKU,
                CategoryId = (int)productVM.CategoryId,
                ProductTags=new List<ProductTag>()
            };
            List<ProductTag> productTags = new List<ProductTag>();
            foreach (int tagId in productVM.TagIds)
            {
                ProductTag productTag = new ProductTag()
                {
                    TagId = tagId,
                };
                product.ProductTags.Add(product);
            }
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            Product existed = await _context.Products.Include(p=>p.ProductTags).FirstOrDefaultAsync(p=> p.Id ==id);
            if (existed == null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM()
            {
                Name = existed.Name,
                Price = existed.Price,
                Description = existed.Description,
                SKU = existed.SKU,
                CategoryId = existed.CategoryId,
                TagIds=existed.ProductTags.Select(p=>p.TagId).ToList(),
                Categories = await _context.Categories.ToListAsync(),
                Tags=await _context.Tags.ToListAsync(),
                
            };
            return View(productVM);
        }

        public async Task<IActionResult> Update(int id,UpdateProductVM productVM)
        {
            if(!ModelState.IsValid)
            {
				productVM.Categories = await _context.Categories.ToListAsync();
                productVM.Tags=await _context.Tags.ToListAsync();
				return View(productVM);
            }
            Product existed= await _context.Products.Include(p=>p.ProductTags).FirstOrDefaultAsync(p=> p.Id == id);
            if (existed == null) return NotFound();
            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if(!result)
            {
                ModelState.AddModelError("CategoryId", "The categoryid is not exist.");
            }
            if(productVM.Photo is not null)
            {
				if (!productVM.Photo.ValidateType("images/"))
				{
					
					ModelState.AddModelError("Photo", "The file type is not compatible.");
					return View(productVM);
				}
				if (productVM.Photo.ValidateSize(2 * 1024))
				{
					
					ModelState.AddModelError("Photo", "File size should not exceed 2 megabytes.");
					return View(productVM);
				}
                string fileName = await productVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "web-images");
                existed.ProductImages.DeleteFile(_env.WebRootPath, "assets", "images", "web-images");
                existed.ProductImages = fileName;

            }

            foreach (ProductTag pTag in existed.ProductTags)
            {
                if (!productVM.TagIds.existed(tId => tId == pTag.TagId))
                {
                    _context.ProductTags.Remove(pTag);
                }
            }
            foreach(int tId in productVM.TagIds)
            {
                if (existed.ProductTags.Any(pt => pt.tagId == tId))
                {
                    existed.ProductTags.Add(new ProductTag { TagId = tId });
                }
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

			//existed.ProductImages.DeleteFile(_env.WebRootPath, "assets", "images", "web-images");
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
