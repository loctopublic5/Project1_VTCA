using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project1_VTCA.Data
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [Required]
        [StringLength(30)]
        public string OrderCode { get; set; }

        public int UserID { get; set; }

        // CẬP NHẬT: Loại bỏ DataAnnotation, để C# toàn quyền kiểm soát.
        public DateTime OrderDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(300)]
        public string ShippingAddress { get; set; }

        [Required]
        [StringLength(10)]
        public string ShippingPhone { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        public int? ApprovedByAdminID { get; set; }

        [StringLength(200)]
        public string? AdminDecisionReason { get; set; }

        [StringLength(200)]
        public string? CustomerCancellationReason { get; set; }

        public bool RefundRequested { get; set; } = false;

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("ApprovedByAdminID")]
        public virtual User? ApprovedByAdmin { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}