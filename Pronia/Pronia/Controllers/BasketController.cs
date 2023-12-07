using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModels;

namespace Pronia.Controllers
{
	[AutoValidateAntiforgeryToken]
	public class BasketController : Controller
	{
		private readonly AppDbContext _context;
		public BasketController(AppDbContext context)
		{
			_context = context;
		}
		public async Task<IActionResult> Index()
		{
			List<BasketItemVM> items=new List<BasketItemVM>();
			if (Request.Cookies["Basket"] is not null)
			{
				List<BasketCookieItemVM> cookies = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);
				foreach (var cookie in cookies)
				{
					Product product = await _context.Products
						.Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary==true))
						.FirstOrDefaultAsync(p => p.Id == cookie.Id);
					if(product != null)
					{
						BasketItemVM item = new BasketItemVM()
						{
							Id = product.Id,
							Name = product.Name,
							Image = product.ProductImages.FirstOrDefault().Url,
							Price = product.Price,
							Count = cookie.Count,
							SubTotal = product.Price * cookie.Count
						};
						items.Add(item);
					}
				}
			}
			return View(items);
		}

		public async Task<IActionResult> AddBasket(int id)
		{
			if (id <= 0) return BadRequest();
			Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
			if (product == null) return NotFound();

			List<BasketCookieItemVM> basket;
			if (Request.Cookies["Basket"]is null)
			{
				basket = new List<BasketCookieItemVM>();
				BasketCookieItemVM item = new BasketCookieItemVM()
				{
					Id = id,
					Count = 1
				};
				basket.Add(item);
			}
			else
			{
				basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);
				BasketCookieItemVM existed=basket.FirstOrDefault(b=>b.Id==id);
				if(existed is null)
				{
					BasketCookieItemVM item = new BasketCookieItemVM()
					{
						Id = id,
						Count = 1
					};
					basket.Add(item);
				}
				else
				{
					existed.Count++;
				}
			}
			
			string json=JsonConvert.SerializeObject(basket);
			Response.Cookies.Append("Basket", json);
			return RedirectToAction(nameof(Index), "Home");
		}

		public IActionResult GetBasket()
		{
			return Content(Request.Cookies["Basket"]);
		}

		public IActionResult Remove(int id)
		{
			if (id <= 0) return BadRequest();
			List<BasketCookieItemVM> basket;
			if (Request.Cookies["Basket"] is null)
			{
				return RedirectToAction(nameof(Index));
			}
			basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);
			BasketCookieItemVM removed=basket.FirstOrDefault(b=>b.Id==id);
			if(removed != null)
			{
				basket.Remove(removed);
			}
			string json=JsonConvert.SerializeObject(basket);
			Response.Cookies.Append("Basket", json);
			return RedirectToAction(nameof(Index),"Basket");
		}
		public IActionResult Plus(int id)
		{
			if (id <= 0) return BadRequest();
			List<BasketCookieItemVM> basket = new List<BasketCookieItemVM>();

			if (Request.Cookies["Basket"] is null)
			{
				return RedirectToAction(nameof(Index));
			}

			basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);
			BasketCookieItemVM item = basket.FirstOrDefault(b => b.Id == id);

			if (item != null)
			{
				item.Count++;
			}

			string json = JsonConvert.SerializeObject(basket);
			Response.Cookies.Append("Basket", json);

			return RedirectToAction(nameof(Index), "Basket");
		}

		public IActionResult Minus(int id)
		{
			if (id <= 0) return BadRequest();
			List<BasketCookieItemVM> basket;

			if (Request.Cookies["Basket"] is null)
			{
				return RedirectToAction(nameof(Index));
			}

			basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);
			BasketCookieItemVM item = basket.FirstOrDefault(b => b.Id == id);

			if (item != null && item.Count > 1)
			{
				item.Count--;
			}

			string json = JsonConvert.SerializeObject(basket);
			Response.Cookies.Append("Basket", json);

			return RedirectToAction(nameof(Index), "Basket");
		}
	}
}


