using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class AddCartAndOrderSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderDetails",
                table: "OrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartItems",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ShippingDiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountAmountPerUnit",
                table: "OrderDetails");

            migrationBuilder.RenameColumn(
                name: "StreetAddress",
                table: "Addresses",
                newName: "AddressDetail");

            migrationBuilder.AddColumn<string>(
                name: "OrderCode",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Orders",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingPhone",
                table: "Orders",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "ProductID",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "OrderID",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderDetailID",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "CartItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "ProductID",
                table: "CartItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "CartItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<int>(
                name: "CartItemID",
                table: "CartItems",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Addresses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReceivePhone",
                table: "Addresses",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderDetails",
                table: "OrderDetails",
                column: "OrderDetailID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartItems",
                table: "CartItems",
                column: "CartItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderID",
                table: "OrderDetails",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserID",
                table: "CartItems",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderDetails",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_OrderID",
                table: "OrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartItems",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_UserID",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingPhone",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderDetailID",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "CartItemID",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "ReceivePhone",
                table: "Addresses");

            migrationBuilder.RenameColumn(
                name: "AddressDetail",
                table: "Addresses",
                newName: "StreetAddress");

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingDiscountAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "ProductID",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "OrderID",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmountPerUnit",
                table: "OrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "CartItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "CartItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "ProductID",
                table: "CartItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderDetails",
                table: "OrderDetails",
                columns: new[] { "OrderID", "ProductID", "Size" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartItems",
                table: "CartItems",
                columns: new[] { "UserID", "ProductID", "Size" });
        }
    }
}
