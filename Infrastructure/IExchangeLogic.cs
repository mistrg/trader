using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trader;
using Trader.Infrastructure;
using Trader.PostgresDb;

public interface IExchangeLogic
{
    double GetTradingTakerFeeRate();
     Task<Tuple<double?,double?>> GetAvailableAmountAsync(string currencyPair);
    Task<Tuple<bool, BuyResult>> BuyLimitOrderAsync(OrderCandidate orderCandidate);
    Task<Tuple<bool, SellResult>> SellMarketAsync(OrderCandidate orderCandidate);

     Task<List<DBItem>> GetOrderBookAsync();

     Task PrintAccountInformationAsync();

}