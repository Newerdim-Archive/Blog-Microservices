using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.API.Dtos
{
    public record PublishEmailConfirmationRequest
    {
        public string Token { get; init; }
        public string TargetEmail { get; init; }
        public string ReturnUrl { get; init; }
    }
}
