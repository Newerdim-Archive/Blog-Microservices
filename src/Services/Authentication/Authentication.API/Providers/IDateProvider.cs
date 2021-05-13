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
        /// <exception cref="ArgumentOutOfRangeException">Throws when number of days is too big</exception>
        DateTimeOffset GetAfterUtcNow(int days, int minutes);
    }
}