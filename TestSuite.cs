using System;
using System.Threading.Tasks;

public static class TestSuite
{

    public static async Task TestLowBuyAsync()
    {
        OrderCandidate oc = new OrderCandidate()
        {
            Amount = 1, 
            Pair = "BTCEUR",
            TotalAskPrice = 1, 
            BuyExchange = nameof(Trader.Coinmate), 
            UnitAskPrice = 1

        };


        var result = await Processor.BuyLimitOrderAsync(oc);
        Console.WriteLine($"Test TestLowBuyAsync result: {result}");
    }

    public static async Task TestLowSellAsync()
    {
        OrderCandidate oc = new OrderCandidate()
        {
            Amount = 0.00030, //10 Euro
            Pair = "BTCEUR",
            BuyExchange = nameof(Trader.Binance), 
        };


        var result = await Processor.SellMarketAsync(oc);
        Console.WriteLine($"Test TestLowSellAsync result: {result}");
    }


    
}