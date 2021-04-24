using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.API.Providers
{
    public class DateProvider : IDateProvider
    {
        public DateTimeOffset GetAfterUtcNow(int days, int minutes)
        {
            return DateTimeOffset.UtcNow.AddDays(days).AddMinutes(minutes);
        }

        public DateTimeOffset GetUtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
