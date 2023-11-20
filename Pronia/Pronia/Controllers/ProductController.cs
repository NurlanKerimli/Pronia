using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;

namespace Pronia.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}
        public IActionResult Detail(int id)
        {
            if (id <= 0) return BadRequest();
            Product product=_context.Products.Include(p=>p.Category).FirstOrDefault(P=>P.Id == id);
            if (product is null)return NotFound();
            
            return View(product);


        }
    }
}
