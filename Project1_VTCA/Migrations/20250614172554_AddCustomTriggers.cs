using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sử dụng cú pháp @"..." để có thể viết SQL trên nhiều dòng

            // --- Trigger tạo tài khoản mặc định là Customer ---
            var enforceSingleAdminTrigger = @"
            CREATE TRIGGER EnforceSingleAdmin
            ON Users
            INSTEAD OF INSERT
            AS
            BEGIN
                DECLARE @AdminCount INT;
                SELECT @AdminCount = COUNT(*) FROM Users WHERE Role = 'Admin';
                
                IF @AdminCount >= 1 AND EXISTS(SELECT 1 FROM inserted WHERE Role = 'Admin')
                BEGIN
                    INSERT INTO Users (Username, PasswordHash, FullName, PhoneNumber, Email, Gender, Balance, TotalSpending, Role)
                    SELECT Username, PasswordHash, FullName, PhoneNumber, Email, Gender, Balance, TotalSpending, 'Customer'
                    FROM inserted;
                END
                ELSE
                BEGIN
                    INSERT INTO Users (Username, PasswordHash, FullName, PhoneNumber, Email, Gender, Balance, TotalSpending, Role)
                    SELECT Username, PasswordHash, FullName, PhoneNumber, Email, Gender, Balance, TotalSpending, Role
                    FROM inserted;
                END
            END;
            ";
            migrationBuilder.Sql(enforceSingleAdminTrigger);

            // --- Trigger insert size tính tổng ---
            var updateTotalQuantityOnInsertTrigger = @"
            CREATE TRIGGER UpdateTotalQuantityOnInsert
            ON ProductSizes
            AFTER INSERT
            AS
            BEGIN
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
            migrationBuilder.Sql(updateTotalQuantityOnInsertTrigger);

            // --- Trigger cho Update size tính lại tổng ---
            var updateTotalQuantityOnUpdateTrigger = @"
            CREATE TRIGGER UpdateTotalQuantityOnUpdate
            ON ProductSizes
            AFTER UPDATE
            AS
            BEGIN
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
            migrationBuilder.Sql(updateTotalQuantityOnUpdateTrigger);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Lệnh để xóa các trigger nếu cần rollback migration này
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS EnforceSingleAdmin;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS UpdateTotalQuantityOnInsert;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS UpdateTotalQuantityOnUpdate;");
        }
    }
}