using Authentication.API.Helpers;

namespace Authentication.IntergrationTests.Controllers
{
    public static class AuthControllerFullRoute
    {
        public static readonly string Login = GetFullRoute(AuthControllerRoutes.Login);
        public static readonly string Register = GetFullRoute(AuthControllerRoutes.Register);
        public static readonly string RefreshTokens = GetFullRoute(AuthControllerRoutes.RefreshTokens);

        private static string GetFullRoute(string endpoint)
        {
            return $"{AuthControllerRoutes.Controller}/{endpoint}";
        }
    }
}