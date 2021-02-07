
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trader
{

    public sealed class InMemDatabase
    {

        public ConcurrentBag<DBItem> Items { get; set; }
        public ConcurrentDictionary<long, OrderCandidate> OrderCandidates { get; internal set; }

        private static InMemDatabase instance = null;
        private static readonly object padlock = new object();

        InMemDatabase()
        {
            Items = new ConcurrentBag<DBItem>();
            OrderCandidates = new ConcurrentDictionary<long, OrderCandidate>();

        }

        public static InMemDatabase Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new InMemDatabase();
                    }
                    return instance;
                }
            }
        }

    }



}