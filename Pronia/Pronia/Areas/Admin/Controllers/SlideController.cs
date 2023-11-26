using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.Admin.ViewModels;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilities.Extensions;
using System.Numerics;

namespace Pronia.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class SlideController : Controller
	{
		private readonly AppDbContext _context;
		private readonly IWebHostEnvironment _env;
        public SlideController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
			_env = env;
        }
        public async Task<IActionResult> Index()
		{
			List<Slide> slides=await _context.Slides.ToListAsync();
			return View(slides);
		}
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(CreateSlideVM slideVM)
		{
			

			if (!ModelState.IsValid)
			{
				return View();
			}
			if (!slideVM.Photo.ValidateType("images/")) 
			{
				ModelState.AddModelError("Photo", "The file type is not compatible.");
				return View();
			}
			if(slideVM.Photo.ValidateSize(2 * 1024))
			{
				ModelState.AddModelError("Photo", "File size should not exceed 2 megabytes.");
				return View();
			}

            string fileName =await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images","slider");
			Slide slide = new Slide()
			{
				Image = fileName,
				Title = slideVM.Title,
				SubTitle = slideVM.SubTitle,
				Description = slideVM.Description,
				Order = slideVM.Order,
				
			};
			await _context.AddAsync(slideVM);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> Update(int id)
		{
			if(id<=0) return BadRequest();
			Slide existed =await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
			if(existed is null) return NotFound();

			UpdateSlideVM slideVM = new UpdateSlideVM()
			{
				Title = existed.Title,
				SubTitle = existed.SubTitle,
				Description = existed.Description,
				Image = existed.Image,
				Order = existed.Order,
			};
			return View(slideVM);
		}
		[HttpPost]

		public async Task<IActionResult> Update(int id,UpdateSlideVM slideVM)
		{
			
			if(!ModelState.IsValid)
			{
				return View(slideVM);
			}
            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null) return NotFound();
            if (slideVM.Photo is not null)
			{
				if (!slideVM.Photo.ValidateType("images/"))
				{
					ModelState.AddModelError("Photo", "The file type is not compatible.");
					return View(slideVM);
				}
				if (slideVM.Photo.ValidateSize(2 * 1024))
				{
					ModelState.AddModelError("Photo", "File size should not exceed 2 megabytes.");
					return View(slideVM);
				}
				string fileName = await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "slider");
				existed.Image.DeleteFile(_env.WebRootPath,"assets","images","slider");
				existed.Image = fileName;
			}
			existed.Title = slideVM.Title;
			existed.SubTitle = slideVM.SubTitle;
			existed.Description = slideVM.Description;
			existed.Order = slideVM.Order;
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int id)
		{
			if (id <= 0) return BadRequest();
			Slide existed=await _context.Slides.FirstOrDefaultAsync(s=>s.Id==id);
			if(existed == null) return NotFound();

			existed.Image.DeleteFile(_env.WebRootPath,"assets","images","slider");
			_context.Slides.Remove(existed);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}


        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return BadRequest();

            Slide detail = await _context.Slides
				.FirstOrDefaultAsync(s => s.Id == id);
            if (detail == null) NotFound();

            return View(detail);

        }
    }
}
