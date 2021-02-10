
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trader
{

    public sealed class InMemDatabase
    {

        public List<DBItem> Items { get; set; }

        private static InMemDatabase instance = null;
        private static readonly object padlock = new object();

        InMemDatabase()
        {
            Items = new List<DBItem>();

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