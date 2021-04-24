using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.API.Dtos
{
    public record RegisterRequest
    {
        public string Username { get; init; }
        public string Email { get; init; }
        public string Password { get; init; }
    }
}
