using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project1_VTCA.Data;

namespace Project1_VTCA.Data
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [StringLength(10)]
        public string GenderApplicability { get; set; }

        //// Báo cho EF biết giá trị này được tính bởi DB (qua trigger)
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int TotalQuantity { get; set; }

        public bool IsActive { get; set; }

        // ... các collection khác giữ nguyên
        public ICollection<ProductCategory> ProductCategories { get; set; }
        public ICollection<ProductSize> ProductSizes { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
