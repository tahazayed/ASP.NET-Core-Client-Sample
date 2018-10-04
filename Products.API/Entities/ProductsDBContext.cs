using Microsoft.EntityFrameworkCore;

namespace Products.API.Entities
{
    public  class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
        {
            base.Database.Migrate();
        }

        public DbSet<Product> Products { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasIndex(b => b.Name);
        }
    }
}
