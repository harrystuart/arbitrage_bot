using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public class BaseDataService : IDataService
    {
        private LadbrokesService mLadBrokesService { get; set; }
        private UnibetService mUnibetService { get; set; }
        private BookerkeeperEntityComparer mBookerkeeperEntityComparer { get; set; }

        public BaseDataService()
        {
            mLadBrokesService = new LadbrokesService();
            mUnibetService = new UnibetService();
            mBookerkeeperEntityComparer = new BookerkeeperEntityComparer();
        }

        public async Task InitialiseAsync()
        {
            await mLadBrokesService.InitialiseAsync();
            await mUnibetService.InitialiseAsync();
        }

        public async Task<Dictionary<Event, IEnumerable<BookkeeperEvent>>> ReconcileSportEventsAsync(Sport sport)
        {
            Task<IEnumerable<BookkeeperEvent>> ladbrokesEventsTask = mLadBrokesService.GetSportEventsAsync(sport);
            Task<IEnumerable<BookkeeperEvent>> unibetEventsTask = mUnibetService.GetSportEventsAsync(sport);

            Task.WaitAll(ladbrokesEventsTask, unibetEventsTask);

            List<BookkeeperEvent> ladbrokesEvents = ladbrokesEventsTask.Result.ToList();
            List<BookkeeperEvent> unibetEvents = unibetEventsTask.Result.ToList();

            List<List<BookkeeperEvent>> allBookkeeperEvents = new List<List<BookkeeperEvent>>();
            allBookkeeperEvents.Add(ladbrokesEvents);
            allBookkeeperEvents.Add(unibetEvents);

            IEnumerable<IEnumerable<BookkeeperEvent>> bookkeeperEventGroups = FindEquiavelentBookkeeperEventsInLists(allBookkeeperEvents);

            Dictionary<Event, IEnumerable<BookkeeperEvent>> events = new Dictionary<Event, IEnumerable<BookkeeperEvent>>();

            foreach (List<BookkeeperEvent> group in bookkeeperEventGroups)
            {
                events.Add(new Event()
                {
                    Competition = group[0].Competition,
                    Name = group[0].Name,
                    Sport = sport,
                    Commencement = group[0].Commencement,
                    TMPUnibetEventId = group[1].BookkeeperEventId
                }, group);
            }

            return events;
        }

        public async Task<Dictionary<Market, IEnumerable<BookkeeperMarket>>> ReconcileEventMarketsAsync(Event @event, IEnumerable<BookkeeperEvent> bookkeeperEvents)
        {
            if (@event.Name == "Tepatitlan FC vs CD Tapatio")
            {
                var v = "";
            }

            Task<IEnumerable<BookkeeperMarket>> ladbrokesMarketsTask = mLadBrokesService
                .GetEventMarketsAsync(bookkeeperEvents.First(x => x.Bookkeeper == Bookkeeper.Ladbrokes));
            Task<IEnumerable<BookkeeperMarket>> unibetMarketsTask = mUnibetService
                .GetEventMarketsAsync(bookkeeperEvents.First(x => x.Bookkeeper == Bookkeeper.Unibet));

            Task.WaitAll(ladbrokesMarketsTask, unibetMarketsTask);

            List<BookkeeperMarket> ladbrokesMarkets = ladbrokesMarketsTask.Result.ToList();
            List<BookkeeperMarket> unibetMarkets = unibetMarketsTask.Result.ToList();

            List<List<BookkeeperMarket>> allBookkeeperMarkets = new List<List<BookkeeperMarket>>();
            allBookkeeperMarkets.Add(ladbrokesMarkets);
            allBookkeeperMarkets.Add(unibetMarkets);

            IEnumerable<IEnumerable<BookkeeperMarket>> bookkeeperMarketGroups = FindEquiavelentBookkeeperMarketsInLists(allBookkeeperMarkets, @event);

            Dictionary<Market, IEnumerable<BookkeeperMarket>> markets = new Dictionary<Market, IEnumerable<BookkeeperMarket>>();

            foreach (List<BookkeeperMarket> group in bookkeeperMarketGroups)
            {
                markets.Add(new Market() { Name = group[0].Name }, group);
            }

            return markets;
        }

        // A market is only valid if all odds for all bookkeepers operating in that market can be reconciled
        public async Task<IEnumerable<Odds>> ReconcileMarketOddsAsync(Event @event, IEnumerable<BookkeeperMarket> bookkeeperMarkets)
        {
            Task<IEnumerable<BookkeeperOdds>> ladbrokesOddsTask = mLadBrokesService
                .GetMarketOddsAsync(@event, bookkeeperMarkets.First(x => x.Bookkeeper == Bookkeeper.Ladbrokes));
            Task<IEnumerable<BookkeeperOdds>> unibetOddsTask = mUnibetService
                .GetMarketOddsAsync(@event, bookkeeperMarkets.First(x => x.Bookkeeper == Bookkeeper.Unibet));

            Task.WaitAll(ladbrokesOddsTask, unibetOddsTask);

            List<BookkeeperOdds> ladbrokesOdds = ladbrokesOddsTask.Result.ToList();
            List<BookkeeperOdds> unibetOdds = unibetOddsTask.Result.ToList();

            List<KeyValuePair<Bookkeeper, List<BookkeeperOdds>>> bookkeeperOdds = new List<KeyValuePair<Bookkeeper, List<BookkeeperOdds>>>();
            bookkeeperOdds.Add(new KeyValuePair<Bookkeeper, List<BookkeeperOdds>>(Bookkeeper.Ladbrokes, ladbrokesOdds));
            bookkeeperOdds.Add(new KeyValuePair<Bookkeeper, List<BookkeeperOdds>>(Bookkeeper.Unibet, unibetOdds));

            int numOddsPerBookkeeper = bookkeeperOdds.First().Value.Count;

            foreach (List<BookkeeperOdds> odds in bookkeeperOdds.Select(x => x.Value))
            {
                if (odds.Count != numOddsPerBookkeeper)
                {
                    return new List<Odds>();
                }
            }

            List<List<BookkeeperOdds>> bookkeeperOddsGroups = new List<List<BookkeeperOdds>>();

            for (int i = 0; i < bookkeeperOdds.Count - 1; i++)
            {
                List<BookkeeperOdds> bookkeeperOdds1 = bookkeeperOdds[i].Value;
                List<BookkeeperOdds> bookkeeperOdds2 = bookkeeperOdds[i + 1].Value;

                for (int m = 0; m < numOddsPerBookkeeper; m++)
                {
                    bookkeeperOddsGroups.Add(new List<BookkeeperOdds>());

                    bool foundMatch = false;

                    for (int n = 0; n < numOddsPerBookkeeper; n++)
                    {
                        if (mBookerkeeperEntityComparer.OddsEqual(bookkeeperOdds1[m], bookkeeperOdds2[n], @event))
                        {
                            foundMatch = true;

                            bookkeeperOddsGroups[m].Add(bookkeeperOdds1[m]);

                            if (i == bookkeeperOdds.Count - 2)
                            {
                                bookkeeperOddsGroups[m].Add(bookkeeperOdds2[n]);
                            }

                            break;
                        }
                    }

                    if (!foundMatch)
                    {
                        return new List<Odds>();
                    }
                }
            }

            List<Odds> standardisedOdds = new List<Odds>();

            foreach (List<BookkeeperOdds> bookkeeperOddsGroup in bookkeeperOddsGroups)
            {
                standardisedOdds.Add(new Odds()
                {
                    Outcome = bookkeeperOddsGroup[0].Outcome,
                    BookkeeperOdds = bookkeeperOddsGroup
                });
            }

            return standardisedOdds;
        }

        private IEnumerable<IEnumerable<BookkeeperEvent>> FindEquiavelentBookkeeperEventsInLists(List<List<BookkeeperEvent>> bookkeeperEvents)
        {
            List<List<BookkeeperEvent>> groups = new List<List<BookkeeperEvent>>();

            for (int i = 0; i < bookkeeperEvents.Count; i++)
            {
                for (int j = i + 1; j < bookkeeperEvents.Count; j++)
                {
                    for (int m = 0; m < bookkeeperEvents[i].Count; m++)
                    {
                        for (int n = 0; n < bookkeeperEvents[j].Count; n++)
                        {
                            if (mBookerkeeperEntityComparer.EventsEqual(bookkeeperEvents[i][m], bookkeeperEvents[j][n]))
                            {
                                bool foundMatchToExistingGroup = false;

                                for (int k = 0; k < groups.Count; k++)
                                {
                                    if (groups[k][0] == bookkeeperEvents[i][m]) // Works if this is reference based
                                    {
                                        foundMatchToExistingGroup = true;
                                        groups[k].Add(bookkeeperEvents[j][n]);
                                        break;
                                    }
                                }

                                if (!foundMatchToExistingGroup)
                                {
                                    groups.Add(new List<BookkeeperEvent>() { bookkeeperEvents[i][m], bookkeeperEvents[j][n] });
                                }

                                bookkeeperEvents[j].RemoveAt(n);
                                break;
                            }
                        }
                    }
                }
            }

            return groups;
        }

        private IEnumerable<IEnumerable<BookkeeperMarket>> FindEquiavelentBookkeeperMarketsInLists(List<List<BookkeeperMarket>> bookkeeperMarkets, Event @event)
        {
            List<List<BookkeeperMarket>> groups = new List<List<BookkeeperMarket>>();

            for (int i = 0; i < bookkeeperMarkets.Count; i++)
            {
                for (int j = i + 1; j < bookkeeperMarkets.Count; j++)
                {
                    // Get all pairings with scores
                    List<KeyValuePair<(BookkeeperMarket, BookkeeperMarket), double>> bookkeeperMarketPairings = new List<KeyValuePair<(BookkeeperMarket, BookkeeperMarket), double>>();

                    for (int m = 0; m < bookkeeperMarkets[i].Count; m++)
                    {
                        for (int n = 0; n < bookkeeperMarkets[j].Count; n++)
                        {
                            double similarity = mBookerkeeperEntityComparer.CalculateMarketSimilarity(
                                        bookkeeperMarkets[i][m], bookkeeperMarkets[j][n], @event);

                            if (similarity > 0.8)
                            {
                                bookkeeperMarketPairings.Add(new KeyValuePair<(BookkeeperMarket, BookkeeperMarket), double>(
                                    (bookkeeperMarkets[i][m], bookkeeperMarkets[j][n]), similarity));
                            }
                        }
                    }

                    // Order by score and then by string length similarity
                    bookkeeperMarketPairings = bookkeeperMarketPairings.OrderByDescending(x => x.Value)
                        .ThenBy(x => Math.Abs(x.Key.Item1.Name.Length - x.Key.Item2.Name.Length))
                        .ToList();

                    foreach (var pairing in bookkeeperMarketPairings)
                    {
                        Console.WriteLine($"{pairing.Value}:{pairing.Key.Item1.Name}:{pairing.Key.Item2.Name}");
                    }
                    Console.WriteLine();

                    while (bookkeeperMarketPairings.Any())
                    {
                        (BookkeeperMarket bookkeeperMarket1, BookkeeperMarket bookkeeperMarket2) = bookkeeperMarketPairings.First().Key;

                        bool foundMatchToExistingGroup = false;

                        for (int k = 0; k < groups.Count; k++)
                        {
                            if (groups[k].Contains(bookkeeperMarket1)) // Works if this is reference based
                            {
                                foundMatchToExistingGroup = true;
                                groups[k].Add(bookkeeperMarket2);
                                break;
                            }
                        }

                        if (!foundMatchToExistingGroup)
                        {
                            groups.Add(new List<BookkeeperMarket>() { bookkeeperMarket1, bookkeeperMarket2 });
                        }

                        bookkeeperMarketPairings.RemoveAll(x => { 
                            if (x.Key.Item1 == bookkeeperMarket1 ||
                                x.Key.Item1 == bookkeeperMarket2 ||
                                x.Key.Item2 == bookkeeperMarket1 ||
                                x.Key.Item2 == bookkeeperMarket2)
                            {
                                return true;
                            }

                            return false;
                        });
                    }
                }
            }

            return groups;
        }
    }
}
