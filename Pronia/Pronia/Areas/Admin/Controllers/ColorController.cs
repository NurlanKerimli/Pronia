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
	}
}

