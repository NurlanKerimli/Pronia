using Microsoft.EntityFrameworkCore;
using Pronia.Models;
using System.Collections.Generic;

namespace Pronia.DAL
{
    public class AppDbContext:DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<Color> Colors { get; set; }
        public DbSet<Size> Sizes { get; set; }
    }
}
