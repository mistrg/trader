using System;

public class OrderCandidate
{
    public long Id {get;}
    public DateTime WhenCreated { get; set; }

    public DateTime WhenBuySpoted { get; set; }
    public DateTime WhenSellSpoted { get; set; }

    public string BuyExchange { get; set; }
    public string SellExchange { get; set; }
    public string Pair { get; set; }


    public double UnitAskPrice { get; set; }

    public double TotalAskPrice { get; set; }

    public double Amount { get; set; }


    public double UnitBidPrice { get; set; }

    public double TotalBidPrice { get; set; }


    public double EstProfitGross { get; set; }

    public double EstProfitNet { get; internal set; }

    public double EstProfitNetRate { get; set; }
    public string BotRunId { get; internal set; }
    public int BotVersion { get; internal set; }
    public double EstBuyFee {get;set;}

    public double EstSellFee {get;set;}

    public OrderCandidate()
    {
        WhenCreated = DateTime.Now;
        Id = DateTime.UtcNow.Ticks;
    }

}