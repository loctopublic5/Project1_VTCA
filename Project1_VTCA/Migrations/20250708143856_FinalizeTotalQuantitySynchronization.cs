using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeTotalQuantitySynchronization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Cài đặt Trigger đã sửa lỗi để xử lý cho tương lai
            const string createTriggerSql = @"
            CREATE TRIGGER TRG_ProductSizes_UpdateTotalQuantity
            ON ProductSizes
            AFTER INSERT, UPDATE, DELETE
            AS
            BEGIN
                SET NOCOUNT ON;

                -- Bảng tạm không cần PRIMARY KEY để xử lý các thao tác hàng loạt
                DECLARE @AffectedProductIDs TABLE (ProductID INT);

                -- Thu thập tất cả các ProductID bị ảnh hưởng (có thể trùng lặp)
                INSERT INTO @AffectedProductIDs (ProductID) SELECT ProductID FROM inserted;
                INSERT INTO @AffectedProductIDs (ProductID) SELECT ProductID FROM deleted;

                -- Cập nhật bảng Products dựa trên danh sách các ProductID duy nhất
                UPDATE p
                SET p.TotalQuantity = ISNULL((
                    SELECT SUM(ISNULL(ps.QuantityInStock, 0))
                    FROM ProductSizes ps
                    WHERE ps.ProductID = p.ProductID
                ), 0)
                FROM Products p
                -- Chỉ join với các ProductID duy nhất
                INNER JOIN (SELECT DISTINCT ProductID FROM @AffectedProductIDs) a ON p.ProductID = a.ProductID;
            END";

            // Tách làm hai lệnh vì ExecuteSqlRaw không hỗ trợ 'GO'
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_ProductSizes_UpdateTotalQuantity;");
            migrationBuilder.Sql(createTriggerSql);

            // Bước 2: Đồng bộ hóa toàn bộ dữ liệu cũ (chạy một lần)
            const string backfillSql = @"
            UPDATE p
            SET p.TotalQuantity = ISNULL(ps_sum.TotalStock, 0)
            FROM Products p
            LEFT JOIN (
                SELECT ProductID, SUM(ISNULL(QuantityInStock, 0)) as TotalStock
                FROM ProductSizes
                GROUP BY ProductID
            ) ps_sum ON p.ProductID = ps_sum.ProductID;
            ";
            migrationBuilder.Sql(backfillSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Logic để rollback: chỉ cần xóa trigger đi là đủ
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_ProductSizes_UpdateTotalQuantity;");
        }
    }
}