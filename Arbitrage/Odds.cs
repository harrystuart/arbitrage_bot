using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class Odds
    {
        public string Outcome { get; set; } = null!;
        public IEnumerable<BookkeeperOdds> BookkeeperOdds { get; set; } = null!;
    }
}
