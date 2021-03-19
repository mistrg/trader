using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trader.PostgresDb
{

    public class BotRun
    {
        public BotRun()
        {
            WhenCreated = DateTime.Now;
        }

        [Key]
        public string Id { get; set; }

        public DateTime WhenCreated {get;set;}
        public int Version { get; set; }
    }
}