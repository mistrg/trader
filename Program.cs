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

        private static readonly HttpClient httpClient = new HttpClient();



        static async Task Main(string[] args)
        {


            Console.WriteLine("Trader version 1 starting!");
            await new Coinmate().ListenToOrderbookAsync(CancellationToken.None);
            // Type your username and press enter
            //     Console.WriteLine("Enter polling interval [milisec]:");

            //     // Create a string variable and get user input from the keyboard and store it in the variable
            //     int  timeout = int.Parse(Console.ReadLine());


            //     var uriCM = "https://coinmate.io/api/ticker?currencyPair=BTC_EUR";
            //     var uriBI = "https://api.binance.com/api/v3/ticker/price?symbol=BTCEUR";
            //     var count = 0;
            //     var cmCheap = 0;
            //     var biCheap = 0;


            //     do
            //     {
            //         while (!Console.KeyAvailable)
            //         {

            //             count++;
            //             try
            //             {
            //                 var result = await httpClient.GetFromJsonAsync<CMTickerResult>(uriCM);
            //                 var resultBI = await httpClient.GetFromJsonAsync<BIResult>(uriBI);
            //                 var cmPrice = result.data.ask;

            //                 Console.WriteLine($"#{count} Coinmate ASK [Eur] {result.data.bid} BID [Eur] {cmPrice}  24H [Eur] {result.data.high} 24L [Eur] {result.data.low}");
            //                 Console.WriteLine($"#{count} Binance Price [Eur] {resultBI.price}");

            //                 if (cmPrice - resultBI.price >= 0)
            //                 {
            //                     biCheap++;
            //                     Console.ForegroundColor = ConsoleColor.Red;
            //                     Console.WriteLine($"#{count}  Price delta ABS [Eur] {cmPrice - resultBI.price}  REL {Math.Round((cmPrice - resultBI.price)*100/cmPrice,2)}%");                            Console.ResetColor();
            //                 }
            //                 else
            //                 {
            //                     cmCheap++;
            //                     Console.ForegroundColor = ConsoleColor.Green;
            //                     Console.WriteLine($"#{count}  Price delta ABS [Eur] {cmPrice - resultBI.price} REL {Math.Round((cmPrice - resultBI.price)*100/cmPrice,2)}%");
            //                     Console.ResetColor();

            //                 }

            //                 await Task.Delay(timeout).ConfigureAwait(false);
            //             }
            //             catch (HttpRequestException) // Non success
            //             {
            //                 Console.WriteLine("An error occurred.");
            //             }
            //             catch (NotSupportedException) // When content type is not valid
            //             {
            //                 Console.WriteLine("The content type is not supported.");
            //             }
            //             catch (JsonException) // Invalid JSON
            //             {
            //                 Console.WriteLine("Invalid JSON.");
            //             }



            //         }
            //     } while (Console.ReadKey(true).Key != ConsoleKey.Escape);



            //     Console.WriteLine($"Coinmate was cheaper in {100 * cmCheap / count}% cases, Binance  in {100 * biCheap / count}%");


            // }


        }
    }
}
