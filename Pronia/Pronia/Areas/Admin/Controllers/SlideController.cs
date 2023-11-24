using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
		public async Task<IActionResult> Create(Slide slide)
		{
			if(slide.Photo is null)
			{
				ModelState.AddModelError("Photo", "Please enter a picture.");
				return View();
			}
			if (!slide.Photo.ValidateType("images/")) 
			{
				ModelState.AddModelError("Photo", "The file type is not compatible.");
				return View();
			}
			if(slide.Photo.ValidateSize(2 * 1024))
			{
				ModelState.AddModelError("Photo", "File size should not exceed 2 megabytes.");
				return View();
			}

			slide.Image =await slide.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images","slider");

			await _context.AddAsync(slide);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> Update(int id)
		{
			if(id<=0) return BadRequest();
			Slide existed =await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
			if(existed is null) return NotFound();

			return View(existed);
		}
		[HttpPost]

		public async Task<IActionResult> Update(int id,Slide slide)
		{
			Slide existed=await _context.Slides.FirstOrDefaultAsync(s=>s.Id == id);	
			if(existed is null) return NotFound();
			if(!ModelState.IsValid)
			{
				return View(existed);
			}
			if(slide.Photo is not null)
			{
				if (!slide.Photo.ValidateType("images/"))
				{
					ModelState.AddModelError("Photo", "The file type is not compatible.");
					return View(existed);
				}
				if (slide.Photo.ValidateSize(2 * 1024))
				{
					ModelState.AddModelError("Photo", "File size should not exceed 2 megabytes.");
					return View(existed);
				}
				string fileName = await slide.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "slider");
				existed.Image.DeleteFile(_env.WebRootPath,"assets","images","slider");
				existed.Image = fileName;
			}
			existed.Title = slide.Title;
			existed.SubTitle = slide.SubTitle;
			existed.Description = slide.Description;
			existed.Order = slide.Order;
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int id)
		{
			if (id <= 0) return BadRequest();
			Slide existed=await _context.Slides.FirstOrDefaultAsync(s=>s.Id==id);
			if(existed == null) return NotFound();

			existed.Image.DeleteFile(_env.WebRootPath,"assets","images","slides");
			_context.Slides.Remove(existed);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}


        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return BadRequest();

            Slide detail = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (detail == null) NotFound();

            return View(detail);

        }
    }
}
