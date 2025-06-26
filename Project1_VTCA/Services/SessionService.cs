using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;

namespace Project1_VTCA.Services
{
    public class SessionService : ISessionService
    {
        public User CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser != null;

        public void LoginUser(User user)
        {
            CurrentUser = user;
        }

        public void LogoutUser()
        {
            CurrentUser = null;
        }
    }
}