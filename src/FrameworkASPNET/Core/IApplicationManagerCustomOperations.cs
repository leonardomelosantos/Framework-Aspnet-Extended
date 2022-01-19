using System.Web;
using FrameworkAspNetExtended.Entities;

namespace FrameworkAspNetExtended.Core
{
    public interface IApplicationManagerCustomOperations
    {
        UserAutenticatedInfo ValidateUserCredentials(UserAutenticatedInfo user);

        UserAutenticatedInfo ChangePassword(HttpContextBase httpContext, UserAutenticatedInfo user, string newPassword);

        void RegisterLogon(HttpContextBase httpContext, UserAutenticatedInfo user);

        void RegisterLogoff(HttpContextBase httpContext);

        UserAutenticatedInfo GetUserAuthenticated(HttpContextBase httpContext);

        bool IsAuthenticatedUser(HttpContextBase httpContext);

        UserAutenticatedInfo RememberPassword(string username);
    }
}
