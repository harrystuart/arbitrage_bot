using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class BookkeeperMarket
    {
        public Bookkeeper Bookkeeper { get; set; }
        public string BookkeeperMarketId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Url { get; set; }
    }
}
