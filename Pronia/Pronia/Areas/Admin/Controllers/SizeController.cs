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
	}
}
