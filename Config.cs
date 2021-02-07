public static class Config
{
    public static string ConnectionString { get { return "mongodb+srv://dbadmin:yYzEvE9dqsYqgNSg@mongcluster.5uwqc.mongodb.net/<dbname>?connect=replicaSet&retryWrites=true&w=majority"; } }
    public static string CoinmatePublicKey { get { return "Ik7aDMVzVhPh5tZsz12Gpp_U62cZTbf-a9Id6VLHQZ8"; } }
    public static string CoinmatePrivateKey { get { return "iyt6UAxhroifDQBCkPTNhnjVL8LRI7TgPpY8AAgisQ8"; } }
    public static string CoinmateClientId { get { return "28298"; } }



    public static string BinanceApiKey { get { return "PhWiIMtXBRVucLWpsmz1TSffWN1jgpvE9TD3T7cLwPtgG1MdmLkqkx1GBr7EdzxW"; } }
    public static string BinanceSecretKey { get { return "wkLjUrU6Jfa9XbZ8kxysr7xajMq67fySuxysIjxYnhpoYi46LtUIrTkfnZC052Y1"; } }
    
    

    public static bool WriteToMongo { get { return false; } }

    public static bool ProcessTrades { get { return true; } }


}