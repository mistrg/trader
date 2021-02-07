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


        var isSuccess = await BuyLimitOrderAsync(orderCandidate);

        if (isSuccess)
            await SellMarketAsync(orderCandidate);

    }

    public static async Task<bool> BuyLimitOrderAsync(OrderCandidate orderCandidate)
    {

        if (!Config.ProcessTrades)
        {
            Presenter.Warning("BuyLimitOrderAsync skipped. ProcessTrades is not activated");
            return true;
        }

        Presenter.PrintOrderCandidate(orderCandidate);


        var coinmateLogic = new CoinmateLogic();



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
            Presenter.ShowPanic($"Buylimit failed. {ex}. Process cancel...");
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

        if (buyResponse.data == null || buyResponse.data <= 0)
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
                result = coinmateLogic.GetOrderByOrderIdAsync(buyResponse.data.Value).Result;
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
                buyCancelResult = await coinmateLogic.CancelOrderAsync(buyResponse.data.Value);
            }
            catch (System.Exception ex)
            {
                Presenter.ShowPanic($"CancelOrderAsync failed. {ex}. Maybe buy was successful. Please check coinmate manually and decide what to do.");

                Console.Write("Do you want to continue with selling? y = yes / n = no");
                var val = Console.ReadLine();
                if (val == "y")
                {
                    Console.WriteLine("Ok. Going to sell...");
                    return true;
                }
                Console.WriteLine("Ok. Process cancel...");

                return false;
            }

            if (buyCancelResult != null && buyCancelResult.data)
                Console.WriteLine("Order was cancelled successfully.Process cancel...");
            else
            {
                Presenter.ShowPanic($"CancelOrderAsync  exited with wrong errorcode. Please check coinmate manually and decide what to do.");
                Console.Write("Do you want to continue with selling? y = yes / n = no");
                var val = Console.ReadLine();
                if (val == "y")
                {
                    Console.WriteLine("Ok. Going to sell...");
                    return true;
                }
                Console.WriteLine("Ok. Process cancel...");
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
            Console.WriteLine($"Successful partial done.  Amount was updated to {orderCandidate.Amount}.  {orderCandidate.Pair}  on {orderCandidate.BuyExchange}. Order type {result.orderTradeType} ");

        }
        else
        {
            Console.WriteLine($"Successfuly bought {orderCandidate.Amount} {orderCandidate.Pair} for {orderCandidate.TotalAskPrice} on {orderCandidate.BuyExchange} status {result.status} Order type {result.orderTradeType} ");
        }

        return true;
    }
    public static async Task<bool> SellMarketAsync(OrderCandidate orderCandidate)
    {

        if (!Config.ProcessTrades)
        {
            Presenter.Warning("BuyLimitOrderAsync skipped. ProcessTrades is not activated");
            return true;
        }

        Presenter.PrintOrderCandidate(orderCandidate);


        var binanceLogic = new BinanceLogic();

        OrderResponse result = null;
        try
        {
            result = await binanceLogic.SellMarketAsync(orderCandidate);

        }
        catch (System.Exception ex)
        {
            Presenter.ShowPanic($"SellMarketAsync failed. {ex}. Please check binance manually.");
            return false;
        }


        // bool buySuccess = System.Threading.SpinWait.SpinUntil(() =>
        // {
        //     try
        //     {
        //         result = coinmateLogic.GetOrderByOrderIdAsync(buyResponse.data.Value).Result;
        //     }
        //     catch (Exception ex)
        //     {
        //         Presenter.ShowError($"GetOrderByOrderIdAsync failed. {ex} Retrying...");
        //     }
        //     return result != null && (result.status == "FILLED" || result.status == "PARTIALLY_FILLED");


        // }, TimeSpan.FromMilliseconds(buyTimeoutInMs));


        if (result == null)
            Presenter.ShowPanic($"SellMarketAsync failed. Result is null.");

        Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}  OrderId {result.orderId} OCID {result.clientOrderId} {result.side} {result.type} price: {result.price} symbol: {result.symbol} Qty: {result.executedQty}/{result.origQty} cumQty: {result.cummulativeQuoteQty}");

        if (result.status == "FILLED")
            Console.WriteLine("Successfully sold");
        else
            Console.WriteLine("Check line above for problems");


        return result.status == "FILLED";

    }
}

