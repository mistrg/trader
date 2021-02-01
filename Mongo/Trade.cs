using System;

public class Trade
{
    public Guid BuyId { get; set; }
    public Guid SellId { get; set; }

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


    public double ProfitAbs { get; set; }


    public double ProfitRate { get; set; }
    public string BotRunId { get; internal set; }
    public int BotVersion { get; internal set; }
    public double ProfitReal { get; internal set; }

    public Trade()
    {
        WhenCreated = DateTime.Now;
    }

}