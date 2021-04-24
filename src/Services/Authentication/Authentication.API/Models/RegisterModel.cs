using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.API.Models
{
    public record RegisterModel
    {
        /// <summary>
        /// Username
        /// <para>Minimum length is 3</para>
        /// <example>User123</example>
        /// </summary>
        [Required]
        public string Username { get; init; }

        /// <summary>
        /// Adress email
        /// <example>User123@gmail.com</example>
        /// </summary>
        [Required]
        public string Email { get; init; }

        /// <summary>
        /// Password
        /// <para>Must contains one lower and one uppercase letter and one number</para>
        /// <para>Minimum length is 6</para>
        /// <example>User123@gmail.com</example>
        /// </summary>
        [Required]
        public string Password { get; init; }
    }
}
