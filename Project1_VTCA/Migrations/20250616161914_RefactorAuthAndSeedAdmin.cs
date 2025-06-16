using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class RefactorAuthAndSeedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Xóa Trigger cũ không còn sử dụng
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS EnforceSingleAdmin;");

            // Bước 2: Chèn dữ liệu cho tài khoản Admin
            migrationBuilder.InsertData(
            table: "Users",
            columns: new[] { "UserID", "Balance", "Email", "FullName", "Gender", "IsActive", "PasswordHash", "PhoneNumber", "Role", "TotalSpending", "Username" },
            values: new object[] { 1, 0m, "admin@shop.com", "Quản Trị Viên", "Unisex", true, "5221b343513b6321287612c6a49688484299351092e62423523425514b87216a", "0987654321", "Admin", 0m, "admin" });
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Xóa dữ liệu Admin đã chèn
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1);

            // Bước 2: Tạo lại Trigger cũ nếu cần rollback
            var enforceSingleAdminTrigger = @"
            CREATE TRIGGER EnforceSingleAdmin ON Users
            INSTEAD OF INSERT AS BEGIN
                -- ... (Dán lại nội dung trigger cũ ở đây nếu bạn muốn có thể rollback hoàn toàn)
            END";
            migrationBuilder.Sql(enforceSingleAdminTrigger);
        }
    }
}