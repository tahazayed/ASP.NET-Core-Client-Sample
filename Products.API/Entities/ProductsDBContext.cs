using Microsoft.EntityFrameworkCore;

namespace Products.API.Entities
{
    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
        {
            Database.Migrate();

        }
        public DbSet<Product> Products { get; set; }
    }
}
