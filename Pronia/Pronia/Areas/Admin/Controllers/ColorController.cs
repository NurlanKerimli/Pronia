using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.Admin.ViewModels;
using Pronia.DAL;
using Pronia.Models;

namespace Pronia.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ColorController : Controller
	{

		private readonly AppDbContext _context;

		public ColorController(AppDbContext context)
		{
			_context = context;
		}
		public async Task<IActionResult> Index()
		{
			List<Color> colors = await _context.Colors.ToListAsync();
			return View(colors);
		}
		public async Task<IActionResult> Create()
		{
			ViewBag.Colors = await _context.Colors.ToListAsync();
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(CreateColorVM colorVM)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Colors = await _context.Colors.ToListAsync();
				return View();
			}
			bool result = _context.Colors.Any(c => c.Name.Trim() == colorVM.Name.Trim());
			if (result)
			{
				ViewBag.Colors = await _context.Colors.ToListAsync();
				ModelState.AddModelError("Name", "This name is already available.");
				return View();
			}
			Color color = new Color()
			{
				Name = colorVM.Name,
			};

			await _context.Colors.AddAsync(color);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));

		}
		public async Task<IActionResult> Update(int id)
		{
			if (id <= 0) return BadRequest();
			Color existed=await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
			if (existed is null) return NotFound();
			return View(existed);
		}
		[HttpPost]

		public async Task<IActionResult> Update(int id,CreateColorVM colorVM)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}
			Color existed= await _context.Colors.FirstOrDefaultAsync(c=>c.Id==id);
			if (existed is null) return NotFound();

            bool result = await _context.Categories.AnyAsync(c => c.Name == existed.Name && c.Id == id);
            if (result)
            {
				ViewBag.Colors = await _context.Colors.ToListAsync();
				ModelState.AddModelError("Name", "This category already exist.");
                return View();
            }
			existed.Name = colorVM.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Color existed = await _context.Colors.FirstOrDefaultAsync(t => t.Id == id);
            if (existed == null) NotFound();
            _context.Colors.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return BadRequest();

            Color detail = await _context.Colors.FirstOrDefaultAsync(d => d.Id == id);
            if (detail == null) NotFound();


            return View(detail);

        }
    }
}

