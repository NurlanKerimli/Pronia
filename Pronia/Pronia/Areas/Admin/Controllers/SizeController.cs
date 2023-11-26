using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.Admin.ViewModels;
using Pronia.Areas.Admin.ViewModels.Size;
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
		public async Task<IActionResult> Create()
		{
			ViewBag.Sizes = await _context.Sizes.ToListAsync();
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(CreateSizeVM sizeVM)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Sizes=await _context.Sizes.ToListAsync();
				return View();
			}
			bool result = _context.Sizes.Any(c => c.Name.Trim() == sizeVM.Name.Trim());
			if (!result)
			{
				ViewBag.Sizes = await _context.Sizes.ToListAsync();
				ModelState.AddModelError("Name", "This name is already available.");
				return View();
			}
			Size size = new Size()
			{
				Name = sizeVM.Name,
			};
			await _context.Sizes.AddAsync(size);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));

		}

		public async Task<IActionResult> Update(int id)
		{
			if (id<=0) return BadRequest();
			Size existed=await _context.Sizes.FirstOrDefaultAsync(s=>s.Id==id);
			if(existed is null) return NotFound();
			return View(existed);
		}
		[HttpPost]
		public async Task<IActionResult> Update(int id, UpdateSizeVM sizeVM)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Sizes = await _context.Sizes.ToListAsync();
				return View();
			}
			Size existed = await _context.Sizes.FirstOrDefaultAsync(s => s.Id == id);
			if (existed is null) return NotFound();

			bool result = await _context.Categories.AnyAsync(c => c.Name == sizeVM.Name && c.Id == id);
			if (result)
			{
				ViewBag.Sizes = await _context.Sizes.ToListAsync();
				ModelState.AddModelError("Name", "This category already exist.");
				return View();
			}
			existed.Name = sizeVM.Name;
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
