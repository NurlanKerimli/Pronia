using Microsoft.AspNetCore.Authorization;
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
    [AutoValidateAntiforgeryToken]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public ProductController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
		[Authorize(Roles = "Admin,Moderator")]
		public async  Task<IActionResult> Index(int page=1)
        {
            int count = await _context.Products.CountAsync();
            ViewBag.TotalPage = Math.Ceiling((double)count / 3);
            ViewBag.CurrentPage = page;
            List<Product> products = await _context.Products.Skip((page-1)*3).Take(3)
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                .Include(p=>p.ProductTags)
                .ThenInclude(pt=>pt.Tag)
                .ToListAsync();
            PaginateVM<Product> paginateVM = new PaginateVM<Product>
            {
                Items = products,
                TotalPage = Math.Ceiling((double)count / 3),
                CurrentPage = page,
            };
            return View(paginateVM);
        }
		[Authorize(Roles = "Admin,Moderator")]
		public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM();
           

            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Tags= await _context.Tags.ToListAsync();
            return View(productVM);
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
				
                ModelState.AddModelError("CategoryId", "This category does not exist");
				productVM.Categories = await _context.Categories.ToListAsync();
				productVM.Tags = await _context.Tags.ToListAsync();

				return View(productVM);
			}
            foreach(int tagId in productVM.TagIds)
            {
                bool tagResult= await _context.Tags.AnyAsync(t => t.Id == tagId);
                if (!tagResult)
                {
                    ModelState.AddModelError("TagIds", "Tag details don't exist.");
					productVM.Categories = await _context.Categories.ToListAsync();
					productVM.Tags = await _context.Tags.ToListAsync();

					return View(productVM);
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
            if (!productVM.MainPhoto.ValidateType("image/"))
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

			if (!productVM.HoverPhoto.ValidateType("image/"))
			{
				productVM.Categories = await _context.Categories.ToListAsync();
				productVM.Tags = await _context.Tags.ToListAsync();
				ModelState.AddModelError("HoverPhoto", "Invalid file type.");
				return View(productVM);
			}

			if (!productVM.HoverPhoto.ValidateSize(600))
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
				Url = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
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
                    Alternative = productVM.Name,
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
						productVM.Categories = await _context.Categories.ToListAsync(); 
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
		[Authorize(Roles = "Admin,Moderator")]
		public async Task<IActionResult> Update(int id)
        {
            Product existed = await _context.Products.Include(p=>p.ProductImages).Include(p=>p.ProductTags).FirstOrDefaultAsync(p=> p.Id ==id);
            if (existed == null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM()
            {
                Name = existed.Name,
                Price = existed.Price,
                Description = existed.Description,
                SKU = existed.SKU,
                CategoryId = existed.CategoryId,
                ProductImages = existed.ProductImages,
                TagIds=existed.ProductTags.Select(p=>p.TagId).ToList(),
                Categories = await _context.Categories.ToListAsync(),
                Tags=await _context.Tags.ToListAsync(),
                
            };
            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id,UpdateProductVM productVM)
        {
			Product existed = await _context.Products.Include(p=>p.ProductImages).Include(p => p.ProductTags).FirstOrDefaultAsync(p => p.Id == id);

			productVM.Categories = await _context.Categories.ToListAsync();
			productVM.Tags = await _context.Tags.ToListAsync();
			productVM.ProductImages = existed.ProductImages;
			if (!ModelState.IsValid)
            {
				return View(productVM);
            }
            if (existed == null) return NotFound();
            if (productVM.MainPhoto is not null)
            {
				if (!productVM.MainPhoto.ValidateType("image/"))
				{
					ModelState.AddModelError("MainPhoto", "Invalid file type.");
					return View(productVM);
				}

				if (!productVM.MainPhoto.ValidateSize(600))
				{
					ModelState.AddModelError("MainPhoto", "The file size is not appropriate");
                    return View(productVM);
                }
			}
			if (productVM.HoverPhoto is not null)
			{
				if (!productVM.HoverPhoto.ValidateType("image/"))
				{
					ModelState.AddModelError("HoverPhoto", "Invalid file type.");
					return View(productVM);
				}

				if (!productVM.HoverPhoto.ValidateSize(600))
				{
					ModelState.AddModelError("HoverPhoto", "The file size is not appropriate");
					return View(productVM);
				}
			}
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
                    ModelState.AddModelError("TagIds", "The Tag is not exist.");

					return View(productVM);
                }
                existed.ProductTags.Add(new ProductTag { TagId = tId });
            }
            if(productVM.MainPhoto != null)
            {
                string fileName = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage existedImg=existed.ProductImages.FirstOrDefault(pi=>pi.IsPrimary==true);
                existedImg.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(existedImg);

                existed.ProductImages.Add(new ProductImage
                {
                    IsPrimary = true,
                    Alternative = productVM.Name,
                    Url = fileName
                });
            }

            if(productVM.HoverPhoto != null)
            {
                string fileName = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage existedImg=existed.ProductImages.FirstOrDefault(pi=>pi.IsPrimary==false);
                existedImg.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(existedImg);
                existed.ProductImages.Add(new ProductImage
                {
                    IsPrimary=false,
                    Alternative = productVM.Name,
                    Url = fileName
                });
            }
            if(productVM.TagIds is null)
            {
                productVM.ImageIds = new List<int>();
            }
            List<ProductImage> removeable = existed.ProductImages.Where(pi => !productVM.ImageIds.Exists(imgId => imgId==pi.Id)&&pi.IsPrimary==null).ToList();
            foreach(ProductImage removedImg in removeable)
            {
                removedImg.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(removedImg);
            }
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

				existed.ProductImages.Add(new ProductImage
				{
					IsPrimary = null,
					Alternative = productVM.Name,
					Url = await photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
				});
			}
			existed.Name = productVM.Name;
            existed.Price = productVM.Price;
            existed.Description = productVM.Description;
            existed.SKU = productVM.SKU;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Product existed=await _context.Products.Include(p=>p.ProductImages).FirstOrDefaultAsync(p=>p.Id==id);
            if (existed==null) return NotFound();
            foreach (ProductImage image in existed.ProductImages ?? new List<ProductImage>())
            {
                image.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
            }
            
			_context.Products.Remove(existed);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public  async Task<IActionResult> Detail(int id)
		{
			if (id <= 0) return BadRequest();

			Product detail = await _context.Products
                .Include(p=>p.Category)
                .Include(p=>p.ProductImages)
                .Include(p=>p.ProductColor)
                .ThenInclude(pc=>pc.Color)
                .Include(s=>s.ProductSize)
                .ThenInclude(ps=>ps.Size)
                .Include(p=>p.ProductTags)
                .ThenInclude(pt=>pt.Tag)
                .FirstOrDefaultAsync(s => s.Id == id);
			if (detail == null) return NotFound();

			return View(detail);

		}
	}
}
