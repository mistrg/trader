public static class Config
{
    public static string CoinmatePublicKey { get { return "Ik7aDMVzVhPh5tZsz12Gpp_U62cZTbf-a9Id6VLHQZ8"; } }
    public static string CoinmatePrivateKey { get { return "iyt6UAxhroifDQBCkPTNhnjVL8LRI7TgPpY8AAgisQ8"; } }
    public static string CoinmateClientId { get { return "28298"; } }



    public static string BinanceApiKey { get { return "PhWiIMtXBRVucLWpsmz1TSffWN1jgpvE9TD3T7cLwPtgG1MdmLkqkx1GBr7EdzxW"; } }
    public static string BinanceSecretKey { get { return "wkLjUrU6Jfa9XbZ8kxysr7xajMq67fySuxysIjxYnhpoYi46LtUIrTkfnZC052Y1"; } }


    public static int Version { get { return 23; } }
    public static string RunId { get; set; }

    public static string PostgresConnectionString { get { return "Server = drbalek.cz; Port = 5432; Database = Drbor; User Id = drborbot; Password = G3ed9lPii3; "; } }



    public static bool AutomatedTrading { get { return false; } }
    public static double AutomatedTradingMinEstimatedProfitNetRate { get { return 0.49; } }




    public static bool ProcessTrades { get { return true; } }

    public static int BuyTimeoutInMs { get { return 2500; } }
    public static int SellTimeoutInMs { get { return 5000; } }


}