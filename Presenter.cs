using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Trader.Email;
using Trader.PostgresDb;

namespace Trader
{
    public class Presenter
    {


        private readonly IMailer _mailer;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public Presenter(IMailer mailer, IConfiguration config)
        {
            _mailer = mailer;
            _config = config;

            var logDirectory = _config.GetValue<string>("Runtime:LogOutputDirectory");
            _logger = new LoggerConfiguration()
                .WriteTo.File(logDirectory, rollingInterval: RollingInterval.Day)
                .CreateLogger();


        }
        public async Task SendMessageAsync(string subject, string message, Arbitrage arbitrage = null)
        {
            message = "<h2>" + message + "</h2>";
            if (arbitrage != null)
            {

                message += " <br/><br/><p>Arbitrage detail</p>";
                message += " <table  border=\"1\" >";


                try
                {
                    PropertyInfo[] properties = typeof(Arbitrage).GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        message += $"<tr><td><strong>{property.Name}</strong></td><td>{property.GetValue(arbitrage)}</td></tr>";
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.Error(ex.ToString());
                }
                message += " </table>";



            }


            var r = await _mailer.SendEmailAsync(subject, message);
            if (!string.IsNullOrWhiteSpace(r))
                ShowError(r);

        }

        public void PrintOrderCandidate(OrderCandidate oc)
        {
            var message = $"{oc.WhenCreated.ToString("dd.MM.yyyy HH:mm:ss")} OCID: {oc.Id} Buy {oc.Amount} {oc.Pair.Substring(0, 3)} on {oc.BuyExchange} for {oc.TotalAskPrice} {oc.Pair.Substring(3, 3)} and sell on {oc.SellExchange} for {oc.TotalBidPrice} {oc.Pair.Substring(3, 3)} and make estNetProfit {oc.EstProfitNet} {oc.Pair.Substring(3, 3)} estSellfee {oc.EstSellFee} estProfitNetRate {oc.EstProfitNetRate}%";
            Console.WriteLine(message);
            _logger.Information(message);

        }
        public void ShowPanic(string error)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(error);
            _logger.Fatal(error);

            Console.ResetColor();
        }
        public void ShowError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            _logger.Error(error);

            Console.ResetColor();
        }

        public void Warning(string warning)
        {

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(warning);
            _logger.Warning(warning);

            Console.ResetColor();
        }

        internal void ShowSuccess(string successMessage)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(successMessage);
            _logger.Information(successMessage);
            Console.ResetColor();
        }
        public void ShowInfo(string message)
        {
            Console.ResetColor();
            Console.WriteLine(message);
            _logger.Information(message);

            Console.ResetColor();
        }
    }
}