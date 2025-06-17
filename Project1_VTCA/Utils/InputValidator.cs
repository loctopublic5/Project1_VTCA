using System.Text.RegularExpressions;

namespace Project1_VTCA.Utils
{
    public static class InputValidator
    {
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return Regex.IsMatch(username, @"^[A-Za-z0-9]{3,20}$");
        }

        // Cập nhật quy tắc kiểm tra mật khẩu mạnh
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || password.Length > 20)
            {
                return false;
            }
            // Yêu cầu: ít nhất 1 chữ hoa, 1 chữ thường, 1 số, và 1 ký tự đặc biệt
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasNumber = new Regex(@"[0-9]+");
            var hasSpecialChar = new Regex(@"[\W_]+"); // \W là ký tự không phải chữ và số

            return hasUpperChar.IsMatch(password) && hasLowerChar.IsMatch(password) && hasNumber.IsMatch(password) && hasSpecialChar.IsMatch(password);
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[a-z0-9\._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$");
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length != 10) return false;
            return phoneNumber.All(char.IsDigit);
        }
    }
}