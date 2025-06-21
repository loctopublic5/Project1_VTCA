using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class SeedGenderPromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Promotions",
                columns: new[] { "PromotionID", "ApplicableCategoryId", "ApplicableGender", "ApplicableProductId", "ApplicableSize", "Code", "DiscountAmount", "DiscountPercentage", "ExpiryDate", "IsActive" },
                values: new object[,]
                {
                    { 1, null, "Female", null, null, "FORHER15", null, 15.00m, new DateTime(2030, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 2, null, "Male", null, null, "FORHIM15", null, 15.00m, new DateTime(2030, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Promotions",
                keyColumn: "PromotionID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Promotions",
                keyColumn: "PromotionID",
                keyValue: 2);
        }
    }
}
