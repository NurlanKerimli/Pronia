using System.ComponentModel.DataAnnotations;

namespace Pronia.Areas.Admin.ViewModels.Product
{
	public class UpdateProductVM
	{
		public string Name { get; set; }
		public decimal Price { get; set; }
		public string Description { get; set; }
		public string SKU { get; set; }
		public IFormFile Photo { get; set; }
		[Required]
		public int? CategoryId { get; set; }
	}
}
