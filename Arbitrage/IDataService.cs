using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public interface IDataService
    {
        public Task InitialiseAsync();
        public Task<Dictionary<Event, IEnumerable<BookkeeperEvent>>> ReconcileSportEventsAsync(Sport sport);
        public Task<Dictionary<Market, IEnumerable<BookkeeperMarket>>> ReconcileEventMarketsAsync(Event @event, IEnumerable<BookkeeperEvent> bookkeeperEvents);
        public Task<IEnumerable<Odds>> ReconcileMarketOddsAsync(Event @event, IEnumerable<BookkeeperMarket> bookkeeperMarkets);
    }
}
