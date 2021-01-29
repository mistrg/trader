using System;

namespace Trader
{
    
 public class DBItem
    {

        public DBItem()
        {
            StartDate = DateTime.Now;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public string Pair { get; set; }
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
}
