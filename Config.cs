public static class Config
{
    public static string ConnectionString { get { return "mongodb+srv://dbadmin:yYzEvE9dqsYqgNSg@mongcluster.5uwqc.mongodb.net/<dbname>?connect=replicaSet&retryWrites=true&w=majority"; } }
    public static string CoinmatePublicKey { get { return "Ik7aDMVzVhPh5tZsz12Gpp_U62cZTbf-a9Id6VLHQZ8"; } }
    public static string CoinmatePrivateKey { get { return "iyt6UAxhroifDQBCkPTNhnjVL8LRI7TgPpY8AAgisQ8"; } }
    public static string CoinmateClientId { get { return "28298"; } }



    public static string BinanceApiKey { get { return "PhWiIMtXBRVucLWpsmz1TSffWN1jgpvE9TD3T7cLwPtgG1MdmLkqkx1GBr7EdzxW"; } }
    public static string BinanceSecretKey { get { return "wkLjUrU6Jfa9XbZ8kxysr7xajMq67fySuxysIjxYnhpoYi46LtUIrTkfnZC052Y1"; } }



    public static string PostgresConnectionString { get { return "Server = ec2-34-252-251-16.eu-west-1.compute.amazonaws.com; Port = 5432; Database = dan85i13ot0ne0; User Id = pablawtgfsvjlv; Password = c918a98d9c1b947003a3f5810c2416f749517740c1522100f9e7089b0e5cb6b6; SslMode=Require; Trust Server Certificate=true"; } }

    public static bool WriteToMongo { get { return true; } }


    public static bool AutomatedTrading { get { return true; } }
    public static double AutomatedTradingMinEstimatedProfitNetRate { get { return 0.49; } }




    public static bool ProcessTrades { get { return true; } }

    public static int BuyTimeoutInMs { get { return 2500; } }
    public static int SellTimeoutInMs { get { return 5000; } }


}