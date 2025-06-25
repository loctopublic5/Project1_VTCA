using System.Text.RegularExpressions;

namespace Project1_VTCA.Utils
{
    public static class InputValidator
    {
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return Regex.IsMatch(username, @"^[a-zA-Z0-9]{3,20}$");
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || password.Length > 20) return false;

            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasNumber = new Regex(@"[0-9]+");
            var hasSpecialChar = new Regex(@"[\W_]+");

            return hasUpperChar.IsMatch(password) && hasLowerChar.IsMatch(password) && hasNumber.IsMatch(password) && hasSpecialChar.IsMatch(password);
        }

        // CẬP NHẬT REGEX VÀ LOGIC KIỂM TRA EMAIL
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            // Quy tắc: Chỉ chứa chữ thường, số, dấu chấm '.'
            // và phải kết thúc bằng @gmail.com
            return Regex.IsMatch(email, @"^[a-z0-9\.]+@gmail\.com$");
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
            return Regex.IsMatch(phoneNumber, @"^0[0-9]{9}$");
        }
    }
}