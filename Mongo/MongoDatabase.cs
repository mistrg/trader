using System;
using System.Net.Sockets;
using System.Threading;
using MongoDB.Driver;

namespace Trader
{


    public sealed class MongoDatabase
    {

        private int retries = 0;
        private MongoClient client;
        private IMongoDatabase db;
        private IMongoCollection<OrderCandidate> collection;

        private static MongoDatabase instance = null;
        private static readonly object padlock = new object();

        MongoDatabase()
        {
            InitializeDb();
        }



        private void InitializeDb()
        {
            client = new MongoClient(Config.ConnectionString);
            db = client.GetDatabase("Trader");
            collection = db.GetCollection<OrderCandidate>("OrderCandidates");


        }


        public void CreateOrderCandidate(OrderCandidate obj)
        {
            try
            {
                // get a collection of MyHelloWorldMongoThings (and create if it doesn't exist)
                collection.InsertOne(obj);

                retries = 0;
            }
            catch (System.Exception ex)
            {

                retries++;
                if (retries >= 10)
                {
                    throw ex;
                }
                Console.WriteLine($"Connection problem. Retrying {retries}/10");

                Thread.Sleep(1000);
                InitializeDb();
                CreateOrderCandidate(obj);

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