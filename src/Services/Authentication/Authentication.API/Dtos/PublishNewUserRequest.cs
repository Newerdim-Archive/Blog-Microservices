using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.API.Dtos
{
    public class PublishNewUserRequest
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }
    }
}
