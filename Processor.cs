using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Trader;
using Trader.Binance;
using Trader.Coinmate;

public static class Processor
{


    private static int buyTimeout = 2500;

    public static async Task ProcessOrderAsync(OrderCandidate orderCandidate)
    {

        Console.WriteLine($"Let's buy {orderCandidate.Amount} {orderCandidate.Pair} for {orderCandidate.TotalAskPrice} on {orderCandidate.BuyExchange}");

        if (!(orderCandidate.BuyExchange == nameof(Trader.Coinmate) && orderCandidate.SellExchange == nameof(Trader.Binance)))
        {
            Presenter.ShowError("Unsupported exchnages. Process cancel...");
            return;
        }

        var coinmateLogic = new CoinmateLogic();

        //Check pairs
        //Check decimal 
        //Check which price
        BuyResponse buyResponse = null;
        try
        {
            buyResponse = await coinmateLogic.BuyLimitOrderAsync(orderCandidate.Pair, orderCandidate.Amount, orderCandidate.UnitAskPrice);
        }
        catch (System.Exception ex)
        {
            Presenter.ShowPanic($"Buylimit failed. {ex.Message}. Process cancel...");
            return;
        }

        if (buyResponse.error)
        {
            Presenter.ShowError($"Buylimit failed. {buyResponse.errorMessage}. Process cancel...");
            return;
        }

        if (buyResponse.data <= 0)
        {
            Presenter.ShowPanic($"Buylimit failed. Invalid order ID. Process cancel...");
            return;
        }

        Console.WriteLine($"Waiting for buy confirmation");

        bool buySuccess = System.Threading.SpinWait.SpinUntil(() =>
        {
            Order result = null;
            try
            {
                result = coinmateLogic.GetOrderByOrderIdAsync(buyResponse.data).Result;
            }
            catch (Exception ex)
            {
                Presenter.ShowError($"GetOrderByOrderIdAsync failed. {ex} Retrying...");
            }
            return result != null && result.status == "FILLED";


        }, TimeSpan.FromMilliseconds(buyTimeout));

        if (!buySuccess)
        {
            Console.WriteLine($"Buylimit order was sent but could not be confirmed in time. Trying to cancel the order.");

            CancelOrderResponse buyCancelResult = null;
            try
            {
                buyCancelResult = await coinmateLogic.CancelOrderAsync(buyResponse.data);
            }
            catch (System.Exception ex)
            {
                Presenter.ShowPanic($"CancelOrderAsync failed. {ex}. Please check the coinmate platform manually...");
            }

            if (buyCancelResult != null && buyCancelResult.data)
                Console.WriteLine("Order was cancelled successfully.");
            else
                Presenter.ShowPanic($"CancelOrderAsync  exited with wrong errorcode. Please check the coinmate platform manually...");

            return;
        }

        Console.WriteLine($"Successfuly bought {orderCandidate.Amount} {orderCandidate.Pair} for {orderCandidate.TotalAskPrice} on {orderCandidate.BuyExchange}");
        Console.WriteLine($"Let's sell {orderCandidate.Amount} {orderCandidate.Pair} for {orderCandidate.TotalBidPrice} on {orderCandidate.SellExchange}");





        var binanceLogic = new BinanceLogic();



    }
}

