using System;
using MongoDB.Driver;

namespace Trader
{


 public sealed class MongoDatabase
    {

        public MongoClient client;
        

        private static MongoDatabase instance = null;
        private static readonly object padlock = new object();

        MongoDatabase()
        {
            client= new MongoClient(Config.ConnectionString);

        }


        public void WriteTrade(Trade obj)
        {
            
            var db = client.GetDatabase("Trader");
            // get a collection of MyHelloWorldMongoThings (and create if it doesn't exist)
            // Using an empty filter so that everything is considered in the filter.
            var collection = db.GetCollection<Trade>("Trades");
            // Count the items in the collection prior to insert
            // Add the entered item to the collection
            collection.InsertOne(obj);
            // Count the items in the collection post insert

        }

        public static MongoDatabase Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MongoDatabase();
                    }
                    return instance;
                }
            }
        }
    }

}