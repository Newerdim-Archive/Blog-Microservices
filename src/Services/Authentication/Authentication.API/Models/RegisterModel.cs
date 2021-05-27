using System.ComponentModel.DataAnnotations;

namespace Authentication.API.Models
{
    public class RegisterModel
    {
        /// <summary>
        /// Username
        /// <para>Minimum length is 3</para>
        /// </summary>
        /// <example>User123</example>
        public string Username { get; set; }

        /// <summary>
        /// Address email
        /// </summary>
        /// <example>User123@mail.com</example>
        public string Email { get; set; }

        /// <summary>
        /// Password
        /// <para>Must contain a lower and  uppercase letter and a number</para>
        /// <para>Minimum length is 6</para>
        /// </summary>
        /// <example>Password123!@#</example>
        public string Password { get; set; }
    }
}