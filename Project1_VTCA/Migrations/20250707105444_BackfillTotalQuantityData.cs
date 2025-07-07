using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class BackfillTotalQuantityData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    UPDATE p
    SET p.TotalQuantity = ISNULL(ps_sum.TotalStock, 0)
    FROM Products p
    LEFT JOIN (
        SELECT ProductID, SUM(ISNULL(QuantityInStock, 0)) as TotalStock
        FROM ProductSizes
        GROUP BY ProductID
    ) ps_sum ON p.ProductID = ps_sum.ProductID;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
