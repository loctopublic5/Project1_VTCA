using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryID", "CategoryType", "IsPromotion", "Name", "ParentID" },
                values: new object[,]
                {
                    { 1, "Product", false, "Phong cách", null },
                    { 2, "Brand", false, "Thương hiệu", null },
                    { 3, "Product", false, "Giày Lifestyle Kinh Điển", 1 },
                    { 4, "Product", false, "Giày Retro & Di Sản", 1 },
                    { 5, "Product", false, "Giày Chạy Bộ & Công Nghệ", 1 },
                    { 6, "Product", false, "Giày Chunky & Cá Tính", 1 },
                    { 7, "Product", false, "Giày Local Brand Đột Phá", 1 },
                    { 8, "Brand", false, "Nike", 2 },
                    { 9, "Brand", false, "Adidas", 2 },
                    { 10, "Brand", false, "Converse", 2 },
                    { 11, "Brand", false, "Vans", 2 },
                    { 12, "Brand", false, "New Balance", 2 },
                    { 13, "Brand", false, "Fila", 2 },
                    { 14, "Brand", false, "Biti's", 2 },
                    { 15, "Brand", false, "Ananas", 2 },
                    { 16, "Brand", false, "Puma", 2 },
                    { 17, "Brand", false, "Skechers", 2 },
                    { 18, "Brand", false, "MLB", 2 },
                    { 19, "Brand", false, "Asics", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryID",
                keyValue: 2);
        }
    }
}
