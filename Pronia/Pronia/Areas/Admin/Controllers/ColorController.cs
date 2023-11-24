using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(Color color)
		{
			if (!ModelState.IsValid)
			{
				return View(color);
			}
			bool result = _context.Colors.Any(c => c.Name.Trim() == color.Name.Trim());
			if (result)
			{
				ModelState.AddModelError("Name", "This name is already available.");
			}
			await _context.Colors.AddAsync(color);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));

		}
		public async Task<IActionResult> Update(int id)
		{
			if (id <= 0) return BadRequest();
			Color color=await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
			if (color is null) return NotFound();
			return View(color);
		}
		[HttpPost]

		public async Task<IActionResult> Update(int id,Color color)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}
			Color existed= await _context.Colors.FirstOrDefaultAsync(c=>c.Id==id);
			if (existed is null) return NotFound();

            bool result = await _context.Categories.AnyAsync(c => c.Name == color.Name && c.Id == id);
            if (result)
            {
                ModelState.AddModelError("Name", "This category already exist.");
                return View();
            }
			existed.Name = color.Name;
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

