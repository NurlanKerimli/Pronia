using System.ComponentModel.DataAnnotations;

namespace Pronia.ViewModels
{
	public class RegisterVM
	{
		[Required]
		[MinLength(3)]
		[MaxLength(25)]
		public string Name { get; set; }
		[Required]
		[MinLength(3)]
		[MaxLength(25)]
		public string Surname {  get; set; }
		[Required]
		[MinLength(6)]
		[MaxLength(25)]
		public string Username { get; set; }
		[Required]
		public string Gender { get; set; }
		[Required]
		[MaxLength(256)]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		[Required]
		[DataType(DataType.Password)]
		[Compare(nameof(Password))]
		public string ConfirmPassword { get; set; }

	}
}
