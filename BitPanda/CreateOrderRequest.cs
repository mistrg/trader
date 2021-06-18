namespace Trader.BitPanda
{
    public class CreateOrderRequest
    {
        public string instrument_code { get; set; }
        public string side { get; set; }
        public string type { get; set; }
        public string amount { get; set; }
        public string time_in_force {get;set;}
        public string client_id {get;set;}

        public string price {get;set;}
    }


    
}