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
        public List<ProductImage>? ProductImages {  get; set; }
        public int ColorId { get; set; }
        public Color Color { get; set; }
        public List<Color> Colors { get; set; }
        public int SizeId {  get; set; }
        public Size Size { get; set; }
        public List<Size> Sizes { get; set; }
    }
}
