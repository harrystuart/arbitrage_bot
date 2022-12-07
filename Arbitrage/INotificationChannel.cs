using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public interface INotificationChannel
    {
        public bool Notify(Sport sport, Event @event, Market market, IEnumerable<BookkeeperMarket> bookkeeperMarkets, double arbitrage, IEnumerable<BookkeeperOdds> bestBookkeeperOdds);
    }
}
