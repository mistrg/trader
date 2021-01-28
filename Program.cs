using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Trader
{
    public class CMTickerResult
    {
        public bool error { get; set; }
        public string errorMessage { get; set; }

        public CMTickerResultData data { get; set; }

        public class CMTickerResultData
        {

            public double last { get; set; }
            public double high { get; set; }
            public double low { get; set; }
            public double amount { get; set; }
            public double bid { get; set; }
            public double ask { get; set; }
            public double change { get; set; }
            public double open { get; set; }
            public double timestamp { get; set; }


        }

    }

    public class BIResult
    {
        public string symbol { get; set; }
        public double price { get; set; }


    }

    class Program
    {




        static async Task  Main(string[] args)
        {


            Console.WriteLine("Trader version 2 starting!");
             new Coinmate().ListenToOrderbook(CancellationToken.None);
           
            while(true)
            {
                await Task.Delay(1000);
                Console.WriteLine($"Database size: {Database.Items.Count} records");
            }

        }
    }
}
