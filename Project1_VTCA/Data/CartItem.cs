﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project1_VTCA.Data
{
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        public int CartItemID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int Size { get; set; }
        public int Quantity { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
    }
}