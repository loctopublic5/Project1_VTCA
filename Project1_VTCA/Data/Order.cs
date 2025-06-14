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
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        public int UserID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime OrderDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShippingFee { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShippingDiscountAmount { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        public int? ApprovedByAdminID { get; set; }

        [StringLength(200)]
        public string AdminDecisionReason { get; set; }

        [StringLength(200)]
        public string CustomerCancellationReason { get; set; }

        public bool RefundRequested { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }

        [ForeignKey("ApprovedByAdminID")]
        public User ApprovedByAdmin { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
