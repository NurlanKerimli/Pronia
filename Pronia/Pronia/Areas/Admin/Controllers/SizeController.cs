using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;

namespace Pronia.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class SizeController : Controller
	{
		private readonly AppDbContext _context;

		public SizeController(AppDbContext context)
		{
			_context = context;
		}
		public async Task<IActionResult> Index()
		{
			List<Size> sizes = await _context.Sizes.ToListAsync();
			return View(sizes);
		}
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(Size size)
		{
			if (!ModelState.IsValid)
			{
				return View(size);
			}
			bool result = _context.Sizes.Any(c => c.Name.Trim() == size.Name.Trim());
			if (result)
			{
				ModelState.AddModelError("Name", "This name is already available.");
			}
			await _context.Sizes.AddAsync(size);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));

		}

		public async Task<IActionResult> Update(int id)
		{
			if (id<=0) return BadRequest();
			Size size=await _context.Sizes.FirstOrDefaultAsync(s=>s.Id==id);
			if(size is null) return NotFound();
			return View(size);
		}
		[HttpPost]
		public async Task<IActionResult> Update(int id, Size size)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}
			Size existed = await _context.Sizes.FirstOrDefaultAsync(s => s.Id == id);
			if (existed is null) return NotFound();

			bool result = await _context.Categories.AnyAsync(c => c.Name == size.Name && c.Id == id);
			if (result)
			{
				ModelState.AddModelError("Name", "This category already exist.");
				return View();
			}
			existed.Name = size.Name;
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Size existed = await _context.Sizes.FirstOrDefaultAsync(t => t.Id == id);
            if (existed == null) NotFound();
            _context.Sizes.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return BadRequest();

            Size detail = await _context.Sizes.FirstOrDefaultAsync(d => d.Id == id);
            if (detail == null) NotFound();


            return View(detail);

        }
    }
}
