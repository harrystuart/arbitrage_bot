using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class BookerkeeperEntityComparer
    {
        private BookerkeeperEntityComparerConfiguration mConfiguration;

        public BookerkeeperEntityComparer()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("configuration.json")
                .Build();

            mConfiguration = config.GetRequiredSection("bookerkeeperEntityComparer").Get<BookerkeeperEntityComparerConfiguration>();
        }

        public bool EventsEqual(BookkeeperEvent a, BookkeeperEvent b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (a.Competition != null && b.Competition != null)
            {
                if (StringDistance.NumberOfSubstringMovementsWithLengthPenalty(a.Competition, b.Competition) > 0.3)
                {
                    return false;
                }
            }

            if (StringDistance.NumberOfSubstringMovementsWithLengthPenalty(a.Name, b.Name) > 0.3)
            {
                return false;
            }

            return true;
        }

        public double CalculateMarketSimilarity(BookkeeperMarket a, BookkeeperMarket b, Event @event)
        {
            if (a == null || b == null)
            {
                return 0;
            }

            if (mConfiguration.KnownCorrespondingMarketNames.ContainsKey(a.Bookkeeper) &&
                mConfiguration.KnownCorrespondingMarketNames[a.Bookkeeper].ContainsKey(@event.Sport) &&
                mConfiguration.KnownCorrespondingMarketNames[a.Bookkeeper][@event.Sport].ContainsKey(a.Name))
            {
                if (mConfiguration.KnownCorrespondingMarketNames[a.Bookkeeper][@event.Sport][a.Name].Contains(b.Name))
                {
                    return 1;
                }

                return 1;
            }

            if (mConfiguration.KnownCorrespondingMarketNames.ContainsKey(b.Bookkeeper) &&
                mConfiguration.KnownCorrespondingMarketNames[b.Bookkeeper].ContainsKey(@event.Sport) &&
                mConfiguration.KnownCorrespondingMarketNames[b.Bookkeeper][@event.Sport].ContainsKey(b.Name))
            {
                if (mConfiguration.KnownCorrespondingMarketNames[b.Bookkeeper][@event.Sport][b.Name].Contains(a.Name))
                {
                    return 1;
                }

                return 1;
            }

            double score = StringDistance.NumberOfSubstringMovementsWithLengthPenalty(a.Name, b.Name);

            return 1 - score;
        }

        public bool MarketsEqual(BookkeeperMarket a, BookkeeperMarket b, Event @event)
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (mConfiguration.KnownCorrespondingMarketNames.ContainsKey(a.Bookkeeper) &&
                mConfiguration.KnownCorrespondingMarketNames[a.Bookkeeper].ContainsKey(@event.Sport) &&
                mConfiguration.KnownCorrespondingMarketNames[a.Bookkeeper][@event.Sport].ContainsKey(a.Name))
            {
                if (mConfiguration.KnownCorrespondingMarketNames[a.Bookkeeper][@event.Sport][a.Name].Contains(b.Name)) {
                    return true;
                }

                return false;
            }

            if (mConfiguration.KnownCorrespondingMarketNames.ContainsKey(b.Bookkeeper) &&
                mConfiguration.KnownCorrespondingMarketNames[b.Bookkeeper].ContainsKey(@event.Sport) &&
                mConfiguration.KnownCorrespondingMarketNames[b.Bookkeeper][@event.Sport].ContainsKey(b.Name))
            {
                if (mConfiguration.KnownCorrespondingMarketNames[b.Bookkeeper][@event.Sport][b.Name].Contains(a.Name))
                {
                    return true;
                }

                return false;
            }

            if (StringDistance.NumberOfSubstringMovementsWithLengthPenalty(a.Name, b.Name) > 0.3)
            {
                return false;
            }

            return true;
        }

        public bool OddsEqual(BookkeeperOdds a, BookkeeperOdds b, Event @event)
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (mConfiguration.KnownCorrespondingOddsNames.ContainsKey(a.Bookkeeper) &&
                mConfiguration.KnownCorrespondingOddsNames[a.Bookkeeper].ContainsKey(@event.Sport) &&
                mConfiguration.KnownCorrespondingOddsNames[a.Bookkeeper][@event.Sport].ContainsKey(a.Outcome))
            {
                if (mConfiguration.KnownCorrespondingOddsNames[a.Bookkeeper][@event.Sport][a.Outcome].Contains(b.Outcome))
                {
                    return true;
                }

                return false;
            }

            if (mConfiguration.KnownCorrespondingOddsNames.ContainsKey(b.Bookkeeper) &&
                mConfiguration.KnownCorrespondingOddsNames[b.Bookkeeper].ContainsKey(@event.Sport) &&
                mConfiguration.KnownCorrespondingOddsNames[b.Bookkeeper][@event.Sport].ContainsKey(b.Outcome))
            {
                if (mConfiguration.KnownCorrespondingOddsNames[b.Bookkeeper][@event.Sport][b.Outcome].Contains(a.Outcome))
                {
                    return true;
                }

                return false;
            }

            if (a.Description != null && b.Description != null)
            {
                if (StringDistance.NumberOfSubstringMovementsWithLengthPenalty(a.Description, b.Description) > 0.3)
                {
                    return false;
                }
            }

            if (StringDistance.NumberOfSubstringMovementsWithLengthPenalty(a.Outcome, b.Outcome) > 0.3)
            {
                return false;
            }

            return true;
        }
    }
}
