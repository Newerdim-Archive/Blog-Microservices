using System;

namespace Authentication.API.Providers
{
    public interface IDateProvider
    {
        /// <summary>
        /// Get UTC now date
        /// </summary>
        /// <returns>UTC now date</returns>
        DateTimeOffset GetUtcNow();

        /// <summary>
        /// Get UTC date after specified time
        /// </summary>
        /// <param name="days">How many days after</param>
        /// <param name="minutes">How many minutes after</param>
        /// <returns>UTC now with added days and minutes</returns>
        DateTimeOffset GetAfterUtcNow(int days, int minutes);
    }
}
