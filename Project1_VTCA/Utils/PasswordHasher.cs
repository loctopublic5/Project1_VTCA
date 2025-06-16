using System.Security.Cryptography;
using System.Text;

namespace Project1_VTCA.Utils
{
    public static class PasswordHasher
    {
        // *** LƯU Ý QUAN TRỌNG VỀ BẢO MẬT ***
        // Cách mã hóa này chỉ dùng cho mục đích học tập.
        // Trong các ứng dụng thực tế, hãy sử dụng các thư viện mạnh mẽ hơn như
        // BCrypt.Net hoặc Argon2.

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}