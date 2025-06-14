using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Project1_VTCA.Data
{
    public class SneakerShopDbContext : DbContext
    {
        public SneakerShopDbContext(DbContextOptions<SneakerShopDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Promotion> Promotions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Gọi lại phương thức gốc để nó vẫn chạy các cấu hình mặc định
            base.OnModelCreating(modelBuilder);

            // --- CẤU HÌNH KHÓA PHỨC HỢP VÀ MỐI QUAN HỆ (giữ nguyên như cũ) ---
            modelBuilder.Entity<ProductCategory>().HasKey(pc => new { pc.ProductID, pc.CategoryID });
            modelBuilder.Entity<ProductSize>().HasKey(ps => new { ps.ProductID, ps.Size });
            modelBuilder.Entity<CartItem>().HasKey(ci => new { ci.UserID, ci.ProductID, ci.Size });
            modelBuilder.Entity<OrderDetail>().HasKey(od => new { od.OrderID, od.ProductID, od.Size });

            modelBuilder.Entity<User>()
                .HasMany(u => u.ApprovedOrders)
                .WithOne(o => o.ApprovedByAdmin)
                .HasForeignKey(o => o.ApprovedByAdminID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserID)
                .OnDelete(DeleteBehavior.Restrict);


            // --- PHẦN CHÈN DỮ LIỆU MẪU (DATA SEEDING) ---

            // Dữ liệu cho bảng Categories
            modelBuilder.Entity<Category>().HasData(


                new Category { CategoryID = 1, Name = "Phong cách", ParentID = null, CategoryType = "Product", IsPromotion = false },
                new Category { CategoryID = 2, Name = "Thương hiệu", ParentID = null, CategoryType = "Brand", IsPromotion = false },



                new Category { CategoryID = 3, Name = "Giày Lifestyle Kinh Điển", ParentID = 1, CategoryType = "Product", IsPromotion = false },
                new Category { CategoryID = 4, Name = "Giày Retro & Di Sản", ParentID = 1, CategoryType = "Product", IsPromotion = false },
                new Category { CategoryID = 5, Name = "Giày Chạy Bộ & Công Nghệ", ParentID = 1, CategoryType = "Product", IsPromotion = false },
                new Category { CategoryID = 6, Name = "Giày Chunky & Cá Tính", ParentID = 1, CategoryType = "Product", IsPromotion = false },
                new Category { CategoryID = 7, Name = "Giày Local Brand Đột Phá", ParentID = 1, CategoryType = "Product", IsPromotion = false },



                new Category { CategoryID = 8, Name = "Nike", ParentID = 2, CategoryType = "Brand", IsPromotion = false },
                new Category { CategoryID = 9, Name = "Adidas", ParentID = 2, CategoryType = "Brand", IsPromotion = false },
                new Category { CategoryID = 10, Name = "Converse", ParentID = 2, CategoryType = "Brand", IsPromotion = false },
                new Category { CategoryID = 11, Name = "Vans", ParentID = 2, CategoryType = "Brand", IsPromotion = false },
                new Category { CategoryID = 12, Name = "New Balance", ParentID = 2, CategoryType = "Brand", IsPromotion = false },
                new Category { CategoryID = 13, Name = "Fila", ParentID = 2, CategoryType = "Brand", IsPromotion = false },
                new Category { CategoryID = 14, Name = "Biti's", ParentID = 2, CategoryType = "Brand", IsPromotion = false },
                new Category { CategoryID = 15, Name = "Ananas", ParentID = 2, CategoryType = "Brand", IsPromotion = false },
                new Category { CategoryID = 16, Name = "Puma", ParentID = 2, CategoryType = "Brand", IsPromotion = false },
                new Category { CategoryID = 17, Name = "Skechers", ParentID = 2, CategoryType = "Brand", IsPromotion = false },
                new Category { CategoryID = 18, Name = "MLB", ParentID = 2, CategoryType = "Brand", IsPromotion = false },
                new Category { CategoryID = 19, Name = "Asics", ParentID = 2, CategoryType = "Brand", IsPromotion = false }
                );

            //SEED dữ liệu 50 sản phẩm


            #region Seed Products
            // --- Sản phẩm 1: Nike Air Force 1 '07 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 1, Name = "Nike Air Force 1 '07", Description = "Biểu tượng bất hủ của Nike, thiết kế da trơn cổ điển, đệm Air-Sole êm ái. Phù hợp mọi phong cách, dễ phối đồ, độ bền cao.", Price = 2290000.00m, GenderApplicability = "Unisex", TotalQuantity = 515, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 1, CategoryID = 3 }, new ProductCategory { ProductID = 1, CategoryID = 8 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 1, Size = 36, QuantityInStock = 25 }, new ProductSize { ProductID = 1, Size = 37, QuantityInStock = 30 }, new ProductSize { ProductID = 1, Size = 38, QuantityInStock = 45 }, new ProductSize { ProductID = 1, Size = 39, QuantityInStock = 50 }, new ProductSize { ProductID = 1, Size = 40, QuantityInStock = 50 }, new ProductSize { ProductID = 1, Size = 41, QuantityInStock = 40 }, new ProductSize { ProductID = 1, Size = 42, QuantityInStock = 35 }, new ProductSize { ProductID = 1, Size = 43, QuantityInStock = 20 });

            // --- Sản phẩm 2: Adidas Superstar ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 2, Name = "Adidas Superstar", Description = "Thiết kế mũi vỏ sò (shell-toe) đặc trưng và 3 sọc răng cưa kinh điển. Một trong những đôi sneaker có ảnh hưởng nhất mọi thời đại, dễ nhận diện.", Price = 1890000.00m, GenderApplicability = "Unisex", TotalQuantity = 525, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 2, CategoryID = 3 }, new ProductCategory { ProductID = 2, CategoryID = 4 }, new ProductCategory { ProductID = 2, CategoryID = 9 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 2, Size = 38, QuantityInStock = 15 }, new ProductSize { ProductID = 2, Size = 39, QuantityInStock = 25 }, new ProductSize { ProductID = 2, Size = 40, QuantityInStock = 30 }, new ProductSize { ProductID = 2, Size = 41, QuantityInStock = 28 }, new ProductSize { ProductID = 2, Size = 42, QuantityInStock = 22 });

            // --- Sản phẩm 3: Converse Chuck Taylor All Star Classic ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 3, Name = "Converse Chuck Taylor All Star Classic", Description = "Phiên bản cổ cao kinh điển với thân giày bằng vải canvas bền chắc, đế cao su lưu hóa. Biểu tượng của sự trẻ trung, cá tính và tự do.", Price = 1395000.00m, GenderApplicability = "Unisex", TotalQuantity = 535, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 3, CategoryID = 3 }, new ProductCategory { ProductID = 3, CategoryID = 10 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 3, Size = 36, QuantityInStock = 50 }, new ProductSize { ProductID = 3, Size = 37, QuantityInStock = 50 }, new ProductSize { ProductID = 3, Size = 38, QuantityInStock = 50 }, new ProductSize { ProductID = 3, Size = 39, QuantityInStock = 40 }, new ProductSize { ProductID = 3, Size = 40, QuantityInStock = 40 });

            // --- Sản phẩm 4: Vans Old Skool Classic ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 4, Name = "Vans Old Skool Classic", Description = "Thiết kế cổ thấp với sọc jazz đặc trưng của Vans bên hông giày. Chất liệu canvas và da lộn kết hợp. Được yêu thích trong cộng đồng skate và thời trang đường phố.", Price = 1480000.00m, GenderApplicability = "Unisex", TotalQuantity = 555, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 4, CategoryID = 3 }, new ProductCategory { ProductID = 4, CategoryID = 11 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 4, Size = 38, QuantityInStock = 22 }, new ProductSize { ProductID = 4, Size = 39, QuantityInStock = 32 }, new ProductSize { ProductID = 4, Size = 40, QuantityInStock = 35 }, new ProductSize { ProductID = 4, Size = 41, QuantityInStock = 30 });

            // --- Sản phẩm 5: New Balance 574 Core ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 5, Name = "New Balance 574 Core", Description = "Dáng giày chạy bộ cổ điển từ những năm 80. Công nghệ ENCAP ở đế giữa giúp tăng cường độ ổn định. Thân giày kết hợp da lộn và vải lưới thoáng khí. Mang đậm phong cách vintage.", Price = 779000.00m, GenderApplicability = "Unisex", TotalQuantity = 565, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 5, CategoryID = 4 }, new ProductCategory { ProductID = 5, CategoryID = 3 }, new ProductCategory { ProductID = 5, CategoryID = 12 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 5, Size = 39, QuantityInStock = 18 }, new ProductSize { ProductID = 5, Size = 40, QuantityInStock = 25 }, new ProductSize { ProductID = 5, Size = 41, QuantityInStock = 24 }, new ProductSize { ProductID = 5, Size = 42, QuantityInStock = 20 });

            // --- Sản phẩm 6: Nike Air Max 90 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 6, Name = "Nike Air Max 90", Description = "Thiết kế mang tính cách mạng với cửa sổ Air có thể nhìn thấy ở gót chân. Kiểu dáng mạnh mẽ, năng động. Một trong những dòng Air Max được yêu thích nhất.", Price = 2800000.00m, GenderApplicability = "Unisex", TotalQuantity = 575, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 6, CategoryID = 5 }, new ProductCategory { ProductID = 6, CategoryID = 4 }, new ProductCategory { ProductID = 6, CategoryID = 8 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 6, Size = 38, QuantityInStock = 28 }, new ProductSize { ProductID = 6, Size = 39, QuantityInStock = 35 }, new ProductSize { ProductID = 6, Size = 40, QuantityInStock = 40 }, new ProductSize { ProductID = 6, Size = 41, QuantityInStock = 33 }, new ProductSize { ProductID = 6, Size = 42, QuantityInStock = 25 });

            // --- Sản phẩm 7: Adidas Ultra Boost ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 7, Name = "Adidas Ultra Boost", Description = "Nổi tiếng với công nghệ đệm Boost mang lại sự êm ái và hoàn trả năng lượng vượt trội. Thân giày Primeknit ôm sát chân. Phù hợp cho cả chạy bộ và thời trang hàng ngày.", Price = 2500000.00m, GenderApplicability = "Unisex", TotalQuantity = 585, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 7, CategoryID = 5 }, new ProductCategory { ProductID = 7, CategoryID = 9 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 7, Size = 37, QuantityInStock = 20 }, new ProductSize { ProductID = 7, Size = 38, QuantityInStock = 25 }, new ProductSize { ProductID = 7, Size = 39, QuantityInStock = 30 }, new ProductSize { ProductID = 7, Size = 40, QuantityInStock = 35 }, new ProductSize { ProductID = 7, Size = 41, QuantityInStock = 30 });

            // --- Sản phẩm 8: Fila Disruptor 2 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 8, Name = "Fila Disruptor 2", Description = "Mẫu giày chunky \"đình đám\" với đế ngoài răng cưa hầm hố. Thiết kế logo Fila nổi bật. Tạo nên phong cách thời trang cá tính và thu hút.", Price = 1500000.00m, GenderApplicability = "Female", TotalQuantity = 280, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 8, CategoryID = 6 }, new ProductCategory { ProductID = 8, CategoryID = 13 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 8, Size = 36, QuantityInStock = 40 }, new ProductSize { ProductID = 8, Size = 37, QuantityInStock = 45 }, new ProductSize { ProductID = 8, Size = 38, QuantityInStock = 35 }, new ProductSize { ProductID = 8, Size = 39, QuantityInStock = 30 });

            // --- Sản phẩm 9: Biti's Hunter X ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 9, Name = "Biti's Hunter X", Description = "Dòng sneaker thành công của Biti's, thiết kế trẻ trung. Công nghệ đế LiteFlex siêu nhẹ và êm. Nhiều phiên bản màu sắc và collab độc đáo.", Price = 767000.00m, GenderApplicability = "Unisex", TotalQuantity = 605, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 9, CategoryID = 7 }, new ProductCategory { ProductID = 9, CategoryID = 3 }, new ProductCategory { ProductID = 9, CategoryID = 14 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 9, Size = 37, QuantityInStock = 60 }, new ProductSize { ProductID = 9, Size = 38, QuantityInStock = 70 }, new ProductSize { ProductID = 9, Size = 39, QuantityInStock = 80 }, new ProductSize { ProductID = 9, Size = 40, QuantityInStock = 75 }, new ProductSize { ProductID = 9, Size = 41, QuantityInStock = 65 }, new ProductSize { ProductID = 9, Size = 42, QuantityInStock = 55 });

            // --- Sản phẩm 10: Ananas Basas Bumper Gum ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 10, Name = "Ananas Basas Bumper Gum", Description = "Thiết kế vulcanized cổ điển, tối giản. Thân giày canvas, đế gum đặc trưng. Được giới trẻ yêu thích vì sự đơn giản và chất lượng.", Price = 580000.00m, GenderApplicability = "Unisex", TotalQuantity = 615, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 10, CategoryID = 7 }, new ProductCategory { ProductID = 10, CategoryID = 3 }, new ProductCategory { ProductID = 10, CategoryID = 15 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 10, Size = 38, QuantityInStock = 40 }, new ProductSize { ProductID = 10, Size = 39, QuantityInStock = 50 }, new ProductSize { ProductID = 10, Size = 40, QuantityInStock = 60 }, new ProductSize { ProductID = 10, Size = 41, QuantityInStock = 55 }, new ProductSize { ProductID = 10, Size = 42, QuantityInStock = 45 });

            // --- Sản phẩm 11: Nike Cortez ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 11, Name = "Nike Cortez", Description = "Mẫu giày chạy bộ biểu tượng từ những năm 70. Thiết kế đơn giản, thanh lịch với dấu Swoosh lớn. Đế giữa EVA nhẹ, đế ngoài xương cá tăng độ bám.", Price = 1800000.00m, GenderApplicability = "Unisex", TotalQuantity = 625, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 11, CategoryID = 3 }, new ProductCategory { ProductID = 11, CategoryID = 4 }, new ProductCategory { ProductID = 11, CategoryID = 8 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 11, Size = 37, QuantityInStock = 20 }, new ProductSize { ProductID = 11, Size = 38, QuantityInStock = 22 }, new ProductSize { ProductID = 11, Size = 39, QuantityInStock = 25 }, new ProductSize { ProductID = 11, Size = 40, QuantityInStock = 23 }, new ProductSize { ProductID = 11, Size = 41, QuantityInStock = 18 });

            // --- Sản phẩm 12: Adidas Stan Smith ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 12, Name = "Adidas Stan Smith", Description = "Thiết kế tennis cổ điển, tối giản với 3 hàng lỗ thoáng khí thay cho 3 sọc. Gót giày và lưỡi gà có logo Stan Smith. Một lựa chọn thanh lịch và không bao giờ lỗi mốt.", Price = 2290000.00m, GenderApplicability = "Unisex", TotalQuantity = 635, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 12, CategoryID = 3 }, new ProductCategory { ProductID = 12, CategoryID = 9 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 12, Size = 36, QuantityInStock = 30 }, new ProductSize { ProductID = 12, Size = 37, QuantityInStock = 35 }, new ProductSize { ProductID = 12, Size = 38, QuantityInStock = 40 }, new ProductSize { ProductID = 12, Size = 40, QuantityInStock = 38 }, new ProductSize { ProductID = 12, Size = 42, QuantityInStock = 33 });

            // --- Sản phẩm 13: Converse Chuck 70 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 13, Name = "Converse Chuck 70", Description = "Phiên bản nâng cấp của Chuck Taylor All Star Classic, với chất liệu canvas dày dặn hơn, đế bóng hơn và lót giày êm ái hơn. Mang đậm chất vintage và độ bền cao hơn.", Price = 2000000.00m, GenderApplicability = "Unisex", TotalQuantity = 645, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 13, CategoryID = 3 }, new ProductCategory { ProductID = 13, CategoryID = 4 }, new ProductCategory { ProductID = 13, CategoryID = 10 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 13, Size = 37, QuantityInStock = 25 }, new ProductSize { ProductID = 13, Size = 38, QuantityInStock = 30 }, new ProductSize { ProductID = 13, Size = 39, QuantityInStock = 35 }, new ProductSize { ProductID = 13, Size = 41, QuantityInStock = 30 }, new ProductSize { ProductID = 13, Size = 42, QuantityInStock = 25 });

            // --- Sản phẩm 14: Vans Slip-On Checkerboard ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 14, Name = "Vans Slip-On Checkerboard", Description = "Thiết kế không dây tiện lợi với họa tiết caro đen trắng kinh điển. Thân giày canvas, cổ giày có đệm. Biểu tượng của văn hóa trượt ván và sự thoải mái.", Price = 1305000.00m, GenderApplicability = "Unisex", TotalQuantity = 655, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 14, CategoryID = 3 }, new ProductCategory { ProductID = 14, CategoryID = 11 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 14, Size = 36, QuantityInStock = 40 }, new ProductSize { ProductID = 14, Size = 38, QuantityInStock = 45 }, new ProductSize { ProductID = 14, Size = 39, QuantityInStock = 50 }, new ProductSize { ProductID = 14, Size = 40, QuantityInStock = 48 });

            // --- Sản phẩm 15: New Balance 550 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 15, Name = "New Balance 550", Description = "Mẫu giày bóng rổ cổ điển từ cuối những năm 80, được tái sinh và trở thành hiện tượng. Thiết kế da trơn, form dáng gọn gàng. Mang phong cách retro thể thao.", Price = 1095000.00m, GenderApplicability = "Unisex", TotalQuantity = 665, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 15, CategoryID = 4 }, new ProductCategory { ProductID = 15, CategoryID = 3 }, new ProductCategory { ProductID = 15, CategoryID = 12 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 15, Size = 40, QuantityInStock = 20 }, new ProductSize { ProductID = 15, Size = 41, QuantityInStock = 25 }, new ProductSize { ProductID = 15, Size = 42, QuantityInStock = 30 }, new ProductSize { ProductID = 15, Size = 43, QuantityInStock = 28 }, new ProductSize { ProductID = 15, Size = 44, QuantityInStock = 20 });

            // --- Sản phẩm 16: Puma Suede Classic ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 16, Name = "Puma Suede Classic", Description = "Mẫu giày da lộn kinh điển của Puma từ năm 1968. Thiết kế đơn giản với formstripe đặc trưng. Gắn liền với văn hóa hip-hop và b-boy.", Price = 1500000.00m, GenderApplicability = "Unisex", TotalQuantity = 675, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 16, CategoryID = 3 }, new ProductCategory { ProductID = 16, CategoryID = 4 }, new ProductCategory { ProductID = 16, CategoryID = 16 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 16, Size = 39, QuantityInStock = 25 }, new ProductSize { ProductID = 16, Size = 40, QuantityInStock = 30 }, new ProductSize { ProductID = 16, Size = 41, QuantityInStock = 28 });

            // --- Sản phẩm 17: Nike Blazer Low '77 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 17, Name = "Nike Blazer Low '77", Description = "Thiết kế bóng rổ cổ điển với kiểu dáng low-top. Thân giày da, dấu Swoosh lớn. Mang vẻ đẹp vintage, dễ phối đồ.", Price = 3239000.00m, GenderApplicability = "Unisex", TotalQuantity = 685, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 17, CategoryID = 3 }, new ProductCategory { ProductID = 17, CategoryID = 4 }, new ProductCategory { ProductID = 17, CategoryID = 8 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 17, Size = 38, QuantityInStock = 15 }, new ProductSize { ProductID = 17, Size = 39, QuantityInStock = 18 }, new ProductSize { ProductID = 17, Size = 40, QuantityInStock = 20 });

            // --- Sản phẩm 18: Adidas Forum Low ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 18, Name = "Adidas Forum Low", Description = "Mẫu giày bóng rổ từ những năm 80, với chi tiết quai dán đặc trưng ở cổ chân (phiên bản Mid/High) hoặc thiết kế cổ thấp gọn gàng.", Price = 2590000.00m, GenderApplicability = "Unisex", TotalQuantity = 695, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 18, CategoryID = 4 }, new ProductCategory { ProductID = 18, CategoryID = 3 }, new ProductCategory { ProductID = 18, CategoryID = 9 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 18, Size = 40, QuantityInStock = 25 }, new ProductSize { ProductID = 18, Size = 41, QuantityInStock = 30 }, new ProductSize { ProductID = 18, Size = 42, QuantityInStock = 28 });

            // --- Sản phẩm 19: New Balance 327 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 19, Name = "New Balance 327", Description = "Lấy cảm hứng từ các mẫu giày chạy bộ thập niên 70 của New Balance. Thiết kế độc đáo với logo \"N\" lớn và đế ngoài răng cưa kéo dài.", Price = 2200000.00m, GenderApplicability = "Unisex", TotalQuantity = 705, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 19, CategoryID = 4 }, new ProductCategory { ProductID = 19, CategoryID = 5 }, new ProductCategory { ProductID = 19, CategoryID = 12 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 19, Size = 37, QuantityInStock = 20 }, new ProductSize { ProductID = 19, Size = 38, QuantityInStock = 25 }, new ProductSize { ProductID = 19, Size = 40, QuantityInStock = 30 });

            // --- Sản phẩm 20: Skechers D'Lites ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 20, Name = "Skechers D'Lites", Description = "Mẫu giày chunky phổ biến với thiết kế nhiều lớp và đế dày. Công nghệ đệm Air-Cooled Memory Foam mang lại sự thoải mái. Phong cách năng động, trẻ trung.", Price = 1500000.00m, GenderApplicability = "Female", TotalQuantity = 200, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 20, CategoryID = 6 }, new ProductCategory { ProductID = 20, CategoryID = 17 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 20, Size = 36, QuantityInStock = 30 }, new ProductSize { ProductID = 20, Size = 37, QuantityInStock = 35 }, new ProductSize { ProductID = 20, Size = 38, QuantityInStock = 40 });

            // --- Sản phẩm 21: MLB BigBall Chunky ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 21, Name = "MLB BigBall Chunky", Description = "Giày chunky đến từ thương hiệu thời trang Hàn Quốc, nổi bật với logo của các đội bóng chày MLB. Đế giày cao và hầm hố.", Price = 1800000.00m, GenderApplicability = "Unisex", TotalQuantity = 725, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 21, CategoryID = 6 }, new ProductCategory { ProductID = 21, CategoryID = 18 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 21, Size = 37, QuantityInStock = 25 }, new ProductSize { ProductID = 21, Size = 38, QuantityInStock = 30 }, new ProductSize { ProductID = 21, Size = 39, QuantityInStock = 28 });

            // --- Sản phẩm 22: Ananas Urbas Corluray Pack ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 22, Name = "Ananas Urbas Corluray Pack", Description = "Thuộc dòng Urbas, sử dụng chất liệu nhung gân (corduroy) độc đáo với 5 gam màu lấy cảm hứng từ mùa thu. Thiết kế vulcanized, phong cách vintage, phi giới tính.", Price = 580000.00m, GenderApplicability = "Unisex", TotalQuantity = 735, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 22, CategoryID = 7 }, new ProductCategory { ProductID = 22, CategoryID = 4 }, new ProductCategory { ProductID = 22, CategoryID = 15 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 22, Size = 39, QuantityInStock = 50 }, new ProductSize { ProductID = 22, Size = 40, QuantityInStock = 60 }, new ProductSize { ProductID = 22, Size = 41, QuantityInStock = 55 });

            // --- Sản phẩm 23: Nike Air Max 270 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 23, Name = "Nike Air Max 270", Description = "Nổi bật với phần đệm Air lớn nhất ở gót chân (270 độ). Thân giày vải dệt kim thoáng khí. Thiết kế hiện đại, mang lại sự thoải mái tối đa cho việc đi lại hàng ngày.", Price = 3000000.00m, GenderApplicability = "Unisex", TotalQuantity = 745, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 23, CategoryID = 5 }, new ProductCategory { ProductID = 23, CategoryID = 3 }, new ProductCategory { ProductID = 23, CategoryID = 8 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 23, Size = 40, QuantityInStock = 20 }, new ProductSize { ProductID = 23, Size = 41, QuantityInStock = 25 }, new ProductSize { ProductID = 23, Size = 42, QuantityInStock = 22 });

            // --- Sản phẩm 24: Adidas NMD_R1 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 24, Name = "Adidas NMD_R1", Description = "Kết hợp giữa thiết kế hiện đại và công nghệ Boost êm ái. Các miếng nhựa EVA đặc trưng ở đế giữa. Thân giày Primeknit hoặc vải lưới. Nhẹ nhàng và thoải mái.", Price = 2090000.00m, GenderApplicability = "Unisex", TotalQuantity = 755, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 24, CategoryID = 5 }, new ProductCategory { ProductID = 24, CategoryID = 3 }, new ProductCategory { ProductID = 24, CategoryID = 9 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 24, Size = 38, QuantityInStock = 28 }, new ProductSize { ProductID = 24, Size = 39, QuantityInStock = 32 }, new ProductSize { ProductID = 24, Size = 40, QuantityInStock = 30 });

            // --- Sản phẩm 25: Vans Authentic ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 25, Name = "Vans Authentic", Description = "Mẫu giày đầu tiên của Vans, thiết kế đơn giản, cổ thấp, dây buộc. Thân giày canvas, đế bánh quế. Một lựa chọn cơ bản và linh hoạt.", Price = 1087500.00m, GenderApplicability = "Unisex", TotalQuantity = 765, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 25, CategoryID = 3 }, new ProductCategory { ProductID = 25, CategoryID = 11 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 25, Size = 36, QuantityInStock = 50 }, new ProductSize { ProductID = 25, Size = 37, QuantityInStock = 55 }, new ProductSize { ProductID = 25, Size = 38, QuantityInStock = 60 });

            // --- Sản phẩm 26: New Balance 9060 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 26, Name = "New Balance 9060", Description = "Thiết kế chunky lấy cảm hứng từ dòng 99X và các mẫu giày chạy bộ đầu những năm 2000. Đế giữa ABZORB và SBS êm ái. Kiểu dáng độc đáo, phá cách.", Price = 1479000.00m, GenderApplicability = "Unisex", TotalQuantity = 775, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 26, CategoryID = 6 }, new ProductCategory { ProductID = 26, CategoryID = 4 }, new ProductCategory { ProductID = 26, CategoryID = 12 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 26, Size = 41, QuantityInStock = 15 }, new ProductSize { ProductID = 26, Size = 42, QuantityInStock = 20 }, new ProductSize { ProductID = 26, Size = 43, QuantityInStock = 18 });

            // --- Sản phẩm 27: Puma RS-X Series ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 27, Name = "Puma RS-X Series", Description = "Dòng giày chunky lấy cảm hứng từ công nghệ Running System (RS) thập niên 80. Thiết kế nhiều lớp, màu sắc nổi bật, đế giày dày và êm.", Price = 1590000.00m, GenderApplicability = "Unisex", TotalQuantity = 785, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 27, CategoryID = 6 }, new ProductCategory { ProductID = 27, CategoryID = 4 }, new ProductCategory { ProductID = 27, CategoryID = 16 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 27, Size = 38, QuantityInStock = 22 }, new ProductSize { ProductID = 27, Size = 39, QuantityInStock = 25 }, new ProductSize { ProductID = 27, Size = 40, QuantityInStock = 20 });

            // --- Sản phẩm 28: Biti's Hunter Street ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 28, Name = "Biti's Hunter Street", Description = "Dòng giày đường phố của Biti's Hunter, thiết kế trẻ trung, năng động, dễ phối đồ. Tập trung vào sự thoải mái và tính ứng dụng hàng ngày.", Price = 679000.00m, GenderApplicability = "Unisex", TotalQuantity = 795, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 28, CategoryID = 7 }, new ProductCategory { ProductID = 28, CategoryID = 3 }, new ProductCategory { ProductID = 28, CategoryID = 14 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 28, Size = 37, QuantityInStock = 40 }, new ProductSize { ProductID = 28, Size = 38, QuantityInStock = 50 }, new ProductSize { ProductID = 28, Size = 39, QuantityInStock = 45 });

            // --- Sản phẩm 29: Ananas Vintas Public 2000s ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 29, Name = "Ananas Vintas Public 2000s", Description = "Thuộc dòng Vintas, mang phong cách retro của những năm 2000. Thiết kế đơn giản, sử dụng chất liệu canvas và da lộn.", Price = 620000.00m, GenderApplicability = "Unisex", TotalQuantity = 805, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 29, CategoryID = 7 }, new ProductCategory { ProductID = 29, CategoryID = 4 }, new ProductCategory { ProductID = 29, CategoryID = 15 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 29, Size = 39, QuantityInStock = 60 }, new ProductSize { ProductID = 29, Size = 40, QuantityInStock = 70 }, new ProductSize { ProductID = 29, Size = 41, QuantityInStock = 65 });

            // --- Sản phẩm 30: Nike Dunk Low ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 30, Name = "Nike Dunk Low", Description = "Xuất thân là giày bóng rổ, trở thành biểu tượng thời trang đường phố. Thiết kế cổ thấp, nhiều phối màu đa dạng và các phiên bản collab.", Price = 2750000.00m, GenderApplicability = "Unisex", TotalQuantity = 815, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 30, CategoryID = 3 }, new ProductCategory { ProductID = 30, CategoryID = 4 }, new ProductCategory { ProductID = 30, CategoryID = 8 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 30, Size = 38, QuantityInStock = 30 }, new ProductSize { ProductID = 30, Size = 39, QuantityInStock = 35 }, new ProductSize { ProductID = 30, Size = 40, QuantityInStock = 40 }, new ProductSize { ProductID = 30, Size = 42, QuantityInStock = 30 });

            // --- Sản phẩm 31: Adidas Gazelle ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 31, Name = "Adidas Gazelle", Description = "Mẫu giày training cổ điển từ những năm 60. Thân giày da lộn mềm mại, đế cao su. Thiết kế thanh lịch, gọn gàng.", Price = 2000000.00m, GenderApplicability = "Unisex", TotalQuantity = 825, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 31, CategoryID = 3 }, new ProductCategory { ProductID = 31, CategoryID = 4 }, new ProductCategory { ProductID = 31, CategoryID = 9 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 31, Size = 37, QuantityInStock = 20 }, new ProductSize { ProductID = 31, Size = 38, QuantityInStock = 25 }, new ProductSize { ProductID = 31, Size = 39, QuantityInStock = 22 });

            // --- Sản phẩm 32: Converse Run Star Hike ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 32, Name = "Converse Run Star Hike", Description = "Biến thể hiện đại của Chuck Taylor với đế chunky răng cưa độc đáo. Thân giày canvas quen thuộc. Tạo điểm nhấn cá tính cho phong cách.", Price = 2500000.00m, GenderApplicability = "Unisex", TotalQuantity = 588, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 32, CategoryID = 6 }, new ProductCategory { ProductID = 32, CategoryID = 3 }, new ProductCategory { ProductID = 32, CategoryID = 10 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 32, Size = 36, QuantityInStock = 15 }, new ProductSize { ProductID = 32, Size = 37, QuantityInStock = 20 }, new ProductSize { ProductID = 32, Size = 38, QuantityInStock = 25 });

            // --- Sản phẩm 33: Vans Sk8-Hi ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 33, Name = "Vans Sk8-Hi", Description = "Phiên bản cổ cao của Old Skool, tăng cường bảo vệ mắt cá chân. Sọc jazz đặc trưng, chất liệu canvas và da lộn. Gắn liền với văn hóa trượt ván.", Price = 1755000.00m, GenderApplicability = "Unisex", TotalQuantity = 845, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 33, CategoryID = 3 }, new ProductCategory { ProductID = 33, CategoryID = 11 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 33, Size = 39, QuantityInStock = 30 }, new ProductSize { ProductID = 33, Size = 40, QuantityInStock = 35 }, new ProductSize { ProductID = 33, Size = 41, QuantityInStock = 40 });

            // --- Sản phẩm 34: New Balance 2002R ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 34, Name = "New Balance 2002R", Description = "Lấy cảm hứng từ giày chạy bộ cao cấp những năm 2000. Đế giữa ABZORB và N-ergy êm ái. Thiết kế kỹ thuật, chất liệu cao cấp.", Price = 3000000.00m, GenderApplicability = "Unisex", TotalQuantity = 855, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 34, CategoryID = 4 }, new ProductCategory { ProductID = 34, CategoryID = 5 }, new ProductCategory { ProductID = 34, CategoryID = 12 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 34, Size = 40, QuantityInStock = 18 }, new ProductSize { ProductID = 34, Size = 41, QuantityInStock = 20 }, new ProductSize { ProductID = 34, Size = 42, QuantityInStock = 25 });

            // --- Sản phẩm 35: Puma Cali ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 35, Name = "Puma Cali", Description = "Lấy cảm hứng từ mẫu giày California cổ điển. Thiết kế đế platform nhẹ nhàng, nữ tính. Thân giày da. Phong cách thoải mái, phóng khoáng.", Price = 1800000.00m, GenderApplicability = "Female", TotalQuantity = 340, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 35, CategoryID = 3 }, new ProductCategory { ProductID = 35, CategoryID = 16 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 35, Size = 36, QuantityInStock = 25 }, new ProductSize { ProductID = 35, Size = 37, QuantityInStock = 30 }, new ProductSize { ProductID = 35, Size = 38, QuantityInStock = 28 });

            // --- Sản phẩm 36: Nike Air Jordan 1 Low ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 36, Name = "Nike Air Jordan 1 Low", Description = "Phiên bản cổ thấp của huyền thoại Air Jordan 1. Thiết kế mang tính biểu tượng, nhiều phối màu lịch sử và mới mẻ. Chất liệu da.", Price = 3190000.00m, GenderApplicability = "Unisex", TotalQuantity = 875, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 36, CategoryID = 3 }, new ProductCategory { ProductID = 36, CategoryID = 4 }, new ProductCategory { ProductID = 36, CategoryID = 8 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 36, Size = 39, QuantityInStock = 15 }, new ProductSize { ProductID = 36, Size = 40, QuantityInStock = 20 }, new ProductSize { ProductID = 36, Size = 41, QuantityInStock = 18 });

            // --- Sản phẩm 37: Adidas Alphabounce ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 37, Name = "Adidas Alphabounce", Description = "Thiết kế hiện đại, tập trung vào sự thoải mái và đa năng. Đệm Bounce linh hoạt. Thân giày Forgedmesh hoặc Engineered Mesh.", Price = 2000000.00m, GenderApplicability = "Male", TotalQuantity = 624, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 37, CategoryID = 5 }, new ProductCategory { ProductID = 37, CategoryID = 3 }, new ProductCategory { ProductID = 37, CategoryID = 9 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 37, Size = 42, QuantityInStock = 30 }, new ProductSize { ProductID = 37, Size = 43, QuantityInStock = 35 }, new ProductSize { ProductID = 37, Size = 44, QuantityInStock = 25 });

            // --- Sản phẩm 38: Asics GEL-Lyte III ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 38, Name = "Asics GEL-Lyte III", Description = "Mẫu giày chạy bộ cổ điển từ những năm 90, nổi tiếng với thiết kế lưỡi gà chẻ đôi (split tongue). Công nghệ đệm GEL êm ái.", Price = 2500000.00m, GenderApplicability = "Unisex", TotalQuantity = 895, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 38, CategoryID = 4 }, new ProductCategory { ProductID = 38, CategoryID = 5 }, new ProductCategory { ProductID = 38, CategoryID = 19 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 38, Size = 38, QuantityInStock = 18 }, new ProductSize { ProductID = 38, Size = 39, QuantityInStock = 20 }, new ProductSize { ProductID = 38, Size = 40, QuantityInStock = 22 });

            // --- Sản phẩm 39: Fila Ray Tracer ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 39, Name = "Fila Ray Tracer", Description = "Mẫu giày chunky lấy cảm hứng từ thập niên 90. Thiết kế nhiều lớp chất liệu, đế giày hầm hố. Phối màu đa dạng, trẻ trung.", Price = 1500000.00m, GenderApplicability = "Unisex", TotalQuantity = 598, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 39, CategoryID = 6 }, new ProductCategory { ProductID = 39, CategoryID = 4 }, new ProductCategory { ProductID = 39, CategoryID = 13 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 39, Size = 37, QuantityInStock = 25 }, new ProductSize { ProductID = 39, Size = 38, QuantityInStock = 30 }, new ProductSize { ProductID = 39, Size = 39, QuantityInStock = 28 });

            // --- Sản phẩm 40: Biti's Hunter Jogging/Running ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 40, Name = "Biti's Hunter Jogging/Running", Description = "Các dòng giày chạy bộ của Biti's Hunter, tập trung vào hiệu năng và sự thoải mái. Công nghệ đế Phylon hoặc LiteFlex.", Price = 881000.00m, GenderApplicability = "Unisex", TotalQuantity = 915, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 40, CategoryID = 7 }, new ProductCategory { ProductID = 40, CategoryID = 5 }, new ProductCategory { ProductID = 40, CategoryID = 14 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 40, Size = 39, QuantityInStock = 40 }, new ProductSize { ProductID = 40, Size = 40, QuantityInStock = 50 }, new ProductSize { ProductID = 40, Size = 41, QuantityInStock = 45 });

            // --- Sản phẩm 41: Ananas Pattas \"Simple Story\" ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 41, Name = "Ananas Pattas \"Simple Story\"", Description = "Dòng giày Pattas với thiết kế tối giản, tập trung vào câu chuyện và thông điệp. Thân giày canvas, đế vulcanized.", Price = 500000.00m, GenderApplicability = "Unisex", TotalQuantity = 925, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 41, CategoryID = 7 }, new ProductCategory { ProductID = 41, CategoryID = 3 }, new ProductCategory { ProductID = 41, CategoryID = 15 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 41, Size = 38, QuantityInStock = 60 }, new ProductSize { ProductID = 41, Size = 39, QuantityInStock = 70 }, new ProductSize { ProductID = 41, Size = 40, QuantityInStock = 65 });

            // --- Sản phẩm 42: Nike Air Max Dn ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 42, Name = "Nike Air Max Dn", Description = "Dòng Air Max mới nhất với công nghệ Dynamic Air (các ống Air có áp suất kép). Mang lại cảm giác chuyển động mượt mà và năng động.", Price = 3500000.00m, GenderApplicability = "Unisex", TotalQuantity = 935, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 42, CategoryID = 5 }, new ProductCategory { ProductID = 42, CategoryID = 8 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 42, Size = 40, QuantityInStock = 15 }, new ProductSize { ProductID = 42, Size = 41, QuantityInStock = 18 }, new ProductSize { ProductID = 42, Size = 42, QuantityInStock = 20 });

            // --- Sản phẩm 43: Adidas Samba OG ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 43, Name = "Adidas Samba OG", Description = "Mẫu giày sân cỏ trong nhà (indoor football) kinh điển. Thân giày da hoặc da lộn, đế gum. Thiết kế vượt thời gian, đang rất thịnh hành trở lại.", Price = 2290000.00m, GenderApplicability = "Unisex", TotalQuantity = 945, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 43, CategoryID = 3 }, new ProductCategory { ProductID = 43, CategoryID = 4 }, new ProductCategory { ProductID = 43, CategoryID = 9 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 43, Size = 38, QuantityInStock = 25 }, new ProductSize { ProductID = 43, Size = 39, QuantityInStock = 30 }, new ProductSize { ProductID = 43, Size = 40, QuantityInStock = 35 });

            // --- Sản phẩm 44: Converse Chuck Taylor All Star Move ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 44, Name = "Converse Chuck Taylor All Star Move", Description = "Phiên bản hiện đại của Chuck Taylor với đế platform nhẹ và cao. Mang lại vẻ ngoài năng động và tôn dáng.", Price = 1800000.00m, GenderApplicability = "Female", TotalQuantity = 360, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 44, CategoryID = 3 }, new ProductCategory { ProductID = 44, CategoryID = 6 }, new ProductCategory { ProductID = 44, CategoryID = 10 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 44, Size = 36, QuantityInStock = 20 }, new ProductSize { ProductID = 44, Size = 37, QuantityInStock = 25 }, new ProductSize { ProductID = 44, Size = 38, QuantityInStock = 30 });

            // --- Sản phẩm 45: Vans Knu Skool ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 45, Name = "Vans Knu Skool", Description = "Lấy cảm hứng từ giày trượt ván thập niên 90, với thiết kế \"phồng\" hơn, lưỡi gà dày và sọc jazz 3D nổi. Mang phong cách chunky retro.", Price = 1000000.00m, GenderApplicability = "Unisex", TotalQuantity = 965, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 45, CategoryID = 6 }, new ProductCategory { ProductID = 45, CategoryID = 3 }, new ProductCategory { ProductID = 45, CategoryID = 11 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 45, Size = 38, QuantityInStock = 28 }, new ProductSize { ProductID = 45, Size = 39, QuantityInStock = 35 }, new ProductSize { ProductID = 45, Size = 40, QuantityInStock = 30 });

            // --- Sản phẩm 46: New Balance 530 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 46, Name = "New Balance 530", Description = "Mẫu giày chạy bộ cổ điển từ những năm 90, nay trở thành item thời trang được yêu thích. Đế giữa ABZORB êm ái. Thân giày lưới và da tổng hợp.", Price = 2490000.00m, GenderApplicability = "Unisex", TotalQuantity = 975, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 46, CategoryID = 4 }, new ProductCategory { ProductID = 46, CategoryID = 5 }, new ProductCategory { ProductID = 46, CategoryID = 12 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 46, Size = 37, QuantityInStock = 18 }, new ProductSize { ProductID = 46, Size = 38, QuantityInStock = 22 }, new ProductSize { ProductID = 46, Size = 39, QuantityInStock = 25 });

            // --- Sản phẩm 47: Puma Mayze Lth ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 47, Name = "Puma Mayze Lth", Description = "Thiết kế đế platform cá tính, lấy cảm hứng từ sự nhộn nhịp đô thị. Dành cho những cô gái yêu thích phong cách thời trang đường phố. Thân giày da.", Price = 2000000.00m, GenderApplicability = "Female", TotalQuantity = 380, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 47, CategoryID = 6 }, new ProductCategory { ProductID = 47, CategoryID = 3 }, new ProductCategory { ProductID = 47, CategoryID = 16 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 47, Size = 36, QuantityInStock = 30 }, new ProductSize { ProductID = 47, Size = 37, QuantityInStock = 35 }, new ProductSize { ProductID = 47, Size = 38, QuantityInStock = 30 });

            // --- Sản phẩm 48: Nike Zoom Vomero 5 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 48, Name = "Nike Zoom Vomero 5", Description = "Mẫu giày chạy bộ đầu những năm 2000, nay trở lại với phong cách retro-tech. Đệm Zoom Air và Cushlon êm ái. Thiết kế nhiều lớp, chi tiết.", Price = 3500000.00m, GenderApplicability = "Unisex", TotalQuantity = 995, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 48, CategoryID = 4 }, new ProductCategory { ProductID = 48, CategoryID = 5 }, new ProductCategory { ProductID = 48, CategoryID = 8 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 48, Size = 40, QuantityInStock = 20 }, new ProductSize { ProductID = 48, Size = 41, QuantityInStock = 25 }, new ProductSize { ProductID = 48, Size = 42, QuantityInStock = 22 });

            // --- Sản phẩm 49: Adidas Campus 00s ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 49, Name = "Adidas Campus 00s", Description = "Lấy cảm hứng từ Adidas Campus cổ điển nhưng với tỷ lệ phóng đại, mang hơi hướng giày skate thập niên 2000. Thân giày da lộn, dây giày bản to.", Price = 2500000.00m, GenderApplicability = "Unisex", TotalQuantity = 1005, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 49, CategoryID = 4 }, new ProductCategory { ProductID = 49, CategoryID = 3 }, new ProductCategory { ProductID = 49, CategoryID = 9 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 49, Size = 38, QuantityInStock = 28 }, new ProductSize { ProductID = 49, Size = 39, QuantityInStock = 30 }, new ProductSize { ProductID = 49, Size = 40, QuantityInStock = 35 });

            // --- Sản phẩm 50: Biti's Hunter Core LiteFoam 3.0 ---
            modelBuilder.Entity<Product>().HasData(new Product { ProductID = 50, Name = "Biti's Hunter Core LiteFoam 3.0", Description = "Phiên bản nâng cấp của dòng Core với công nghệ đế LiteFoam 3.0 cải tiến, tăng độ êm và nhẹ. Thiết kế đa năng, phù hợp nhiều hoạt động.", Price = 843000.00m, GenderApplicability = "Unisex", TotalQuantity = 1015, IsActive = true });
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductID = 50, CategoryID = 7 }, new ProductCategory { ProductID = 50, CategoryID = 5 }, new ProductCategory { ProductID = 50, CategoryID = 3 }, new ProductCategory { ProductID = 50, CategoryID = 14 });
            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { ProductID = 50, Size = 39, QuantityInStock = 45 }, new ProductSize { ProductID = 50, Size = 40, QuantityInStock = 55 }, new ProductSize { ProductID = 50, Size = 41, QuantityInStock = 50 });

            #endregion
        }
    }
}
