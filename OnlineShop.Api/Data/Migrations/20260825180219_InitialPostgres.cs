using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OnlineShop.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParentCategoryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Size = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "ParentCategoryId" },
                values: new object[,]
                {
                    { 1, "Clothing", null },
                    { 2, "Electronics", null },
                    { 3, "Home & Living", null },
                    { 4, "Men", 1 },
                    { 5, "Women", 1 },
                    { 6, "Computers", 2 },
                    { 7, "Phones", 2 },
                    { 9, "Furniture", 3 },
                    { 10, "Kitchen", 3 },
                    { 8, "Laptops", 6 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "Name", "Price", "StockQuantity" },
                values: new object[,]
                {
                    { 1, 4, "Soft everyday tee in a regular fit.", "https://picsum.photos/seed/tee/600/600", "Classic Cotton T-Shirt", 24.99m, 120 },
                    { 2, 4, "Stretch chinos suitable for work or weekend.", "https://picsum.photos/seed/chinos/600/600", "Slim Fit Chinos", 59.99m, 80 },
                    { 3, 5, "Lightweight linen dress with a relaxed silhouette.", "https://picsum.photos/seed/dress/600/600", "Linen Summer Dress", 79.99m, 45 },
                    { 4, 5, "Soft open-front cardigan for layering.", "https://picsum.photos/seed/cardigan/600/600", "Knit Cardigan", 64.50m, 60 },
                    { 5, 7, "6.5-inch OLED display with dual camera system.", "https://picsum.photos/seed/phone/600/600", "NovaPhone X", 699.00m, 35 },
                    { 6, 7, "Wireless earbuds with active noise cancellation.", "https://picsum.photos/seed/buds/600/600", "Pulse Buds Pro", 149.00m, 90 },
                    { 9, 9, "Solid oak table seating up to six.", "https://picsum.photos/seed/table/600/600", "Oak Dining Table", 449.00m, 12 },
                    { 10, 9, "Upholstered armchair with walnut legs.", "https://picsum.photos/seed/chair/600/600", "Lounge Armchair", 299.00m, 20 },
                    { 11, 10, "Non-stick ceramic set with three pans and lids.", "https://picsum.photos/seed/cookware/600/600", "Ceramic Cookware Set", 129.99m, 40 },
                    { 12, 10, "Glass carafe, dripper, and filters for daily brewing.", "https://picsum.photos/seed/coffee/600/600", "Pour-Over Coffee Kit", 42.00m, 70 }
                });

            migrationBuilder.InsertData(
                table: "ProductVariants",
                columns: new[] { "Id", "Color", "ProductId", "Size", "StockQuantity" },
                values: new object[,]
                {
                    { 1, "Black", 1, "S", 20 },
                    { 2, "Black", 1, "M", 35 },
                    { 3, "Black", 1, "L", 30 },
                    { 4, "Black", 1, "XL", 15 },
                    { 5, "White", 1, "M", 20 },
                    { 6, "White", 1, "L", 20 },
                    { 7, "Sand", 3, "S", 10 },
                    { 8, "Sand", 3, "M", 15 },
                    { 9, "Sand", 3, "L", 12 },
                    { 10, "Navy", 3, "M", 8 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "Name", "Price", "StockQuantity" },
                values: new object[,]
                {
                    { 7, 8, "Ultralight 14-inch laptop with all-day battery life.", "https://picsum.photos/seed/aerobook/600/600", "AeroBook 14", 1099.00m, 25 },
                    { 8, 8, "High-performance laptop for creators and developers.", "https://picsum.photos/seed/forgepro/600/600", "ForgePro 16", 1599.00m, 15 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
