using System.Collections.Generic;

namespace Trader.BitBay
{
public class QueryRequest
    {
        public List<string> markets { get; set; }
        public List<object> limit { get; set; }
        public List<object> offset { get; set; }
        public string fromTime { get; set; }
        public string toTime { get; set; }
        public List<object> userId { get; set; }
        public string offerId { get; set; }
        public List<object> initializedBy { get; set; }
        public List<object> rateFrom { get; set; }
        public List<object> rateTo { get; set; }
        public List<object> userAction { get; set; }
        public List<object> nextPageCursor { get; set; }
    }
    
}