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
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// Address email
        /// </summary>
        /// <example>User123@mail.com</example>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// Password
        /// <para>Must contain a lower and  uppercase letter and a number</para>
        /// <para>Minimum length is 6</para>
        /// </summary>
        /// <example>User123@mail.com</example>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// URL where user should be redirected when click email confirmation link
        /// <para>Note: It will put a token at the end of URL</para>
        /// </summary>
        /// <example>https://www.mysite.com/emailConfirmation?token=</example>
        [Required]
        public string EmailConfirmationUrl { get; set; }
    }
}