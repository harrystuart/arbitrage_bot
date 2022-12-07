using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public abstract class BookkeeperService
    {
        protected HttpClient mHttpClient;
        protected readonly ILogger<BookkeeperService> mLogger;

        public BookkeeperService()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole()
                    .AddEventLog();
            });

            mLogger = loggerFactory.CreateLogger<BookkeeperService>();
            mHttpClient = new HttpClient();
        }

        public abstract Task<IEnumerable<BookkeeperEvent>> GetSportEventsAsync(Sport sport);
        public abstract Task<IEnumerable<BookkeeperMarket>> GetEventMarketsAsync(BookkeeperEvent bookkeeperEvent);
        public abstract Task<IEnumerable<BookkeeperOdds>> GetMarketOddsAsync(Event @event, BookkeeperMarket market);
    }
}
