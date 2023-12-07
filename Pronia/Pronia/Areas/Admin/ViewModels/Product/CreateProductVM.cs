using Pronia.Models;
using System.ComponentModel.DataAnnotations;

namespace Pronia.Areas.Admin.ViewModels
{
	public class CreateProductVM
	{
		public string Name { get; set; }
		public decimal Price { get; set; }
		public string Description { get; set; }
		public string SKU { get; set; }
		[Required]
		public int? CategoryId { get; set; }
        public List<Category>? Categories { get; set; }
        public List<int> TagIds {  get; set; }
		public List<Tag>? Tags { get; set; }
        public List<Models.Color>? Colors { get; set; }
        public List<Models.Size>? Sizes { get; set; }
		public IFormFile MainPhoto {  get; set; }
		public IFormFile HoverPhoto { get; set; }	
		public List<IFormFile>? Photos { get; set; }
	}
}
