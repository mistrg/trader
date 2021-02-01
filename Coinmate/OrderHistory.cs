namespace Trader.Coinmate
{

    public class OrderHistory 
    {
        public long id {get;set;}
        public long timestamp {get;set;}
        public string type {get;set;}
        public double? price {get;set;}
        public double? remainingAmount {get;set;}
        public double? originalAmount {get;set;}
        public string status {get;set;}

        public double? stopPrice {get;set;}
        public string orderTradeType {get;set;}
        public bool hidden {get;set;}
        public double? avgPrice {get;set;}
        public bool trailing {get;set;}
    }
}
