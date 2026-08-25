using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OnlineShop.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariantsAndAdminSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false)
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
        }
    }
}
