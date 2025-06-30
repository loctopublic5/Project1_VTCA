using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductQuantityTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xóa các trigger khi nâng cấp CSDL
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.UpdateTotalQuantityOnInsert;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.UpdateTotalQuantityOnUpdate;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Tạo lại các trigger nếu cần hạ cấp CSDL (rollback)
            var insertTriggerSql = @"
            CREATE TRIGGER UpdateTotalQuantityOnInsert
            ON ProductSizes
            AFTER INSERT
            AS
            BEGIN
                SET NOCOUNT ON;
                UPDATE p
                SET p.TotalQuantity = ISNULL((
                    SELECT SUM(ps.QuantityInStock)
                    FROM ProductSizes ps
                    WHERE ps.ProductID = i.ProductID
                ), 0)
                FROM Products p
                JOIN inserted i ON p.ProductID = i.ProductID;
            END;";
            migrationBuilder.Sql(insertTriggerSql);

            var updateTriggerSql = @"
            CREATE TRIGGER UpdateTotalQuantityOnUpdate
            ON ProductSizes
            AFTER UPDATE
            AS
            BEGIN
                SET NOCOUNT ON;
                UPDATE p
                SET p.TotalQuantity = ISNULL((
                    SELECT SUM(ps.QuantityInStock)
                    FROM ProductSizes ps
                    WHERE ps.ProductID = i.ProductID
                ), 0)
                FROM Products p
                JOIN inserted i ON p.ProductID = i.ProductID;
            END;";
            migrationBuilder.Sql(updateTriggerSql);
        }
    }
}