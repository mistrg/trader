using System;
using System.Linq;

namespace Trader
{
    public static class Presenter
    {

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
        
        public static void ListResults()
        {

            var biPrice = 0;

            var opened = InMemDatabase.Instance.Items.Where(p => p.EndDate == null && !p.InPosition);
            var profitable = opened.Where(x => x.askPrice < biPrice);


            double cashRequired = 0;
            double totalamount = 0;

            foreach (var x in profitable.OrderBy(x => x.Duration))
            {
                var profitRate = Math.Round((biPrice - x.askPrice.Value) / biPrice * 100, 2);

                if (profitRate < 1.35)
                {
                    Console.Write($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm")} Volume: {Math.Round(x.amount * x.askPrice.Value, 2)} EUR  Duration: {x.Duration:hh\\:mm\\:ss}");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" Profit: {profitRate}% ");
                    Console.ResetColor();

                }
                else if (1.35 <= profitRate && profitRate < 1.4)
                {
                    Console.Write($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm")} Volume: {Math.Round(x.amount * x.askPrice.Value, 2)} EUR  Duration: {x.Duration:hh\\:mm\\:ss}");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($" Profit: {profitRate}% ");
                    Console.ResetColor();
                }
                else if (1.4 <= profitRate)
                {
                    Console.Write($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm")} Volume: {Math.Round(x.amount * x.askPrice.Value, 2)} EUR  Duration: {x.Duration:hh\\:mm\\:ss}");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" Profit: {profitRate}% ");
                    Console.ResetColor();
                    cashRequired += Math.Round(x.amount * x.askPrice.Value, 2);
                    totalamount += x.amount;
                    x.InPosition = true;
                }
            }


            if (cashRequired > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Curently required cash {Math.Round(cashRequired, 0)} Euro, Sellable {Math.Round(totalamount * biPrice)} Euro, Total profit  {Math.Round(totalamount * biPrice) - Math.Round(cashRequired, 0)} Euro");
                Console.ResetColor();
            }
        }

       
    }
}