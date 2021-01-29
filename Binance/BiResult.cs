using System.Collections.Generic;

public class BIResult
{

    public long lastUpdateId { get; set; }

    public string[][] bids { get; set; }
    public string[][] asks { get; set; }


    

    // public class OrderItem
    // {
    //     public List<string> item { get; set; }


    //     public double price { get { return double.Parse(item[1]); } }
    //     public double amount { get { return double.Parse(item[0]); } }
    // }


}