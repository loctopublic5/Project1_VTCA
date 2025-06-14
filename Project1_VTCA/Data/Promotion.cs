using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1_VTCA.Data
{
    [Table("Promotions")]
    public class Promotion
    {
        [Key]
        public int PromotionID { get; set; }

        [StringLength(50)]
        public string Code { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? DiscountAmount { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? DiscountPercentage { get; set; }

        [StringLength(10)]
        public string ApplicableGender { get; set; }

        public int? ApplicableSize { get; set; }

        public int? ApplicableProductId { get; set; }

        public int? ApplicableCategoryId { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey("ApplicableProductId")]
        public Product Product { get; set; }

        [ForeignKey("ApplicableCategoryId")]
        public Category Category { get; set; }
    }

}
