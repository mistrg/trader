namespace Trader.BitFlyer
{
    public class GetBalanceResponse
    {
        public string currency_code { get; set; }
        public double amount { get; set; }
        public double available { get; set; }
    }


    
}