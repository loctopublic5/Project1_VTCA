namespace Project1_VTCA.DTOs
{
    public record AuthResult(bool IsSuccess, string Message, string UserRole = null);
}