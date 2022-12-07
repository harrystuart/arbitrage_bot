using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class UnibetServiceConfiguration
    {
        public string BaseEventUrl { get; set; } = null!;
        public string BaseSiteUrl { get; set; } = null!;
        public string GetOddsUrl { get; set; } = null!;
        public Dictionary<Sport, string> GetSportEventsUrls { get; set; } = null!;
    }
}
