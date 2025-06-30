using Project1_VTCA.Data;

namespace Project1_VTCA.Services.Interface
{
    public interface ISessionService
    {
        User CurrentUser { get; }
        bool IsLoggedIn { get; }
        void LoginUser(User user);
        void LogoutUser();
    }
}