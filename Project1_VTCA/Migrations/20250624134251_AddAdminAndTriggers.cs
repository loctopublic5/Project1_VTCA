using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAndTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Script SQL để tạo tài khoản Admin một cách an toàn
            var seedAdminSql = @"
            SET IDENTITY_INSERT Users ON;

            IF NOT EXISTS (SELECT 1 FROM Users WHERE UserID = 1)
            BEGIN
                INSERT INTO Users (UserID, Username, PasswordHash, FullName, PhoneNumber, Email, Gender, Role, IsActive, Balance, TotalSpending)
                VALUES (1, 'admin', '03a3b022b6424cb8928545869429433158c3f443a53942468f731154563a5682', N'Quản Trị Viên', '0987654321', 'admin@shop.com', 'Unisex', 'Admin', 1, 0, 0);
            END

            SET IDENTITY_INSERT Users OFF;
            ";
            migrationBuilder.Sql(seedAdminSql);

            // Trigger cho việc INSERT vào ProductSizes
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
            END;
            ";
            migrationBuilder.Sql(insertTriggerSql);

            // Trigger cho việc UPDATE trong ProductSizes
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
            END;
            ";
            migrationBuilder.Sql(updateTriggerSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Users WHERE UserID = 1;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS UpdateTotalQuantityOnInsert;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS UpdateTotalQuantityOnUpdate;");
        }
    }
}