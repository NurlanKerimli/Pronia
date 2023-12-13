using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Pronia.Areas.Admin.ViewModels;

using Pronia.DAL;
using Pronia.Models;

namespace Pronia.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page=1)
        {
            int count=await _context.Categories.CountAsync();
            ViewBag.TotalPage = Math.Ceiling((double)count / 3);
            ViewBag.CurrentPage = page;
            List<Category> categories= await _context.Categories.Skip((page-1)*3).Take(3).ToListAsync();
            PaginateVM<Category> paginateVM = new PaginateVM<Category>
            {
                Items = categories,
                TotalPage = Math.Ceiling((double)count / 3),
                CurrentPage=page
            };
            return View(paginateVM);
        }
        public async Task<IActionResult> Create()
        {
			ViewBag.Categories = await _context.Categories.ToListAsync();
			return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM categoryVM)
        {
            if(!ModelState.IsValid)
            {
                ViewBag.Categories=await _context.Categories.ToListAsync();
                return View();
            }
            bool result = _context.Categories.Any(c=>c.Name.Trim()== categoryVM.Name.Trim());
            if (!result)
            {
				ViewBag.Categories = await _context.Categories.ToListAsync();
				ModelState.AddModelError("Name", "This name is already available.");
                return View();
            }
            Category category = new Category()
            {
                Name = categoryVM.Name,
            };

            
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
           
        }

        public async Task<IActionResult>Update(int id)
        {
            if(id<=0)return BadRequest();
            Category existed= await _context.Categories.FirstOrDefaultAsync(c=>c.Id==id);

            if (existed==null) NotFound();
            UpdateCategoryVM categoryVM = new UpdateCategoryVM()
            {
                Name = existed.Name
            };
            return View(categoryVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, CreateCategoryVM categoryVM)
        {
            if (!ModelState.IsValid)
            {
				categoryVM.Categories = await _context.Categories.ToListAsync();
				return View(categoryVM);
            }
            Category existed=await _context.Categories.FirstOrDefaultAsync(c=>c.Id == id);
            if (existed==null) NotFound();

            bool result= await _context.Categories.AnyAsync(c=>c.Name== categoryVM.Name && c.Id==id);
            if (result)
            {
				ViewBag.Categories = await _context.Categories.ToListAsync();
				ModelState.AddModelError("Name", "This category already exist.");
                return View();
            }
            existed.Name= categoryVM.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id<=0) return BadRequest();
            Category existed=await _context.Categories.FirstOrDefaultAsync(c=>c.Id==id);
            if (existed == null) NotFound();
            _context .Categories.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            if(id<=0) return BadRequest();

			Category detail = await _context.Categories
                .Include(x=>x.Products).ThenInclude(y=>y.ProductImages)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (detail==null) NotFound(); 

            
            
            return View(detail);

        }
    }

}
