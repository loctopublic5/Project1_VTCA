using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Project1_VTCA.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchemaAndSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParentID = table.Column<int>(type: "int", nullable: true),
                    CategoryType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsPromotion = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentID",
                        column: x => x.ParentID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GenderApplicability = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TotalQuantity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSpending = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => new { x.ProductID, x.CategoryID });
                    table.ForeignKey(
                        name: "FK_ProductCategories_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCategories_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductSizes",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    QuantityInStock = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSizes", x => new { x.ProductID, x.Size });
                    table.ForeignKey(
                        name: "FK_ProductSizes_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    PromotionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ApplicableGender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ApplicableSize = table.Column<int>(type: "int", nullable: true),
                    ApplicableProductId = table.Column<int>(type: "int", nullable: true),
                    ApplicableCategoryId = table.Column<int>(type: "int", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.PromotionID);
                    table.ForeignKey(
                        name: "FK_Promotions_Categories_ApplicableCategoryId",
                        column: x => x.ApplicableCategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryID");
                    table.ForeignKey(
                        name: "FK_Promotions_Products_ApplicableProductId",
                        column: x => x.ApplicableProductId,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    AddressID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    StreetAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressID);
                    table.ForeignKey(
                        name: "FK_Addresses_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => new { x.UserID, x.ProductID, x.Size });
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApprovedByAdminID = table.Column<int>(type: "int", nullable: true),
                    AdminDecisionReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerCancellationReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RefundRequested = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderID);
                    table.ForeignKey(
                        name: "FK_Orders_Users_ApprovedByAdminID",
                        column: x => x.ApprovedByAdminID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmountPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => new { x.OrderID, x.ProductID, x.Size });
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryID", "CategoryType", "IsPromotion", "Name", "ParentID" },
                values: new object[,]
                {
                    { 1, "Product", false, "Phong cách", null },
                    { 2, "Brand", false, "Thương hiệu", null }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductID", "Description", "GenderApplicability", "IsActive", "Name", "Price", "TotalQuantity" },
                values: new object[,]
                {
                    { 1, "Biểu tượng bất hủ của Nike, thiết kế da trơn cổ điển, đệm Air-Sole êm ái. Phù hợp mọi phong cách, dễ phối đồ, độ bền cao.", "Unisex", true, "Nike Air Force 1 '07", 2290000.00m, 0 },
                    { 2, "Thiết kế mũi vỏ sò (shell-toe) đặc trưng và 3 sọc răng cưa kinh điển. Một trong những đôi sneaker có ảnh hưởng nhất mọi thời đại, dễ nhận diện.", "Unisex", true, "Adidas Superstar", 1890000.00m, 0 },
                    { 3, "Phiên bản cổ cao kinh điển với thân giày bằng vải canvas bền chắc, đế cao su lưu hóa. Biểu tượng của sự trẻ trung, cá tính và tự do.", "Unisex", true, "Converse Chuck Taylor All Star Classic", 1395000.00m, 0 },
                    { 4, "Thiết kế cổ thấp với sọc jazz đặc trưng của Vans bên hông giày. Chất liệu canvas và da lộn kết hợp. Được yêu thích trong cộng đồng skate và thời trang đường phố.", "Unisex", true, "Vans Old Skool Classic", 1480000.00m, 0 },
                    { 5, "Dáng giày chạy bộ cổ điển từ những năm 80. Công nghệ ENCAP ở đế giữa giúp tăng cường độ ổn định. Thân giày kết hợp da lộn và vải lưới thoáng khí. Mang đậm phong cách vintage.", "Unisex", true, "New Balance 574 Core", 779000.00m, 0 },
                    { 6, "Thiết kế mang tính cách mạng với cửa sổ Air có thể nhìn thấy ở gót chân. Kiểu dáng mạnh mẽ, năng động. Một trong những dòng Air Max được yêu thích nhất.", "Unisex", true, "Nike Air Max 90", 2800000.00m, 0 },
                    { 7, "Nổi tiếng với công nghệ đệm Boost mang lại sự êm ái và hoàn trả năng lượng vượt trội. Thân giày Primeknit ôm sát chân. Phù hợp cho cả chạy bộ và thời trang hàng ngày.", "Unisex", true, "Adidas Ultra Boost", 2500000.00m, 0 },
                    { 8, "Mẫu giày chunky \"đình đám\" với đế ngoài răng cưa hầm hố. Thiết kế logo Fila nổi bật. Tạo nên phong cách thời trang cá tính và thu hút.", "Female", true, "Fila Disruptor 2", 1500000.00m, 0 },
                    { 9, "Dòng sneaker thành công của Biti's, thiết kế trẻ trung. Công nghệ đế LiteFlex siêu nhẹ và êm. Nhiều phiên bản màu sắc và collab độc đáo.", "Unisex", true, "Biti's Hunter X", 767000.00m, 0 },
                    { 10, "Thiết kế vulcanized cổ điển, tối giản. Thân giày canvas, đế gum đặc trưng. Được giới trẻ yêu thích vì sự đơn giản và chất lượng.", "Unisex", true, "Ananas Basas Bumper Gum", 580000.00m, 0 },
                    { 11, "Mẫu giày chạy bộ biểu tượng từ những năm 70. Thiết kế đơn giản, thanh lịch với dấu Swoosh lớn. Đế giữa EVA nhẹ, đế ngoài xương cá tăng độ bám.", "Unisex", true, "Nike Cortez", 1800000.00m, 0 },
                    { 12, "Thiết kế tennis cổ điển, tối giản với 3 hàng lỗ thoáng khí thay cho 3 sọc. Gót giày và lưỡi gà có logo Stan Smith. Một lựa chọn thanh lịch và không bao giờ lỗi mốt.", "Unisex", true, "Adidas Stan Smith", 2290000.00m, 0 },
                    { 13, "Phiên bản nâng cấp của Chuck Taylor All Star Classic, với chất liệu canvas dày dặn hơn, đế bóng hơn và lót giày êm ái hơn. Mang đậm chất vintage và độ bền cao hơn.", "Unisex", true, "Converse Chuck 70", 2000000.00m, 0 },
                    { 14, "Thiết kế không dây tiện lợi với họa tiết caro đen trắng kinh điển. Thân giày canvas, cổ giày có đệm. Biểu tượng của văn hóa trượt ván và sự thoải mái.", "Unisex", true, "Vans Slip-On Checkerboard", 1305000.00m, 0 },
                    { 15, "Mẫu giày bóng rổ cổ điển từ cuối những năm 80, được tái sinh và trở thành hiện tượng. Thiết kế da trơn, form dáng gọn gàng. Mang phong cách retro thể thao.", "Unisex", true, "New Balance 550", 1095000.00m, 0 },
                    { 16, "Mẫu giày da lộn kinh điển của Puma từ năm 1968. Thiết kế đơn giản với formstripe đặc trưng. Gắn liền với văn hóa hip-hop và b-boy.", "Unisex", true, "Puma Suede Classic", 1500000.00m, 0 },
                    { 17, "Thiết kế bóng rổ cổ điển với kiểu dáng low-top. Thân giày da, dấu Swoosh lớn. Mang vẻ đẹp vintage, dễ phối đồ.", "Unisex", true, "Nike Blazer Low '77", 3239000.00m, 0 },
                    { 18, "Mẫu giày bóng rổ từ những năm 80, với chi tiết quai dán đặc trưng ở cổ chân (phiên bản Mid/High) hoặc thiết kế cổ thấp gọn gàng.", "Unisex", true, "Adidas Forum Low", 2590000.00m, 0 },
                    { 19, "Lấy cảm hứng từ các mẫu giày chạy bộ thập niên 70 của New Balance. Thiết kế độc đáo với logo \"N\" lớn và đế ngoài răng cưa kéo dài.", "Unisex", true, "New Balance 327", 2200000.00m, 0 },
                    { 20, "Mẫu giày chunky phổ biến với thiết kế nhiều lớp và đế dày. Công nghệ đệm Air-Cooled Memory Foam mang lại sự thoải mái. Phong cách năng động, trẻ trung.", "Female", true, "Skechers D'Lites", 1500000.00m, 0 },
                    { 21, "Giày chunky đến từ thương hiệu thời trang Hàn Quốc, nổi bật với logo của các đội bóng chày MLB. Đế giày cao và hầm hố.", "Unisex", true, "MLB BigBall Chunky", 1800000.00m, 0 },
                    { 22, "Thuộc dòng Urbas, sử dụng chất liệu nhung gân (corduroy) độc đáo với 5 gam màu lấy cảm hứng từ mùa thu. Thiết kế vulcanized, phong cách vintage, phi giới tính.", "Unisex", true, "Ananas Urbas Corluray Pack", 580000.00m, 0 },
                    { 23, "Nổi bật với phần đệm Air lớn nhất ở gót chân (270 độ). Thân giày vải dệt kim thoáng khí. Thiết kế hiện đại, mang lại sự thoải mái tối đa cho việc đi lại hàng ngày.", "Unisex", true, "Nike Air Max 270", 3000000.00m, 0 },
                    { 24, "Kết hợp giữa thiết kế hiện đại và công nghệ Boost êm ái. Các miếng nhựa EVA đặc trưng ở đế giữa. Thân giày Primeknit hoặc vải lưới. Nhẹ nhàng và thoải mái.", "Unisex", true, "Adidas NMD_R1", 2090000.00m, 0 },
                    { 25, "Mẫu giày đầu tiên của Vans, thiết kế đơn giản, cổ thấp, dây buộc. Thân giày canvas, đế bánh quế. Một lựa chọn cơ bản và linh hoạt.", "Unisex", true, "Vans Authentic", 1087500.00m, 0 },
                    { 26, "Thiết kế chunky lấy cảm hứng từ dòng 99X và các mẫu giày chạy bộ đầu những năm 2000. Đế giữa ABZORB và SBS êm ái. Kiểu dáng độc đáo, phá cách.", "Unisex", true, "New Balance 9060", 1479000.00m, 0 },
                    { 27, "Dòng giày chunky lấy cảm hứng từ công nghệ Running System (RS) thập niên 80. Thiết kế nhiều lớp, màu sắc nổi bật, đế giày dày và êm.", "Unisex", true, "Puma RS-X Series", 1590000.00m, 0 },
                    { 28, "Dòng giày đường phố của Biti's Hunter, thiết kế trẻ trung, năng động, dễ phối đồ. Tập trung vào sự thoải mái và tính ứng dụng hàng ngày.", "Unisex", true, "Biti's Hunter Street", 679000.00m, 0 },
                    { 29, "Thuộc dòng Vintas, mang phong cách retro của những năm 2000. Thiết kế đơn giản, sử dụng chất liệu canvas và da lộn.", "Unisex", true, "Ananas Vintas Public 2000s", 620000.00m, 0 },
                    { 30, "Xuất thân là giày bóng rổ, trở thành biểu tượng thời trang đường phố. Thiết kế cổ thấp, nhiều phối màu đa dạng và các phiên bản collab.", "Unisex", true, "Nike Dunk Low", 2750000.00m, 0 },
                    { 31, "Mẫu giày training cổ điển từ những năm 60. Thân giày da lộn mềm mại, đế cao su. Thiết kế thanh lịch, gọn gàng.", "Unisex", true, "Adidas Gazelle", 2000000.00m, 0 },
                    { 32, "Biến thể hiện đại của Chuck Taylor với đế chunky răng cưa độc đáo. Thân giày canvas quen thuộc. Tạo điểm nhấn cá tính cho phong cách.", "Unisex", true, "Converse Run Star Hike", 2500000.00m, 0 },
                    { 33, "Phiên bản cổ cao của Old Skool, tăng cường bảo vệ mắt cá chân. Sọc jazz đặc trưng, chất liệu canvas và da lộn. Gắn liền với văn hóa trượt ván.", "Unisex", true, "Vans Sk8-Hi", 1755000.00m, 0 },
                    { 34, "Lấy cảm hứng từ giày chạy bộ cao cấp những năm 2000. Đế giữa ABZORB và N-ergy êm ái. Thiết kế kỹ thuật, chất liệu cao cấp.", "Unisex", true, "New Balance 2002R", 3000000.00m, 0 },
                    { 35, "Lấy cảm hứng từ mẫu giày California cổ điển. Thiết kế đế platform nhẹ nhàng, nữ tính. Thân giày da. Phong cách thoải mái, phóng khoáng.", "Female", true, "Puma Cali", 1800000.00m, 0 },
                    { 36, "Phiên bản cổ thấp của huyền thoại Air Jordan 1. Thiết kế mang tính biểu tượng, nhiều phối màu lịch sử và mới mẻ. Chất liệu da.", "Unisex", true, "Nike Air Jordan 1 Low", 3190000.00m, 0 },
                    { 37, "Thiết kế hiện đại, tập trung vào sự thoải mái và đa năng. Đệm Bounce linh hoạt. Thân giày Forgedmesh hoặc Engineered Mesh.", "Male", true, "Adidas Alphabounce", 2000000.00m, 0 },
                    { 38, "Mẫu giày chạy bộ cổ điển từ những năm 90, nổi tiếng với thiết kế lưỡi gà chẻ đôi (split tongue). Công nghệ đệm GEL êm ái.", "Unisex", true, "Asics GEL-Lyte III", 2500000.00m, 0 },
                    { 39, "Mẫu giày chunky lấy cảm hứng từ thập niên 90. Thiết kế nhiều lớp chất liệu, đế giày hầm hố. Phối màu đa dạng, trẻ trung.", "Unisex", true, "Fila Ray Tracer", 1500000.00m, 0 },
                    { 40, "Các dòng giày chạy bộ của Biti's Hunter, tập trung vào hiệu năng và sự thoải mái. Công nghệ đế Phylon hoặc LiteFlex.", "Unisex", true, "Biti's Hunter Jogging/Running", 881000.00m, 0 },
                    { 41, "Dòng giày Pattas với thiết kế tối giản, tập trung vào câu chuyện và thông điệp. Thân giày canvas, đế vulcanized.", "Unisex", true, "Ananas Pattas \"Simple Story\"", 500000.00m, 0 },
                    { 42, "Dòng Air Max mới nhất với công nghệ Dynamic Air (các ống Air có áp suất kép). Mang lại cảm giác chuyển động mượt mà và năng động.", "Unisex", true, "Nike Air Max Dn", 3500000.00m, 0 },
                    { 43, "Mẫu giày sân cỏ trong nhà (indoor football) kinh điển. Thân giày da hoặc da lộn, đế gum. Thiết kế vượt thời gian, đang rất thịnh hành trở lại.", "Unisex", true, "Adidas Samba OG", 2290000.00m, 0 },
                    { 44, "Phiên bản hiện đại của Chuck Taylor với đế platform nhẹ và cao. Mang lại vẻ ngoài năng động và tôn dáng.", "Female", true, "Converse Chuck Taylor All Star Move", 1800000.00m, 0 },
                    { 45, "Lấy cảm hứng từ giày trượt ván thập niên 90, với thiết kế \"phồng\" hơn, lưỡi gà dày và sọc jazz 3D nổi. Mang phong cách chunky retro.", "Unisex", true, "Vans Knu Skool", 1000000.00m, 0 },
                    { 46, "Mẫu giày chạy bộ cổ điển từ những năm 90, nay trở thành item thời trang được yêu thích. Đế giữa ABZORB êm ái. Thân giày lưới và da tổng hợp.", "Unisex", true, "New Balance 530", 2490000.00m, 0 },
                    { 47, "Thiết kế đế platform cá tính, lấy cảm hứng từ sự nhộn nhịp đô thị. Dành cho những cô gái yêu thích phong cách thời trang đường phố. Thân giày da.", "Female", true, "Puma Mayze Lth", 2000000.00m, 0 },
                    { 48, "Mẫu giày chạy bộ đầu những năm 2000, nay trở lại với phong cách retro-tech. Đệm Zoom Air và Cushlon êm ái. Thiết kế nhiều lớp, chi tiết.", "Unisex", true, "Nike Zoom Vomero 5", 3500000.00m, 0 },
                    { 49, "Lấy cảm hứng từ Adidas Campus cổ điển nhưng với tỷ lệ phóng đại, mang hơi hướng giày skate thập niên 2000. Thân giày da lộn, dây giày bản to.", "Unisex", true, "Adidas Campus 00s", 2500000.00m, 0 },
                    { 50, "Phiên bản nâng cấp của dòng Core với công nghệ đế LiteFoam 3.0 cải tiến, tăng độ êm và nhẹ. Thiết kế đa năng, phù hợp nhiều hoạt động.", "Unisex", true, "Biti's Hunter Core LiteFoam 3.0", 843000.00m, 0 }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryID", "CategoryType", "IsPromotion", "Name", "ParentID" },
                values: new object[,]
                {
                    { 3, "Product", false, "Giày Lifestyle Kinh Điển", 1 },
                    { 4, "Product", false, "Giày Retro & Di Sản", 1 },
                    { 5, "Product", false, "Giày Chạy Bộ & Công Nghệ", 1 },
                    { 6, "Product", false, "Giày Chunky & Cá Tính", 1 },
                    { 7, "Product", false, "Giày Local Brand Đột Phá", 1 },
                    { 8, "Brand", false, "Nike", 2 },
                    { 9, "Brand", false, "Adidas", 2 },
                    { 10, "Brand", false, "Converse", 2 },
                    { 11, "Brand", false, "Vans", 2 },
                    { 12, "Brand", false, "New Balance", 2 },
                    { 13, "Brand", false, "Fila", 2 },
                    { 14, "Brand", false, "Biti's", 2 },
                    { 15, "Brand", false, "Ananas", 2 },
                    { 16, "Brand", false, "Puma", 2 },
                    { 17, "Brand", false, "Skechers", 2 },
                    { 18, "Brand", false, "MLB", 2 },
                    { 19, "Brand", false, "Asics", 2 }
                });

            migrationBuilder.InsertData(
                table: "ProductSizes",
                columns: new[] { "ProductID", "Size", "QuantityInStock" },
                values: new object[,]
                {
                    { 1, 36, 25 },
                    { 1, 37, 30 },
                    { 1, 38, 45 },
                    { 1, 39, 50 },
                    { 1, 40, 50 },
                    { 1, 41, 40 },
                    { 1, 42, 35 },
                    { 1, 43, 20 },
                    { 2, 38, 15 },
                    { 2, 39, 25 },
                    { 2, 40, 30 },
                    { 2, 41, 28 },
                    { 2, 42, 22 },
                    { 3, 36, 50 },
                    { 3, 37, 50 },
                    { 3, 38, 50 },
                    { 3, 39, 40 },
                    { 3, 40, 40 },
                    { 4, 38, 22 },
                    { 4, 39, 32 },
                    { 4, 40, 35 },
                    { 4, 41, 30 },
                    { 5, 39, 18 },
                    { 5, 40, 25 },
                    { 5, 41, 24 },
                    { 5, 42, 20 },
                    { 6, 38, 28 },
                    { 6, 39, 35 },
                    { 6, 40, 40 },
                    { 6, 41, 33 },
                    { 6, 42, 25 },
                    { 7, 37, 20 },
                    { 7, 38, 25 },
                    { 7, 39, 30 },
                    { 7, 40, 35 },
                    { 7, 41, 30 },
                    { 8, 36, 40 },
                    { 8, 37, 45 },
                    { 8, 38, 35 },
                    { 8, 39, 30 },
                    { 9, 37, 60 },
                    { 9, 38, 70 },
                    { 9, 39, 80 },
                    { 9, 40, 75 },
                    { 9, 41, 65 },
                    { 9, 42, 55 },
                    { 10, 38, 40 },
                    { 10, 39, 50 },
                    { 10, 40, 60 },
                    { 10, 41, 55 },
                    { 10, 42, 45 },
                    { 11, 37, 20 },
                    { 11, 38, 22 },
                    { 11, 39, 25 },
                    { 11, 40, 23 },
                    { 11, 41, 18 },
                    { 12, 36, 30 },
                    { 12, 37, 35 },
                    { 12, 38, 40 },
                    { 12, 40, 38 },
                    { 12, 42, 33 },
                    { 13, 37, 25 },
                    { 13, 38, 30 },
                    { 13, 39, 35 },
                    { 13, 41, 30 },
                    { 13, 42, 25 },
                    { 14, 36, 40 },
                    { 14, 38, 45 },
                    { 14, 39, 50 },
                    { 14, 40, 48 },
                    { 15, 40, 20 },
                    { 15, 41, 25 },
                    { 15, 42, 30 },
                    { 15, 43, 28 },
                    { 15, 44, 20 },
                    { 16, 39, 25 },
                    { 16, 40, 30 },
                    { 16, 41, 28 },
                    { 17, 38, 15 },
                    { 17, 39, 18 },
                    { 17, 40, 20 },
                    { 18, 40, 25 },
                    { 18, 41, 30 },
                    { 18, 42, 28 },
                    { 19, 37, 20 },
                    { 19, 38, 25 },
                    { 19, 40, 30 },
                    { 20, 36, 30 },
                    { 20, 37, 35 },
                    { 20, 38, 40 },
                    { 21, 37, 25 },
                    { 21, 38, 30 },
                    { 21, 39, 28 },
                    { 22, 39, 50 },
                    { 22, 40, 60 },
                    { 22, 41, 55 },
                    { 23, 40, 20 },
                    { 23, 41, 25 },
                    { 23, 42, 22 },
                    { 24, 38, 28 },
                    { 24, 39, 32 },
                    { 24, 40, 30 },
                    { 25, 36, 50 },
                    { 25, 37, 55 },
                    { 25, 38, 60 },
                    { 26, 41, 15 },
                    { 26, 42, 20 },
                    { 26, 43, 18 },
                    { 27, 38, 22 },
                    { 27, 39, 25 },
                    { 27, 40, 20 },
                    { 28, 37, 40 },
                    { 28, 38, 50 },
                    { 28, 39, 45 },
                    { 29, 39, 60 },
                    { 29, 40, 70 },
                    { 29, 41, 65 },
                    { 30, 38, 30 },
                    { 30, 39, 35 },
                    { 30, 40, 40 },
                    { 30, 42, 30 },
                    { 31, 37, 20 },
                    { 31, 38, 25 },
                    { 31, 39, 22 },
                    { 32, 36, 15 },
                    { 32, 37, 20 },
                    { 32, 38, 25 },
                    { 33, 39, 30 },
                    { 33, 40, 35 },
                    { 33, 41, 40 },
                    { 34, 40, 18 },
                    { 34, 41, 20 },
                    { 34, 42, 25 },
                    { 35, 36, 25 },
                    { 35, 37, 30 },
                    { 35, 38, 28 },
                    { 36, 39, 15 },
                    { 36, 40, 20 },
                    { 36, 41, 18 },
                    { 37, 42, 30 },
                    { 37, 43, 35 },
                    { 37, 44, 25 },
                    { 38, 38, 18 },
                    { 38, 39, 20 },
                    { 38, 40, 22 },
                    { 39, 37, 25 },
                    { 39, 38, 30 },
                    { 39, 39, 28 },
                    { 40, 39, 40 },
                    { 40, 40, 50 },
                    { 40, 41, 45 },
                    { 41, 38, 60 },
                    { 41, 39, 70 },
                    { 41, 40, 65 },
                    { 42, 40, 15 },
                    { 42, 41, 18 },
                    { 42, 42, 20 },
                    { 43, 38, 25 },
                    { 43, 39, 30 },
                    { 43, 40, 35 },
                    { 44, 36, 20 },
                    { 44, 37, 25 },
                    { 44, 38, 30 },
                    { 45, 38, 28 },
                    { 45, 39, 35 },
                    { 45, 40, 30 },
                    { 46, 37, 18 },
                    { 46, 38, 22 },
                    { 46, 39, 25 },
                    { 47, 36, 30 },
                    { 47, 37, 35 },
                    { 47, 38, 30 },
                    { 48, 40, 20 },
                    { 48, 41, 25 },
                    { 48, 42, 22 },
                    { 49, 38, 28 },
                    { 49, 39, 30 },
                    { 49, 40, 35 },
                    { 50, 39, 45 },
                    { 50, 40, 55 },
                    { 50, 41, 50 }
                });

            migrationBuilder.InsertData(
                table: "ProductCategories",
                columns: new[] { "CategoryID", "ProductID" },
                values: new object[,]
                {
                    { 3, 1 },
                    { 8, 1 },
                    { 3, 2 },
                    { 4, 2 },
                    { 9, 2 },
                    { 3, 3 },
                    { 10, 3 },
                    { 3, 4 },
                    { 11, 4 },
                    { 3, 5 },
                    { 4, 5 },
                    { 12, 5 },
                    { 4, 6 },
                    { 5, 6 },
                    { 8, 6 },
                    { 5, 7 },
                    { 9, 7 },
                    { 6, 8 },
                    { 13, 8 },
                    { 3, 9 },
                    { 7, 9 },
                    { 14, 9 },
                    { 3, 10 },
                    { 7, 10 },
                    { 15, 10 },
                    { 3, 11 },
                    { 4, 11 },
                    { 8, 11 },
                    { 3, 12 },
                    { 9, 12 },
                    { 3, 13 },
                    { 4, 13 },
                    { 10, 13 },
                    { 3, 14 },
                    { 11, 14 },
                    { 3, 15 },
                    { 4, 15 },
                    { 12, 15 },
                    { 3, 16 },
                    { 4, 16 },
                    { 16, 16 },
                    { 3, 17 },
                    { 4, 17 },
                    { 8, 17 },
                    { 3, 18 },
                    { 4, 18 },
                    { 9, 18 },
                    { 4, 19 },
                    { 5, 19 },
                    { 12, 19 },
                    { 6, 20 },
                    { 17, 20 },
                    { 6, 21 },
                    { 18, 21 },
                    { 4, 22 },
                    { 7, 22 },
                    { 15, 22 },
                    { 3, 23 },
                    { 5, 23 },
                    { 8, 23 },
                    { 3, 24 },
                    { 5, 24 },
                    { 9, 24 },
                    { 3, 25 },
                    { 11, 25 },
                    { 4, 26 },
                    { 6, 26 },
                    { 12, 26 },
                    { 4, 27 },
                    { 6, 27 },
                    { 16, 27 },
                    { 3, 28 },
                    { 7, 28 },
                    { 14, 28 },
                    { 4, 29 },
                    { 7, 29 },
                    { 15, 29 },
                    { 3, 30 },
                    { 4, 30 },
                    { 8, 30 },
                    { 3, 31 },
                    { 4, 31 },
                    { 9, 31 },
                    { 3, 32 },
                    { 6, 32 },
                    { 10, 32 },
                    { 3, 33 },
                    { 11, 33 },
                    { 4, 34 },
                    { 5, 34 },
                    { 12, 34 },
                    { 3, 35 },
                    { 16, 35 },
                    { 3, 36 },
                    { 4, 36 },
                    { 8, 36 },
                    { 3, 37 },
                    { 5, 37 },
                    { 9, 37 },
                    { 4, 38 },
                    { 5, 38 },
                    { 19, 38 },
                    { 4, 39 },
                    { 6, 39 },
                    { 13, 39 },
                    { 5, 40 },
                    { 7, 40 },
                    { 14, 40 },
                    { 3, 41 },
                    { 7, 41 },
                    { 15, 41 },
                    { 5, 42 },
                    { 8, 42 },
                    { 3, 43 },
                    { 4, 43 },
                    { 9, 43 },
                    { 3, 44 },
                    { 6, 44 },
                    { 10, 44 },
                    { 3, 45 },
                    { 6, 45 },
                    { 11, 45 },
                    { 4, 46 },
                    { 5, 46 },
                    { 12, 46 },
                    { 3, 47 },
                    { 6, 47 },
                    { 16, 47 },
                    { 4, 48 },
                    { 5, 48 },
                    { 8, 48 },
                    { 3, 49 },
                    { 4, 49 },
                    { 9, 49 },
                    { 3, 50 },
                    { 5, 50 },
                    { 7, 50 },
                    { 14, 50 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserID",
                table: "Addresses",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductID",
                table: "CartItems",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentID",
                table: "Categories",
                column: "ParentID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductID",
                table: "OrderDetails",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ApprovedByAdminID",
                table: "Orders",
                column: "ApprovedByAdminID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserID",
                table: "Orders",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CategoryID",
                table: "ProductCategories",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_ApplicableCategoryId",
                table: "Promotions",
                column: "ApplicableCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_ApplicableProductId",
                table: "Promotions",
                column: "ApplicableProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "ProductSizes");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
