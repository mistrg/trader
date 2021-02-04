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
            client = new MongoClient(Config.ConnectionString);
            db = client.GetDatabase("Trader");

        }



        public void CreateOrderCandidate(OrderCandidate obj)
        {

            var collection = db.GetCollection<OrderCandidate>("OrderCandidates");


            // get a collection of MyHelloWorldMongoThings (and create if it doesn't exist)
            collection.InsertOne(obj);




        }

        public static void Reset()
        {
            Instance.db = null;
            Instance.client = null;
            instance = null;
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