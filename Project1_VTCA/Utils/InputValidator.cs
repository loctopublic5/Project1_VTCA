using System.Text.RegularExpressions;

namespace Project1_VTCA.Utils
{
    public static class InputValidator
    {
        // Kiểm tra Username: không dấu, không khoảng trắng, không ký tự đặc biệt
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            // Chỉ cho phép chữ cái (a-z) và số (0-9)
            return Regex.IsMatch(username, @"^[A-Za-z0-9]+$");
        }

        // Kiểm tra Mật khẩu: có độ dài tối thiểu và tối đa
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            // Yêu cầu độ dài từ 6 đến 20 ký tự
            return password.Length >= 6 && password.Length <= 20;
        }

        // Kiểm tra Email: theo định dạng bạn yêu cầu
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            // Yêu cầu: chữ thường, số, không dấu, liền trước @
            return Regex.IsMatch(email, @"^[a-z0-9]+@[a-z]+\.[a-z]{2,}$");
        }

        // Giữ nguyên kiểm tra số điện thoại
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
            return phoneNumber.All(char.IsDigit) && phoneNumber.Length == 10;
        }
    }
}