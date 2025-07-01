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
                VALUES (1, 'Admin25', '602e1baac1bdc4c6587dbe0f0a14a2c2737fe9718aa84e255db58e69142b6dec', N'Quản Trị Viên', '0987654321', 'admin@shop.com', 'Unisex', 'Admin', 1, 0, 0);
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