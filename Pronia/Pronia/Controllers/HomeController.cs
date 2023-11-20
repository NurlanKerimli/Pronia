using Microsoft.AspNetCore.Mvc;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModels;

namespace Pronia.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            List<Slide> slides = _context.Slides.OrderBy(s => s.Order).ToList();
			List<Product> products = _context.Products.Take(4).ToList();
            List<Product> latests=_context.Products.OrderByDescending(l=>l.Id).Take(8).ToList();
            HomeVM home = new HomeVM
            {
                Slides = slides,
                Products = products,
                Latests= latests,
                
            };

            return View(home);
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
