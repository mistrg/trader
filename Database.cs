
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public static class Database
{
    public static ConcurrentBag<DBItem> Items {get;set; }
    
    static Database(){
        Items =  new ConcurrentBag<DBItem>();
    } 


}



public class DBItem
{
    public double profit {get;set;}

    public DBItem()
    {
        StartDate = DateTime.Now;
    }
    
    public string Pair {get;set;}
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public TimeSpan Duration
    {
        get
        {
            if (EndDate != null)
                return EndDate.Value - StartDate;
            return DateTime.Now - StartDate;
        }
    }

    public double? askPrice { get; set; }
    public double? bidPrice { get; set; }
    public double amount { get; set; }
    public bool InPosition { get; internal set; }
    public string Exch { get; internal set; }
}