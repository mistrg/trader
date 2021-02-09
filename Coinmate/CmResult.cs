
using System.Collections.Generic;

namespace Trader.Coinmate
{
    public class CmResult
    {
        public string channel { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("event")]
        public string Event { get; set; }

        public Payload payload { get; set; }






    }
    public class Payload
    {

        public List<PayAmount> bids { get; set; }
        public List<PayAmount> asks { get; set; }




    }

    public class PayAmount
    {
        public double price { get; set; }
        public double amount { get; set; }
    }

}