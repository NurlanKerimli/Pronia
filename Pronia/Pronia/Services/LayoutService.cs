using Azure.Core;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModels;

namespace Pronia.Services
{
    public class LayoutService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;

        public LayoutService(AppDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }
        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
            return settings;
        }
        public async Task<List<BasketItemVM>> GetBasketsAsync()
        {
            List<BasketItemVM> items = new List<BasketItemVM>();
            if (_http.HttpContext.Request.Cookies["Basket"] is not null)
            {
                List<BasketCookieItemVM> cookies = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(_http.HttpContext.Request.Cookies["Basket"]);
                foreach (var cookie in cookies)
                {
                    Product product = await _context.Products
                        .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                        .FirstOrDefaultAsync(p => p.Id == cookie.Id);
                    if (product is not null)
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
            return items;

        }
    }
}
