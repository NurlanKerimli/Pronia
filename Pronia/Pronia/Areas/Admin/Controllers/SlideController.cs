using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using System.Numerics;

namespace Pronia.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class SlideController : Controller
	{
		private readonly AppDbContext _context;
        public SlideController(AppDbContext context)
        {
            _context = context;
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
		public async Task<IActionResult> Create(Slide slide)
		{
			if(slide.Photo is null)
			{
				ModelState.AddModelError("Photo", "Please enter a picture.");
				return View();
			}
			if(!slide.Photo.ContentType.Contains("images/"))
			{
				ModelState.AddModelError("Photo", "The file type is not compatible.");
			}
			if(slide.Photo.Length > 2 * 1024 * 1024)
			{
				ModelState.AddModelError("Photo", "File size should not exceed 2 megabytes.");
			}
			FileStream file = new FileStream(@"D:\Users\nurlan\Documents\CodeAcademy resources\Tasks\Task -27\Pronia\Pronia\wwwroot\assets\images\slider\"+slide.Photo.FileName,FileMode.Create);
			await slide.Photo.CopyToAsync(file);
			slide.Image = slide.Photo.FileName;

			await _context.AddAsync(slide);
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
