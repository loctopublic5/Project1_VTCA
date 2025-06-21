using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategoryPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Promotions",
                columns: new[] { "PromotionID", "ApplicableCategoryId", "ApplicableGender", "ApplicableProductId", "ApplicableSize", "Code", "DiscountAmount", "DiscountPercentage", "ExpiryDate", "IsActive" },
                values: new object[,]
                {
                    { 3, 5, "Female", null, null, "FEMALETECH10", null, 10.00m, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 4, 7, "Female", null, null, "FEMALELOCAL10", null, 10.00m, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 5, 6, "Male", null, null, "MALECHUNKY10", null, 10.00m, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 6, 3, "Male", null, null, "MALELIFESTYLE10", null, 10.00m, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 7, 4, "All", null, null, "RETROFORALL10", null, 10.00m, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Promotions",
                keyColumn: "PromotionID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Promotions",
                keyColumn: "PromotionID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Promotions",
                keyColumn: "PromotionID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Promotions",
                keyColumn: "PromotionID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Promotions",
                keyColumn: "PromotionID",
                keyValue: 7);
        }
    }
}
