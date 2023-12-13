using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Pronia.Areas.Admin.ViewModels;
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
		public async Task<IActionResult> Index(int page=1)
		{
			int count=await _context.Tags.CountAsync();
			ViewBag.TotalPage = Math.Ceiling((double)count / 3);
			ViewBag.CurrentPage = page;
			List<Tag> tags = await _context.Tags.Skip((page-1)*3).Take(3).ToListAsync();
			PaginateVM<Tag> paginateVM = new PaginateVM<Tag>
			{
				Items = tags,
				TotalPage=Math.Ceiling((double)count / 3),
				CurrentPage=page
			};
			return View(paginateVM);
		}
		public async Task<IActionResult> Create()
		{
			ViewBag.Tags = await _context.Tags.ToListAsync();
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(CreateTagVM tagVM)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Tags=await _context.Tags.ToListAsync();
				return View();
			}
			bool result = _context.Tags.Any(c => c.Name.Trim() == tagVM.Name.Trim());
			if (!result)
			{
				ViewBag.Tags = await _context.Tags.ToListAsync();
				ModelState.AddModelError("Name", "This name is already available.");
			}
			Tag tag = new Tag()
			{
				Name = tagVM.Name,
			};

			await _context.Tags.AddAsync(tag);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> Update(int id)
		{
			if (id <= 0) return BadRequest();
          
			Tag existed = await _context.Tags.FirstOrDefaultAsync(t => t.Id== id);
            
			if (existed == null) NotFound();
			UpdateTagVM tagVM = new UpdateTagVM()
			{
				Name = existed.Name,
			};
			return View(tagVM);
		
		}
		[HttpPost]

		public async Task<IActionResult> Update(int id, UpdateTagVM tagVM)
		{
			if(ModelState.IsValid)return View();
			Tag existed=await _context.Tags.FirstOrDefaultAsync(t=>t.Id==id);
			if (existed == null) NotFound();

			bool result=await _context.Tags.AnyAsync(t=>t.Name==tagVM.Name && t.Id==id);
			if (result)
			{
				ViewBag.Tags = await _context.Tags.ToListAsync();
				ModelState.AddModelError("Name", "This category already exist.");
				return View();
			}
			existed.Name = tagVM.Name;
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

        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return BadRequest();

            Tag detail = await _context.Tags.FirstOrDefaultAsync(d => d.Id == id);
            if (detail == null) NotFound();



            return View(detail);

        }
    }
}

