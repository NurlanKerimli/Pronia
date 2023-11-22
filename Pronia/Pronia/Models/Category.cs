using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Pronia.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage="Insert Name")]
        [MaxLength(25,ErrorMessage = "No more than 25 values should be sent")]
        public string Name { get; set; }
        public List<Product>? Products { get; set; }
    }
}
