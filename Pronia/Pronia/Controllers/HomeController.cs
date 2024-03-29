﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Services;
using Pronia.ViewModels;

namespace Pronia.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            throw new Exception("Exception");

            List<Slide> slides =await _context.Slides.OrderBy(s => s.Order).ToListAsync();
			List<Product> products =await _context.Products.Take(4)
                .Include(p => p.ProductImages.Where(pi=>pi.IsPrimary!=null))
                .ToListAsync();
            List<Product> latests=await _context.Products.OrderByDescending(l=>l.Id).Take(8).Include(l => l.ProductImages).ToListAsync();
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
        public IActionResult ErrorPage(string error= "An error occurred.")
        {
            return View(model:error);
        }

      
          

    }
}
