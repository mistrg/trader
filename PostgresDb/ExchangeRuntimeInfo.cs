using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trader.PostgresDb
{
    public class ExchangeRuntimeInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string ExchangeName { get; set; }

        public string BotRunId { get; set; }

        [ForeignKey(nameof(BotRunId))]
        public BotRun Botrun { get; set; }


        public long OrderBookTotalCount { get; set; }
        public long OrderBookSuccessCount { get; set; }
        public long OrderBookFailCount { get; set; }

    }

}