using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class BookkeeperOdds
    {
        public Bookkeeper Bookkeeper { get; set; }
        public string BookkeeperOddsId { get; set; } = null!;
        public string Outcome { get; set; } = null!;
        public double Value { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
    }
}
