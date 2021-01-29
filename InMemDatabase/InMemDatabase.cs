
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trader
{
    public static class InMemDatabase
    {
        public static ConcurrentBag<DBItem> Items { get; set; }

        static InMemDatabase()
        {
            Items = new ConcurrentBag<DBItem>();
        }


    }



   
}