using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class BookerkeeperEntityComparerConfiguration
    {
        public Dictionary<Bookkeeper, Dictionary<Sport, Dictionary<string, List<string>>>> KnownCorrespondingMarketNames { get; set; } = null!;
        public Dictionary<Bookkeeper, Dictionary<Sport, Dictionary<string, List<string>>>> KnownCorrespondingOddsNames { get; set; } = null!;
    }
}
