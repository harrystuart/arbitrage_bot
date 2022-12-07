using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class ConsoleNotificationChannel : INotificationChannel
    {
        public bool Notify(Sport sport, Event @event, Market market, IEnumerable<BookkeeperMarket> bookkeeperMarkets, double arbitrage, IEnumerable<BookkeeperOdds> bestBookkeeperOdds)
        {
            Console.WriteLine(Enum.GetName(typeof(Sport), sport));
            Console.WriteLine(@event.Name);
            Console.WriteLine(market.Name);
            Console.WriteLine(arbitrage);
            Console.WriteLine(@event.Commencement.ToString());
            
            foreach (BookkeeperMarket bookkeeperMarket in bookkeeperMarkets)
            {
                Console.WriteLine(bookkeeperMarket.Name);
            }

            foreach (BookkeeperOdds bookkeeperOdds in bestBookkeeperOdds)
            {
                Console.WriteLine(bookkeeperOdds.Url);
                Console.WriteLine($"{Enum.GetName(typeof(Bookkeeper), bookkeeperOdds.Bookkeeper)}\t{bookkeeperOdds.Outcome}\t{bookkeeperOdds.Value}");
            }

            Console.WriteLine();

            return true;
        }
    }
}
