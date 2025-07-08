using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class ReimplementTotalQuantityTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Câu lệnh SQL để tạo Trigger All-in-one
            var createTriggerSql = @"
            CREATE TRIGGER TRG_ProductSizes_UpdateTotalQuantity
            ON ProductSizes
            AFTER INSERT, UPDATE, DELETE
            AS
            BEGIN
                SET NOCOUNT ON;

                -- Lấy tất cả các ProductID bị ảnh hưởng từ cả dòng được thêm/sửa và dòng bị xóa
                DECLARE @AffectedProductIDs TABLE (ProductID INT PRIMARY KEY);
                INSERT INTO @AffectedProductIDs (ProductID) SELECT ProductID FROM inserted;
                INSERT INTO @AffectedProductIDs (ProductID) SELECT ProductID FROM deleted WHERE ProductID NOT IN (SELECT ProductID FROM @AffectedProductIDs);

                -- Cập nhật bảng Products cho các ProductID bị ảnh hưởng
                UPDATE p
                SET p.TotalQuantity = ISNULL((
                    SELECT SUM(ISNULL(ps.QuantityInStock, 0))
                    FROM ProductSizes ps
                    WHERE ps.ProductID = p.ProductID
                ), 0)
                FROM Products p
                INNER JOIN @AffectedProductIDs a ON p.ProductID = a.ProductID;
            END
            ";

            migrationBuilder.Sql(createTriggerSql);

            // Chạy một lần cập nhật cuối cùng để đồng bộ hóa toàn bộ dữ liệu cũ
            var finalUpdateSql = @"
            UPDATE p
            SET p.TotalQuantity = ISNULL(sq.TotalStock, 0)
            FROM Products p
            LEFT JOIN (
                SELECT 
                    ProductID, 
                    SUM(ISNULL(QuantityInStock, 0)) AS TotalStock
                FROM ProductSizes
                GROUP BY ProductID
            ) AS sq ON p.ProductID = sq.ProductID;
            ";
            migrationBuilder.Sql(finalUpdateSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_ProductSizes_UpdateTotalQuantity;");
        }
    }
}