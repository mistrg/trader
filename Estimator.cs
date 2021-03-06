using System;
using System.Collections.Generic;
using System.Linq;
using Trader.PostgresDb;

namespace Trader
{
    public class Estimator
    {

        public OrderCandidate Run(IEnumerable<DBItem> db)
        {
            foreach (var buyEntry in db.Where(p => !p.InPosition))
            {

                var sellEntrys = db.Where(sellEntry => sellEntry.Exch != buyEntry.Exch && sellEntry.Pair == buyEntry.Pair && (buyEntry.askPrice < sellEntry.bidPrice) && !sellEntry.InPosition);
                foreach (var sellEntry in sellEntrys)
                {
                    if (buyEntry.InPosition || sellEntry.InPosition)
                        continue;

                    var minimalAmount = Math.Round(Math.Min(buyEntry.amount, sellEntry.amount), 6);



                    if (minimalAmount <= 0.0005)
                        continue; //Too small ~13,92 Euro


                    if (sellEntry.bidPrice.Value * minimalAmount <= 11) // Price more then 11 Euros
                        continue;
                    if (buyEntry.askPrice.Value * minimalAmount <= 11) // Price more then 11 Euros
                        continue;


                    var estProfitGross = Math.Round(sellEntry.bidPrice.Value * minimalAmount - buyEntry.askPrice.Value * minimalAmount, 2);




                    var estBuyFee = Math.Round(buyEntry.askPrice.Value * minimalAmount * buyEntry.TakerFeeRate, 2);

                    var estSellFee = Math.Round(sellEntry.bidPrice.Value * minimalAmount * sellEntry.TakerFeeRate, 2);


                    var estProfitNet = Math.Round(estProfitGross - estBuyFee - estSellFee, 2);

                    var estProfitNetRate = Math.Round(100 * estProfitNet / (sellEntry.bidPrice.Value * minimalAmount), 2);

                    if (estProfitNetRate <= 0)
                        continue;


                    buyEntry.InPosition = true;
                    sellEntry.InPosition = true;


                    var oc = new OrderCandidate()
                    {
                        WhenBuySpoted = buyEntry.StartDate,
                        WhenSellSpoted = sellEntry.StartDate,
                        BuyExchange = buyEntry.Exch,
                        SellExchange = sellEntry.Exch,
                        Pair = buyEntry.Pair,
                        UnitAskPrice = buyEntry.askPrice.Value,
                        TotalAskPrice = Math.Round(buyEntry.askPrice.Value * minimalAmount, 2),
                        Amount = minimalAmount,
                        UnitBidPrice = sellEntry.bidPrice.Value,
                        TotalBidPrice = Math.Round(sellEntry.bidPrice.Value * minimalAmount, 2),
                        EstProfitGross = estProfitGross,
                        EstProfitNet = estProfitNet,
                        EstProfitNetRate = estProfitNetRate,
                        EstBuyFee = estBuyFee,
                        EstSellFee = estSellFee,
                        BotVersion = Config.Version,
                        BotRunId = Config.RunId
                    };
                    return oc;


                }
            }
            return null;
        }
    }
}