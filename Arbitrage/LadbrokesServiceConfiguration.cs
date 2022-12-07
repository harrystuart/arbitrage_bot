using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class LadbrokesServiceConfiguration
    {
        public string BaseEventUrl { get; set; } = null!;
        public string BaseSiteUrl { get; set; } = null!;
        public string GetEventsUrl { get; set; } = null!;
        public string GetOddsUrl { get; set; } = null!;
        public ICollection<Category> Categories { get; set; } = null!;

        public class Category
        {
            public string Id { get; set; } = null!;
            public Sport Sport { get; set; }
        }
    }
}
