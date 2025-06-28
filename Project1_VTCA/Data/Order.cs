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

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime OrderDate { get; set; } = DateTime.Now;

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

        // --- CÁC CỘT ĐƯỢC KHÔI PHỤC LẠI ---
        [StringLength(50)]
        public string PaymentMethod { get; set; } // Ví dụ: "COD", "Online"

        public int? ApprovedByAdminID { get; set; }

        [StringLength(200)]
        public string AdminDecisionReason { get; set; } // Lý do admin hủy đơn

        [StringLength(200)]
        public string CustomerCancellationReason { get; set; } // Lý do khách hàng hủy đơn

        public bool RefundRequested { get; set; } = false;
        // ------------------------------------

        // Khóa ngoại
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("ApprovedByAdminID")]
        public virtual User ApprovedByAdmin { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}