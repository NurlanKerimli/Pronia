﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilities.Exceptions;
using Pronia.ViewModels;

namespace Pronia.Controllers
{
    [AutoValidateAntiforgeryToken]
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
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) throw new WrongRequestException("The query is invalid");
            Product product = await _context.Products
                .Include(p=>p.Category)
                .Include(p => p.ProductImages)
                .Include(p=>p.ProductTags)
                .FirstOrDefaultAsync(P => P.Id == id);
			if (product is null) throw new NotFoundException("Product not found");
			DetailVM detailVM = new DetailVM()
            {
                Product = product,
                RelatedProducts =await _context.Products
                .Where(p=>p.CategoryId==product.CategoryId && p.Id!=product.Id)
                .Take(12)
                .Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary!=null))
                .ToListAsync()
            };
            

            return View(detailVM);


        }
    }
}
