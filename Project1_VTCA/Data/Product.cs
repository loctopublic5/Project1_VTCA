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
        
        public Product()
        {
            ProductCategories = new HashSet<ProductCategory>();
            ProductSizes = new HashSet<ProductSize>();
            OrderDetails = new HashSet<OrderDetail>();
        }

        [Key]
        public int ProductID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [MaxLength(10)]
        public string GenderApplicability { get; set; } 

        public int TotalQuantity { get; set; }

        // Navigation Properties
        public virtual ICollection<ProductCategory> ProductCategories { get; set; }
        public virtual ICollection<ProductSize> ProductSizes { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
    }
}
