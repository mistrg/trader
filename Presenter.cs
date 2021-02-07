using System;
using System.Linq;
using System.Threading;

namespace Trader
{
    public static class Presenter
    {

        public static void PrintOrderCandidate(OrderCandidate oc)
        {
            //Console.ResetColor();
            
            // if (oc.EstProfitNetRate > 1.5)
            //     Console.ForegroundColor = ConsoleColor.DarkGreen;
            // else if (oc.EstProfitNetRate > 0.5)
            //     Console.ForegroundColor = ConsoleColor.DarkYellow;
            // else
            //     Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"{oc.WhenCreated.ToString("dd.MM.yyyy HH:mm:ss")} OCID: {oc.Id} Buy {oc.Amount} {oc.Pair.Substring(0, 3)} on {oc.BuyExchange} for {oc.TotalAskPrice} {oc.Pair.Substring(3, 3)} and sell on {oc.SellExchange} for {oc.TotalBidPrice} {oc.Pair.Substring(3, 3)} and make estNetProfit {oc.EstProfitNet} {oc.Pair.Substring(3, 3)} estSellfee {oc.EstSellFee} estProfitNetRate {oc.EstProfitNetRate}%");



            // Console.WriteLine($" ");
             //Console.ResetColor();

        }
        public static void ShowPanic(string error)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(error);
            Console.ResetColor();
        }
        public static void ShowError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
        }

        public static void Warning(string warning)
        {

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(warning);
            Console.ResetColor();
        }

        internal static void ShowSuccess(string successMessage)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(successMessage);
            Console.ResetColor();
        }
    }
}