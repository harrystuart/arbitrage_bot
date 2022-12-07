using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class Event
    {
        public string Name { get; set; } = null!;
        public string? Competition { get; set; }
        public Sport Sport { get; set; }
        public IEnumerable<Market>? Markets { get; set; }
        public DateTimeOffset? Commencement { get; set; }
        public string TMPUnibetEventId { get; set; }
    }
}
