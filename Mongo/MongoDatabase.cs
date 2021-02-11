using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using MongoDB.Driver;
using System.Linq;

namespace Trader
{


    public sealed class MongoDatabase
    {

        private MongoClient client;
        private IMongoDatabase db;

        private static MongoDatabase instance = null;
        private static readonly object padlock = new object();


        private List<OrderCandidate> last2000OrderCandidates = new List<OrderCandidate>();

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



        public void CreateOrSkipOrderCandidate(OrderCandidate obj, bool orderProcessed)
        {
            if (!Config.WriteToMongo)
            {
                return;
            }

            if (!orderProcessed && last2000OrderCandidates.Any(p => p.BuyExchange == obj.BuyExchange && p.SellExchange == obj.SellExchange && p.Pair == obj.Pair && p.Amount == obj.Amount))
            {
                //Duplicate offer 
                return;
            }

            last2000OrderCandidates.Add(obj);

            var oversize = last2000OrderCandidates.Count - 2000;
            if (oversize > 0)
                last2000OrderCandidates.RemoveRange(0, oversize);


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