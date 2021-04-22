using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.API.Providers
{
    public interface IDateProvider
    {
        DateTimeOffset GetUtcNow();
    }
}
