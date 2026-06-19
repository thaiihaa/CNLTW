using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KiemTraGiuaKi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DishCategories_BIT242377",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishCategories_BIT242377", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dishes_BIT242377",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreparationTime = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DishCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes_BIT242377", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dishes_BIT242377_DishCategories_BIT242377_DishCategoryId",
                        column: x => x.DishCategoryId,
                        principalTable: "DishCategories_BIT242377",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DishImages_BIT242377",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsThumbnail = table.Column<bool>(type: "bit", nullable: false),
                    DishId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishImages_BIT242377", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DishImages_BIT242377_Dishes_BIT242377_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes_BIT242377",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DishCategories_BIT242377",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Các món ăn mặn", "Món mặn" },
                    { 2, "Các món ăn chay", "Món chay" },
                    { 3, "Các món tráng miệng", "Tráng miệng" }
                });

            migrationBuilder.InsertData(
                table: "Dishes_BIT242377",
                columns: new[] { "Id", "Description", "DishCategoryId", "IsAvailable", "Name", "PreparationTime", "Price" },
                values: new object[,]
                {
                    { 1, "Phở bò truyền thống", 1, true, "Phở bò", 15, 45000m },
                    { 2, "Bánh xèo giòn tan", 1, true, "Bánh xèo", 20, 35000m },
                    { 3, "Cà tím nấu chay", 2, true, "Cà tím luộc", 10, 20000m },
                    { 4, "Canh rau cải ngọt", 2, true, "Canh rau cải", 8, 25000m },
                    { 5, "Chè đậu đỏ ngọt ngào", 3, true, "Chè đậu đỏ", 25, 15000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dish_Name_CategoryId",
                table: "Dishes_BIT242377",
                columns: new[] { "Name", "DishCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_BIT242377_DishCategoryId",
                table: "Dishes_BIT242377",
                column: "DishCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DishImages_BIT242377_DishId",
                table: "DishImages_BIT242377",
                column: "DishId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DishImages_BIT242377");

            migrationBuilder.DropTable(
                name: "Dishes_BIT242377");

            migrationBuilder.DropTable(
                name: "DishCategories_BIT242377");
        }
    }
}
