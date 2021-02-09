using System;
using System.Threading.Tasks;

public class TestSuite
{
    private readonly Processor _processor;

    public TestSuite(Processor processor)
    {
        _processor = processor;
    }
    public async Task TestLowBuyAsync()
    {
        OrderCandidate oc = new OrderCandidate()
        {
            Amount = 1,
            Pair = "BTCEUR",
            TotalAskPrice = 1,
            BuyExchange = nameof(Trader.Coinmate),
            UnitAskPrice = 1

        };


        var result = await _processor.BuyLimitOrderAsync(oc);
        Console.WriteLine($"Test TestLowBuyAsync result: {result}");
    }

    public async Task TestLowSellAsync()
    {
        var amount = Math.Round(0.09335303, 6);
        OrderCandidate oc = new OrderCandidate()
        {
            Amount = amount,
            Pair = "BTCEUR",
            BuyExchange = nameof(Trader.Binance),
        };


        var result = await _processor.SellMarketAsync(oc);
        Console.WriteLine($"Test TestLowSellAsync result: {result}");
    }







}