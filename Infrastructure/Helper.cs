using System;

namespace Trader.Infrastructure
{
    public static class Helper
    {

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static long ConvertToTimestamp(DateTime value)
        {
           
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
            TimeSpan span = (value - epoch);
            return (long)Convert.ToDouble(span.TotalSeconds);
        }


    }
}