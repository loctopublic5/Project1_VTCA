using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Project1_VTCA.Data
{
    [Table("Users")]
    public class User
    {
        [Key] // Đánh dấu đây là khóa chính
        public int UserID { get; set; }

        [StringLength(50)] // Giới hạn độ dài chuỗi
        public string Username { get; set; }

        [StringLength(100)]
        public string PasswordHash { get; set; }

        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        [Column(TypeName = "decimal(18, 2)")] // Chỉ định kiểu dữ liệu trong DB
        public decimal Balance { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalSpending { get; set; }

        [StringLength(20)]
        public string Role { get; set; }

        public bool IsActive { get; set; }

        // Các mối quan hệ
        public ICollection<Address> Addresses { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Order> ApprovedOrders { get; set; }
    }
}
