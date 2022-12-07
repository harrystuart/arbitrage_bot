using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class BookkeeperEvent
    {
        public Bookkeeper Bookkeeper { get; set; }
        public string BookkeeperEventId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Competition { get; set; }
        public DateTimeOffset? Commencement { get; set; }
        public string? Url { get; set; }
    }
}
