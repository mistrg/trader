public static class Config
{
    public static int Version { get { return 25; } }
    public static string RunId { get; set; }

    public static bool AutomatedTrading { get { return false; } }
    public static double AutomatedTradingMinEstimatedProfitNetRate { get { return 0.49; } }




    public static bool ProcessTrades { get { return false; } }

    public static int BuyTimeoutInMs { get { return 2500; } }
    public static int SellTimeoutInMs { get { return 5000; } }

    public static bool PauseAfterArbitrage { get{return true;} }
}