using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.API.Enums
{
    public enum RegisterResultMessage
    {
        Successful,
        EmailAlreadyExists,
        UsernameAlreadyExists
    }
}
