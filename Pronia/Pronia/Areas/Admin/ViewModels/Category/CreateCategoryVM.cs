using Pronia.Models;

namespace Pronia.Areas.Admin.ViewModels
{
	public class CreateCategoryVM
	{
		public string Name { get; set; }
		public List<Category> Categories { get; internal set; }
	}
}
