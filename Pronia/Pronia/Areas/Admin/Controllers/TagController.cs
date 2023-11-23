using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Pronia.DAL;
using Pronia.Models;

namespace Pronia.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class TagController : Controller
	{
		private readonly AppDbContext _context;

		public TagController(AppDbContext context)
		{
			_context = context;
		}
		public async Task<IActionResult> Index()
		{
			List<Tag> tags = await _context.Tags.ToListAsync();
			return View(tags);
		}
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(Tag tag)
		{
			if (!ModelState.IsValid)
			{
				return View(tag);
			}
			bool result = _context.Tags.Any(c => c.Name.Trim() == tag.Name.Trim());
			if (result)
			{
				ModelState.AddModelError("Name", "This name is already available.");
			}
			await _context.Tags.AddAsync(tag);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}


		public async Task<IActionResult> Update(int id)
		{
			if (id <= 0) return BadRequest();
          
			Tag tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id== id);
            
			if (tag == null) NotFound();
			return View(tag);
		
		}
		[HttpPost]

		public async Task<IActionResult> Update(int id,Tag tag)
		{
			if(ModelState.IsValid)return View();
			Tag existed=await _context.Tags.FirstOrDefaultAsync(t=>t.Id==id);
			if (existed == null) NotFound();

			bool result=await _context.Tags.AnyAsync(t=>t.Name==tag.Name && t.Id==id);
			if (result)
			{
				ModelState.AddModelError("Name", "This category already exist.");
				return View();
			}
			existed.Name = tag.Name;
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));

		}
		public async Task<IActionResult> Delete(int id)
		{
			if(id<=0) return BadRequest();
			Tag existed=await _context.Tags.FirstOrDefaultAsync(t=> t.Id==id);
			if (existed == null) NotFound();
			_context.Tags.Remove(existed);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
	}
}

