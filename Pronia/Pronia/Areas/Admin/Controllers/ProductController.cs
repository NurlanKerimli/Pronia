﻿using Microsoft.AspNetCore.Mvc;
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
            CreateProductVM productVM = new CreateProductVM();
           

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
                productVM.Tags = await _context.Tags.ToListAsync();
                return View(productVM);
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
			if (productVM.CategoryId == 0)
			{
				ModelState.AddModelError("AuthorId", "You must choose Author");
				productVM.Categories = await _context.Categories.ToListAsync();
				productVM.Tags = await _context.Tags.ToListAsync();

				return View(productVM);
			}


			if (await _context.Categories.FirstOrDefaultAsync(x => x.Id == productVM.CategoryId) is null)
			{
				ModelState.AddModelError("AuthorId", "This Author is not exist");
				productVM.Categories = await _context.Categories.ToListAsync();
				productVM.Tags = await _context.Tags.ToListAsync();

				return View(productVM);
			}
            if (productVM.MainPhoto.ValidateType("image/"))
            {
                productVM.Categories=await _context.Categories.ToListAsync();
                productVM.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("MainPhoto", "Invalid file type.");
				return View(productVM);
            }

            if (!productVM.MainPhoto.ValidateSize(600))
            {
				productVM.Categories = await _context.Categories.ToListAsync();
				productVM.Tags = await _context.Tags.ToListAsync();
				ModelState.AddModelError("MainPhoto", "The file size is not appropriate");
			}

			if (productVM.MainPhoto.ValidateType("image/"))
			{
				productVM.Categories = await _context.Categories.ToListAsync();
				productVM.Tags = await _context.Tags.ToListAsync();
				ModelState.AddModelError("HoverPhoto", "Invalid file type.");
				return View(productVM);
			}

			if (!productVM.MainPhoto.ValidateSize(600))
			{
				productVM.Categories = await _context.Categories.ToListAsync();
				productVM.Tags = await _context.Tags.ToListAsync();
				ModelState.AddModelError("HoverPhoto", "The file size is not appropriate");
			}

            ProductImage main = new ProductImage()
            {
                IsPrimary = true,
                Url=await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath,"assets","images","website-images"),
                Alternative=productVM.Name
            };

			ProductImage hover = new ProductImage()
			{
				IsPrimary = false,
				Url = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
				Alternative = productVM.Name
			};
			Product product = new Product
            {
                Name = productVM.Name,
                Price = productVM.Price,
                Description = productVM.Description,
                SKU = productVM.SKU,
                CategoryId = (int)productVM.CategoryId,
                ProductTags=new List<ProductTag>(),
                ProductImages=new List<ProductImage> { main,hover }
            };

            TempData["Message"] = "";
            foreach (IFormFile photo in productVM.Photos ?? new List<IFormFile>())
            {
                if (!photo.ValidateType("image/"))
                {
                    TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file type does not match</p>";
                    continue;
                }
                if (!productVM.HoverPhoto.ValidateSize(600))
                {
					TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file size is not appropriate</p>";
					continue;
                }

                product.ProductImages.Add(new ProductImage
                {
                    IsPrimary = null,
                    Alternative = product.Name,
                    Url = await photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
                });
            }
			if (productVM.TagIds is not null)
			{
				foreach (var item in productVM.TagIds)
				{
					if (!await _context.Tags.AnyAsync(x => x.Id == item))
					{
						ModelState.AddModelError("TagIds", "This tag is not exist");
						productVM.Categories = await _context.Categories.ToListAsync(); //Müəllim nəyə görə biz burda eyni vaxtda həm categories hem de tagı yoxlayırıq.// 
						productVM.Tags = await _context.Tags.ToListAsync();

						return View(productVM);
					}
				}

				foreach (var item in productVM.TagIds)
				{
					product.ProductTags.Add(new ProductTag { TagId = item });
				}
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

            existed.ProductTags.RemoveAll(pt=>!productVM.TagIds.Exists(tId=>tId==pt.TagId));

            List<int> creatable=productVM.TagIds.Where(tId=>!existed.ProductTags.Exists(pt=>pt.TagId==tId)).ToList();
            foreach (int tId in creatable)
            {
                bool tagResult = await _context.Tags.AnyAsync(t => t.Id == tId);
                if (!tagResult)
                {
					productVM.Categories = await _context.Categories.ToListAsync();
					productVM.Tags = await _context.Tags.ToListAsync();
                    ModelState.AddModelError("TagIds", "The Tag is not exist.");

					return View(productVM);
                }
                existed.ProductTags.Add(new ProductTag { TagId = tId });
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
                .Include(p=>p.Category)
                .Include(p=>p.ProductImages)
                .Include(p=>p.ProductTags)
                .ThenInclude(pt=>pt.Tag)
                .FirstOrDefaultAsync(s => s.Id == id);
			if (detail == null) NotFound();

			return View(detail);

		}
	}
}
