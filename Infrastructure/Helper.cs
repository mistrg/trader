using System;

namespace Trader.Infrastructure
{
    public static class Helper
    {

        public static string GetPostgreConnectionString()
        {
            string envVar = Config.PostgresConnectionString;
            //parse database URL. Format is postgres://<username>:<password>@<host>/<dbname>
            var uri = new Uri(envVar);
            var username = uri.UserInfo.Split(':')[0];
            var password = uri.UserInfo.Split(':')[1];
            var connectionString =
            "Host=" + uri.AbsolutePath.Substring(1) +
            "; Database=" + uri.AbsolutePath.Substring(1) +
            "; Username=" + username +
            "; Password=" + password +
            "; Port=" + uri.Port +
            "; SSL Mode=Require; Trust Server Certificate=true;";
            return connectionString;
        }
    }
}