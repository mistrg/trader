using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trader;
using Trader.Infrastructure;
using Trader.PostgresDb;

public interface IExchangeLogic
{
    Task SaveTelemetryAsync();
    Task<double> GetTradingTakerFeeRateAsync();
    
    Task<Tuple<double?, double?>> GetAvailableAmountAsync(string currencyPair);
    Task<bool> BuyLimitOrderAsync(Arbitrage arbitrage);
    Task<bool> SellMarketAsync(Arbitrage orderCandidate);

    Task<List<DBItem>> GetOrderBookAsync();

    Task PrintAccountInformationAsync();

}