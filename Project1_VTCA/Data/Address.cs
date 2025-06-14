using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1_VTCA.Data
{
    [Table("Addresses")]
    public class Address
    {
        [Key]
        public int AddressID { get; set; }

        public int UserID { get; set; }

        // Đã đổi tên từ AddressDetail và cập nhật độ dài
        [StringLength(200)]
        public string StreetAddress { get; set; }

        // Thêm cột City
        [StringLength(100)]
        public string City { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }
    }
}
