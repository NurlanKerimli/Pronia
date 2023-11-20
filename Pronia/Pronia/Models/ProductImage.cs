﻿namespace Pronia.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool? IsPrimary { get; set; }
        public int ProductId {  get; set; }
        public string Alternative {  get; set; }
        public Product Product { get; set; }
    }
}
