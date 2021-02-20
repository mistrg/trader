using System.Threading.Tasks;

public interface IExchangeLogic
{
    double GetTradingTakerFeeRate();
    Task<double> GetAvailableAmountAsync(string currency);
}