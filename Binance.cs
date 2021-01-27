using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Trader;

public static class Binance
{
    static string uriBI = "https://api.binance.com/api/v3/ticker/price?symbol=BTCEUR";
    private static readonly HttpClient httpClient = new HttpClient();

    public static async Task<double> GetPriceAsync()
    {
        var resultBI = await httpClient.GetFromJsonAsync<BIResult>(uriBI);

        return resultBI?.price ?? 0;
    }

}