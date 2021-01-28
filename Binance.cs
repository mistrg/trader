using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Trader;

public static class Binance
{
        public class BIResult
    {
        public string symbol { get; set; }
        public double bidPrice { get; set; }
        public double bidQty { get; set; }

    }
    
    static string uriBI = "https://api.binance.com/api/v3/ticker/bookTicker";
    private static readonly HttpClient httpClient = new HttpClient();

    public static async Task<double> GetBidPriceAsync(string symbol)
    {
        var resultBI = await httpClient.GetFromJsonAsync<BIResult>(uriBI+"?symbol="+symbol);

        return resultBI?.bidPrice ?? 0;
    }

}