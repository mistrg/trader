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
        Console.WriteLine($"Test result: {result}");
    }
}