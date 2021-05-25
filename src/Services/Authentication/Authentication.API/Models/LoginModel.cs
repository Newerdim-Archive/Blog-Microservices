namespace Authentication.API.Models
{
    public class LoginModel
    {
        /// <summary>
        /// Username
        /// </summary>
        /// <example>User123</example>
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        /// <example>User123!@#</example>
        public string Password { get; set; }
    }
}