using System;

namespace RedditSharp.Utils
{
    internal static class DateTimeExtensions
    {
        public static double DateTimeToUnixTimestamp(this DateTime dateTime)
        {
            double time = (dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;

            return Convert.ToInt32(time);
        }
    }
}