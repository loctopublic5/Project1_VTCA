using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIsActiveforAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Addresses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Addresses");
        }
    }
}
