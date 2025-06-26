using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project1_VTCA.Data
{
    [Table("Addresses")]
    public class Address
    {
        [Key]
        public int AddressID { get; set; }
        public int UserID { get; set; }

        [Required]
        [StringLength(200)]
        public string AddressDetail { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(10)]
        public string ReceivePhone { get; set; }

        public bool IsDefault { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}