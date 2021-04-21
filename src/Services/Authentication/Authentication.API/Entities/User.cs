using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.API.Entities
{
    public record User : BaseEntity
    {
        public string Username { get; init; }
        public string Email { get; init; }
        public byte[] PasswordHash { get; init; }
        public byte[] PasswordSalt { get; init; }
        public bool ConfirmedEmail { get; init; }
    }
}
