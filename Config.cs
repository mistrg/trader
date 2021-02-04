public static class Config
{
    public static string ConnectionString { get { return "mongodb+srv://dbadmin:yYzEvE9dqsYqgNSg@mongcluster.5uwqc.mongodb.net/<dbname>?connect=replicaSet&retryWrites=true&w=majority"; } }
    public static string CoinmatePublicKey { get { return "3yGprI0h4HkDKTlUaxDfZ0DAEvydg61HKXPNN6MDrWY"; } }
    public static string CoinmatePrivateKey { get { return "E3WMta56vv9ZJw1MH1XIagvUAq105B9LS4fkilAikeY"; } }
    public static string CoinmateClientId { get { return "28298"; } }

    public static bool WriteToMongo { get { return false; } }

    public static bool ProcessTrades { get { return true; } }


}