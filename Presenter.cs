using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trader.Email;

namespace Trader
{
    public class Presenter
    {


        private readonly IMailer _mailer;

        public Presenter(IMailer mailer)
        {
            _mailer = mailer;
            
        }
        public async Task  SendMessageAsync(string message)
        {
            await _mailer.SendEmailAsync("zdenek@drbalek.de", "Drbor Report", message);

        }

        public void PrintOrderCandidate(OrderCandidate oc)
        {
            Console.WriteLine($"{oc.WhenCreated.ToString("dd.MM.yyyy HH:mm:ss")} OCID: {oc.Id} Buy {oc.Amount} {oc.Pair.Substring(0, 3)} on {oc.BuyExchange} for {oc.TotalAskPrice} {oc.Pair.Substring(3, 3)} and sell on {oc.SellExchange} for {oc.TotalBidPrice} {oc.Pair.Substring(3, 3)} and make estNetProfit {oc.EstProfitNet} {oc.Pair.Substring(3, 3)} estSellfee {oc.EstSellFee} estProfitNetRate {oc.EstProfitNetRate}%");

        }
        public void ShowPanic(string error)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(error);
            Console.ResetColor();
        }
        public void ShowError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
        }

        public void Warning(string warning)
        {

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(warning);
            Console.ResetColor();
        }

        internal void ShowSuccess(string successMessage)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(successMessage);
            Console.ResetColor();
        }
    }
}