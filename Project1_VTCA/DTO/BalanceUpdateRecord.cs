namespace Project1_VTCA.DTOs
{
    public record BalanceUpdateResult(bool IsSuccess, string Message, decimal NewBalance);
}