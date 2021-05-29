using Authentication.API.Helpers;

namespace Authentication.IntergrationTests.Controllers
{
    public static class AuthControllerFullRoute
    {
        public static readonly string Login = GetFullRoute(AuthControllerRoutes.Login);
        public static readonly string Register = GetFullRoute(AuthControllerRoutes.Register);

        #region Private Methods

        private static string GetFullRoute(string endpoint)
        {
            return $"{AuthControllerRoutes.Controller}/{endpoint}";
        }

        #endregion
    }
}