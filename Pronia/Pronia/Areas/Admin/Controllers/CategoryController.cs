using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index()
        {
            List<Category> categories= await _context.Categories.ToListAsync();
            return View(categories);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if(!ModelState.IsValid)
            {
                return View(category);
            }
            bool result = _context.Categories.Any(c=>c.Name.Trim()==category.Name.Trim());
            if (result)
            {
                ModelState.AddModelError("Name", "This name is already available.");
            }
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
           
        }

        public async Task<IActionResult>Update(int id)
        {
            if(id<=0)return BadRequest();
            Category category= await _context.Categories.FirstOrDefaultAsync(c=>c.Id==id);

            if (category==null) NotFound();
            return View(category);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (ModelState.IsValid)
            {
                return View();
            }
            Category existed=await _context.Categories.FirstOrDefaultAsync(c=>c.Id == id);
            if (existed==null) NotFound();

            bool result= await _context.Categories.AnyAsync(c=>c.Name==category.Name && c.Id==id);
            if (result)
            {
                ModelState.AddModelError("Name", "This category already exist.");
                return View();
            }
            existed.Name= category.Name;
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

			Category detail = await _context.Categories.FirstOrDefaultAsync(d => d.Id == id);
            if (detail==null) NotFound(); 

            
            
            return View(detail);

        }
    }

}
