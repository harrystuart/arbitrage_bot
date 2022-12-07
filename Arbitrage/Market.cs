using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class Market
    {
        public string Name { get; set; } = null!;
        public IEnumerable<Odds>? Odds { get; set; }
    }
}
