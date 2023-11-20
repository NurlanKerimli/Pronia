namespace Pronia.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        //public string Image1 { get; set; }
        //public string Image2 { get; set; }
        public string SKU { get; set; }
        public int CategoryId {  get; set; }
        public Category Category { get; set; }
        List<ProductImage>? ProductImages {  get; set; }
    }
}
