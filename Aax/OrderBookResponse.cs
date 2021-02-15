using System.Collections.Generic;

namespace Trader.Aax
{
    
    public class OrderBookResponse 
    {

            public List<List<string>> asks { get; set; } 
            public List<List<string>> bids { get; set; } 
            public string e { get; set; } 
            public long t { get; set; } 

    }
}
