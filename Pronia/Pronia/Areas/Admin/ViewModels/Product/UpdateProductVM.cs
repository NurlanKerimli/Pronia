using System.ComponentModel.DataAnnotations;

namespace Pronia.Areas.Admin.ViewModels

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
		List<int> TagIds { get; set; }
		public List<Category>? Categories { get; set; }
		public List<Tag>? Tags { get; set; }
	}
}
