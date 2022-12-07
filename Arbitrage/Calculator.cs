using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public static class Calculator
    {
        public static (double, IEnumerable<BookkeeperOdds>) FindBestArbitrage(Market market)
        {
            List<BookkeeperOdds> optimalBookkeeperOdds = FindBestOdds(market);

            double arbitrage = optimalBookkeeperOdds.Select(x => x.Value).Sum(x => 1.0 / x);

            return (arbitrage, optimalBookkeeperOdds);
        }

        private static List<BookkeeperOdds> FindBestOdds(Market market)
        {
            if (market.Odds == null)
            {
                throw new Exception($"Odds are null for market {market}.");
            }

            List<BookkeeperOdds> optimalBookkeeperOdds = new List<BookkeeperOdds>();

            foreach (Odds odds in market.Odds)
            {
                BookkeeperOdds maxBookkeeperOdds = odds.BookkeeperOdds.MaxBy(x => x.Value);
                optimalBookkeeperOdds.Add(maxBookkeeperOdds);
            }

            return optimalBookkeeperOdds;
        }
    }
}
