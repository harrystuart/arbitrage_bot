using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class Engine
    {
        private IDataService mDataService { get; set; }
        private IEnumerable<INotificationChannel> mNotificationChannels { get; set; }

        public Engine(IDataService dataService, 
            IEnumerable<INotificationChannel> notificationChannels)
        {
            mDataService = dataService;
            mNotificationChannels = notificationChannels;
        }

        public async Task RunAsync(IEnumerable<Sport> sports)
        {
            await mDataService.InitialiseAsync();

            foreach (Sport sport in sports)
            {
                Dictionary<Event, IEnumerable<BookkeeperEvent>> events = await mDataService.ReconcileSportEventsAsync(sport);

                foreach ((Event @event, IEnumerable<BookkeeperEvent> bookkeeperEvents) in events)
                {
                    Dictionary<Market, IEnumerable<BookkeeperMarket>> markets = await mDataService.ReconcileEventMarketsAsync(@event, bookkeeperEvents);
                    @event.Markets = markets.Keys;

                    foreach ((Market market, IEnumerable<BookkeeperMarket> bookkeeperMarkets) in markets)
                    {
                        IEnumerable<Odds> odds = await mDataService.ReconcileMarketOddsAsync(@event, bookkeeperMarkets);
                        market.Odds = odds;

                        if (odds.Any())
                        {
                            (double arbitrage, IEnumerable<BookkeeperOdds> bookkeeperOdds) = Calculator.FindBestArbitrage(market);

                            if (arbitrage < 1)
                            {
                                foreach (INotificationChannel notificationChannel in mNotificationChannels)
                                {
                                    notificationChannel.Notify(sport, @event, market, bookkeeperMarkets, arbitrage, bookkeeperOdds);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
