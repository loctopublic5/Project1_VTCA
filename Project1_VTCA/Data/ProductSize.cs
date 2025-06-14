using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1_VTCA.Data
{
    [Table("ProductSizes")]
    public class ProductSize
    {
        [Key]
        [Column(Order = 0)]
        public int ProductID { get; set; }

        [Key]
        [Column(Order = 1)]
        public int Size { get; set; }

        public int? QuantityInStock { get; set; }

        [ForeignKey("ProductID")]
        public Product Product { get; set; }
    }
}
