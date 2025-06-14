using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1_VTCA.Data
{
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        [Column(Order = 0)]
        public int UserID { get; set; }

        [Key]
        [Column(Order = 1)]
        public int ProductID { get; set; }

        [Key]
        [Column(Order = 2)]
        public int Size { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }

        [ForeignKey("ProductID")]
        public Product Product { get; set; }
    }
}
