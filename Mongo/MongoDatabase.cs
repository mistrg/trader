using System;
using MongoDB.Driver;

namespace Trader
{

    public class MongoDatabase
    {
        public void WriteTrade(Trade obj)
        {

            // get a mongoclient using the default connection string
            var mongo = new MongoClient(Config.ConnectionString);
            // get (and create if doesn't exist) a database from the mongoclient
            var db = mongo.GetDatabase("Trader");
            // get a collection of MyHelloWorldMongoThings (and create if it doesn't exist)
            // Using an empty filter so that everything is considered in the filter.
            var collection = db.GetCollection<Trade>("Trades");
            // Count the items in the collection prior to insert
            var count = collection.CountDocuments(new FilterDefinitionBuilder<Trade>().Empty);
            Console.WriteLine($"Number of items in the collection after insert: {count}");
            // Add the entered item to the collection
            collection.InsertOne(obj);
            // Count the items in the collection post insert
            count = collection.CountDocuments(new FilterDefinitionBuilder<Trade>().Empty);
            Console.WriteLine($"Number of items in the collection after insert: {count}");

        }
    }
}