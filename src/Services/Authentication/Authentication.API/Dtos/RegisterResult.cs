using Authentication.API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.API.Dtos
{
    public class RegisterResult
    {
        public RegisterResultMessage Message { get; set; }
        public int UserId { get; set; }
    }
}
