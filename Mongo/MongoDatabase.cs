using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using MongoDB.Driver;

namespace Trader
{


    public sealed class MongoDatabase
    {

        private MongoClient client;
        private IMongoDatabase db;

        private static MongoDatabase instance = null;
        private static readonly object padlock = new object();

        MongoDatabase()
        {
            if (Config.WriteToMongo)
            {
                client = new MongoClient(Config.ConnectionString);
                db = client.GetDatabase("Trader");
            }
            else
            {
                Console.WriteLine("CreateOrderCandidate skipped. WriteToMongo is not activated");

            }

        }



        public void CreateOrderCandidate(OrderCandidate obj)
        {
            if (!Config.WriteToMongo)
            {
                return;
            }
            var collection = db.GetCollection<OrderCandidate>("OrderCandidatesV2");

            collection.InsertOne(obj);

        }

        public static void Reset()
        {
            if (Config.WriteToMongo)
            {
                Instance.db = null;
                Instance.client = null;
                instance = null;
            }
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