public static class Config
{
    public static string CoinmatePublicKey { get { return ""; } }
    public static string CoinmatePrivateKey { get { return ""; } }
    public static string CoinmateClientId { get { return ""; } }



    public static string BinanceApiKey { get { return ""; } }
    public static string BinanceSecretKey { get { return ""; } }


    public static int Version { get { return 24; } }
    public static string RunId { get; set; }

    public static string PostgresConnectionString { get { return "Server = drbalek.cz; Port = 5432; Database = Drbor; User Id = drborbot; Password = G3ed9lPii3; "; } }



    public static bool AutomatedTrading { get { return false; } }
    public static double AutomatedTradingMinEstimatedProfitNetRate { get { return 0.49; } }




    public static bool ProcessTrades { get { return true; } }

    public static int BuyTimeoutInMs { get { return 2500; } }
    public static int SellTimeoutInMs { get { return 5000; } }


}