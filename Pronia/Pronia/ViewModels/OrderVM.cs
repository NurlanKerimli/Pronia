using Pronia.Models;

namespace Pronia.ViewModels
{
    public class OrderVM
    {
        public string Address {  get; set; }
        public List<BasketItem>? BasketItems { get; set; }

    }
}
