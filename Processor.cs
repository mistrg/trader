using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Trader;
using Trader.Binance;
using Trader.Coinmate;

public static class Processor
{


    private static int buyTimeoutInMs = 2500;

    public static async Task ProcessOrderAsync(OrderCandidate orderCandidate)
    {

        if (!(orderCandidate.BuyExchange == nameof(Trader.Coinmate) && orderCandidate.SellExchange == nameof(Trader.Binance)))
        {
            Presenter.ShowError("Unsupported exchnages. Process cancel...");
            return;
        }


        var isSucces = await BuyLimitOrderAsync(orderCandidate);

        if (isSucces)
            await SellMarketAsync(orderCandidate);

    }

    public static async Task<bool> BuyLimitOrderAsync(OrderCandidate orderCandidate)
    {

        if (!Config.ProcessTrades)
        {
            Presenter.Warning("BuyLimitOrderAsync skipped. ProcessTrades is not activated");
            return true;
        }

        Console.WriteLine($"Let's buy limit {orderCandidate.Amount} {orderCandidate.Pair} for {orderCandidate.TotalAskPrice} on {orderCandidate.BuyExchange}");


        var coinmateLogic = new CoinmateLogic();

        //TODO: Check decimal 
        //TODO: write trade to database

        BuyResponse buyResponse = null;
        try
        {
            var pair = coinmateLogic.GetLongPair(orderCandidate.Pair);
            if (!coinmateLogic.Pairs.Any(p => p == pair))
            {
                Presenter.ShowError("Unsupported currency pair. Process cancel...");
                return false;
            }

            buyResponse = await coinmateLogic.BuyLimitOrderAsync(pair, orderCandidate.Amount, orderCandidate.UnitAskPrice, orderCandidate.Id);
        }
        catch (System.Exception ex)
        {
            Presenter.ShowPanic($"Buylimit failed. {ex.Message}. Process cancel...");
            return false;
        }

        if (buyResponse == null)
        {
            Presenter.ShowError($"Buyresponse is empty. Process cancel...");
            return false;
        }

        if (buyResponse.error)
        {
            Presenter.ShowError($"Buylimit failed. {buyResponse.errorMessage}. Process cancel...");
            return false;
        }

        if (buyResponse.data <= 0)
        {
            Presenter.ShowPanic($"Buylimit failed. Invalid order ID. Process cancel...");
            return false;
        }

        Console.WriteLine($"Waiting for buy confirmation");

        Order result = null;

        bool buySuccess = System.Threading.SpinWait.SpinUntil(() =>
        {
            try
            {
                result = coinmateLogic.GetOrderByOrderIdAsync(buyResponse.data).Result;
            }
            catch (Exception ex)
            {
                Presenter.ShowError($"GetOrderByOrderIdAsync failed. {ex} Retrying...");
            }
            return result != null && (result.status == "FILLED" || result.status == "PARTIALLY_FILLED");


        }, TimeSpan.FromMilliseconds(buyTimeoutInMs));

        if (!buySuccess)
        {
            Console.WriteLine($"Buylimit order was sent but could not be confirmed in time. Current state is {result?.status} Trying to cancel the order.");

            CancelOrderResponse buyCancelResult = null;
            try
            {
                buyCancelResult = await coinmateLogic.CancelOrderAsync(buyResponse.data);
            }
            catch (System.Exception ex)
            {
                Presenter.ShowPanic($"CancelOrderAsync failed. {ex}. Maybe buy was successful. Please check coinmate manually.Process cancel...");

                //TODO: In case of manual confirmation we could sell on binance 
                return false;
            }

            if (buyCancelResult != null && buyCancelResult.data)
                Console.WriteLine("Order was cancelled successfully.Process cancel...");
            else
            {
                Presenter.ShowPanic($"CancelOrderAsync  exited with wrong errorcode. Please check the coinmate platform manually...Process cancel...");
                //TODO: In case of manual confirmation we could sell on binance 
            }

            return false;
        }

        if (result.status == "PARTIALLY_FILLED")
        {
            if (result.remainingAmount is null)
            {
                Presenter.ShowPanic($"Partial buy was done but remaing amount is not set. Don't know how much to sell. Please check the coinmate platform manually...Process cancel...");
                return false;
            }
            orderCandidate.Amount -= result.remainingAmount.Value;
            Console.WriteLine($"Successful partial done.  Amount was updated to {orderCandidate.Amount} {orderCandidate.Pair}  on {orderCandidate.BuyExchange}.  ");

        }
        else
        {
            Console.WriteLine($"Successfuly bought {orderCandidate.Amount} {orderCandidate.Pair} for {orderCandidate.TotalAskPrice} on {orderCandidate.BuyExchange} status {result.status}");
        }

        return true;
    }
    public static async Task<bool> SellMarketAsync(OrderCandidate orderCandidate)
    {
        if (!Config.ProcessTrades)
        {
            Presenter.Warning("SellMarketAsync skipped. ProcessTrades is not activated");
            return true;
        }

        Console.WriteLine($"Let's sell market {orderCandidate.Amount} {orderCandidate.Pair} for {orderCandidate.TotalBidPrice} on {orderCandidate.SellExchange}");
        return true;

    }
}

