using System;
using MongoDB.Driver;

namespace Trader
{

    public static class MongoDatabase
    {
        public static MongoClient client;
        static MongoDatabase()
        {
            client= new MongoClient(Config.ConnectionString);
        }

        public static void WriteTrade(Trade obj)
        {
            
            var db = client.GetDatabase("Trader");
            // get a collection of MyHelloWorldMongoThings (and create if it doesn't exist)
            // Using an empty filter so that everything is considered in the filter.
            var collection = db.GetCollection<Trade>("Trades");
            // Count the items in the collection prior to insert
            var count = collection.CountDocuments(new FilterDefinitionBuilder<Trade>().Empty);
            // Add the entered item to the collection
            collection.InsertOne(obj);
            // Count the items in the collection post insert
            count = collection.CountDocuments(new FilterDefinitionBuilder<Trade>().Empty);

        }
    }
}