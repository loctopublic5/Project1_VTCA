namespace Project1_VTCA.DTOs
{
    /// <summary>
    /// Đại diện cho kết quả của một hành động xác thực.
    /// </summary>
    /// <param name="IsSuccess">Hành động có thành công hay không.</param>
    /// <param name="Message">Thông báo trả về cho người dùng.</param>
    public record AuthResult(bool IsSuccess, string Message, string UserRole = null);
}