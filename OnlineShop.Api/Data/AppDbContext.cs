using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Entities;

namespace OnlineShop.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);

            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).IsRequired().HasMaxLength(2000);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.ImageUrl).IsRequired().HasMaxLength(500);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Size).HasMaxLength(50);
            entity.Property(v => v.Color).HasMaxLength(50);

            entity.HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Top-level categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Clothing", ParentCategoryId = null },
            new Category { Id = 2, Name = "Electronics", ParentCategoryId = null },
            new Category { Id = 3, Name = "Home & Living", ParentCategoryId = null }
        );

        // Subcategories (and one nested level under Electronics)
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 4, Name = "Men", ParentCategoryId = 1 },
            new Category { Id = 5, Name = "Women", ParentCategoryId = 1 },
            new Category { Id = 6, Name = "Computers", ParentCategoryId = 2 },
            new Category { Id = 7, Name = "Phones", ParentCategoryId = 2 },
            new Category { Id = 8, Name = "Laptops", ParentCategoryId = 6 },
            new Category { Id = 9, Name = "Furniture", ParentCategoryId = 3 },
            new Category { Id = 10, Name = "Kitchen", ParentCategoryId = 3 }
        );

        modelBuilder.Entity<Product>().HasData(
            // Men
            new Product
            {
                Id = 1,
                Name = "Classic Cotton T-Shirt",
                Description = "Soft everyday tee in a regular fit.",
                Price = 24.99m,
                StockQuantity = 120,
                CategoryId = 4,
                ImageUrl = "https://picsum.photos/seed/tee/600/600"
            },
            new Product
            {
                Id = 2,
                Name = "Slim Fit Chinos",
                Description = "Stretch chinos suitable for work or weekend.",
                Price = 59.99m,
                StockQuantity = 80,
                CategoryId = 4,
                ImageUrl = "https://picsum.photos/seed/chinos/600/600"
            },
            // Women
            new Product
            {
                Id = 3,
                Name = "Linen Summer Dress",
                Description = "Lightweight linen dress with a relaxed silhouette.",
                Price = 79.99m,
                StockQuantity = 45,
                CategoryId = 5,
                ImageUrl = "https://picsum.photos/seed/dress/600/600"
            },
            new Product
            {
                Id = 4,
                Name = "Knit Cardigan",
                Description = "Soft open-front cardigan for layering.",
                Price = 64.50m,
                StockQuantity = 60,
                CategoryId = 5,
                ImageUrl = "https://picsum.photos/seed/cardigan/600/600"
            },
            // Phones
            new Product
            {
                Id = 5,
                Name = "NovaPhone X",
                Description = "6.5-inch OLED display with dual camera system.",
                Price = 699.00m,
                StockQuantity = 35,
                CategoryId = 7,
                ImageUrl = "https://picsum.photos/seed/phone/600/600"
            },
            new Product
            {
                Id = 6,
                Name = "Pulse Buds Pro",
                Description = "Wireless earbuds with active noise cancellation.",
                Price = 149.00m,
                StockQuantity = 90,
                CategoryId = 7,
                ImageUrl = "https://picsum.photos/seed/buds/600/600"
            },
            // Laptops (nested under Computers)
            new Product
            {
                Id = 7,
                Name = "AeroBook 14",
                Description = "Ultralight 14-inch laptop with all-day battery life.",
                Price = 1099.00m,
                StockQuantity = 25,
                CategoryId = 8,
                ImageUrl = "https://picsum.photos/seed/aerobook/600/600"
            },
            new Product
            {
                Id = 8,
                Name = "ForgePro 16",
                Description = "High-performance laptop for creators and developers.",
                Price = 1599.00m,
                StockQuantity = 15,
                CategoryId = 8,
                ImageUrl = "https://picsum.photos/seed/forgepro/600/600"
            },
            // Furniture
            new Product
            {
                Id = 9,
                Name = "Oak Dining Table",
                Description = "Solid oak table seating up to six.",
                Price = 449.00m,
                StockQuantity = 12,
                CategoryId = 9,
                ImageUrl = "https://picsum.photos/seed/table/600/600"
            },
            new Product
            {
                Id = 10,
                Name = "Lounge Armchair",
                Description = "Upholstered armchair with walnut legs.",
                Price = 299.00m,
                StockQuantity = 20,
                CategoryId = 9,
                ImageUrl = "https://picsum.photos/seed/chair/600/600"
            },
            // Kitchen
            new Product
            {
                Id = 11,
                Name = "Ceramic Cookware Set",
                Description = "Non-stick ceramic set with three pans and lids.",
                Price = 129.99m,
                StockQuantity = 40,
                CategoryId = 10,
                ImageUrl = "https://picsum.photos/seed/cookware/600/600"
            },
            new Product
            {
                Id = 12,
                Name = "Pour-Over Coffee Kit",
                Description = "Glass carafe, dripper, and filters for daily brewing.",
                Price = 42.00m,
                StockQuantity = 70,
                CategoryId = 10,
                ImageUrl = "https://picsum.photos/seed/coffee/600/600"
            }
        );

        modelBuilder.Entity<ProductVariant>().HasData(
            new ProductVariant { Id = 1, ProductId = 1, Size = "S", Color = "Black", StockQuantity = 20 },
            new ProductVariant { Id = 2, ProductId = 1, Size = "M", Color = "Black", StockQuantity = 35 },
            new ProductVariant { Id = 3, ProductId = 1, Size = "L", Color = "Black", StockQuantity = 30 },
            new ProductVariant { Id = 4, ProductId = 1, Size = "XL", Color = "Black", StockQuantity = 15 },
            new ProductVariant { Id = 5, ProductId = 1, Size = "M", Color = "White", StockQuantity = 20 },
            new ProductVariant { Id = 6, ProductId = 1, Size = "L", Color = "White", StockQuantity = 20 },
            new ProductVariant { Id = 7, ProductId = 3, Size = "S", Color = "Sand", StockQuantity = 10 },
            new ProductVariant { Id = 8, ProductId = 3, Size = "M", Color = "Sand", StockQuantity = 15 },
            new ProductVariant { Id = 9, ProductId = 3, Size = "L", Color = "Sand", StockQuantity = 12 },
            new ProductVariant { Id = 10, ProductId = 3, Size = "M", Color = "Navy", StockQuantity = 8 }
        );
    }
}
