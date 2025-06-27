namespace Project1_VTCA
{
    /// <summary>
    /// Lớp đóng gói kết quả trả về từ các phương thức của Service.
    /// Giúp phân tách trách nhiệm giữa logic và giao diện.
    /// </summary>
    /// <param name="IsSuccess">Cho biết thao tác có thành công hay không.</param>
    /// <param name="Message">Chứa thông điệp thô, không có markup định dạng.</param>
    public record ServiceResponse(bool IsSuccess, string Message);
}