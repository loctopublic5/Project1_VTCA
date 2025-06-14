using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1_VTCA.Data
{
    [Table("ProductCategories")]
    public class ProductCategory
    {
        // Khóa chính phức hợp
        [Key]
        [Column(Order = 0)]
        public int ProductID { get; set; }

        [Key]
        [Column(Order = 1)]
        public int CategoryID { get; set; }

        // Navigation properties
        [ForeignKey("ProductID")]
        public Product Product { get; set; }

        [ForeignKey("CategoryID")]
        public Category Category { get; set; }
    }
}
