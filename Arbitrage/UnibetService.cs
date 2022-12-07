using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Arbitrage
{
    public class UnibetService : BookkeeperService
    {
        private UnibetServiceConfiguration mConfiguration { get; set; }
        private dynamic? OddsData { get; set; } 
        private Dictionary<string, List<string>> mEventParticipants { get; set; }

        public UnibetService()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("configuration.json")
                .Build();

            mConfiguration = config.GetRequiredSection("unibetService").Get<UnibetServiceConfiguration>();

            mEventParticipants = new Dictionary<string, List<string>>();
        }

        public async Task InitialiseAsync()
        {
            UriBuilder uriBuilder = new UriBuilder(mConfiguration.BaseSiteUrl);

            var httpResult = await mHttpClient.GetAsync(uriBuilder.ToString());

            if (!httpResult.IsSuccessStatusCode)
            {
                mLogger.LogWarning($"Could not get visit Ladbrokes");
            }
        }

        public override async Task<IEnumerable<BookkeeperEvent>> GetSportEventsAsync(Sport sport)
        {
            if (!mConfiguration.GetSportEventsUrls.Select(x => x.Key).Contains(sport))
            {
                mLogger.LogWarning($"Unibet not configured to retrieve {sport} events.");
                return new List<BookkeeperEvent>();
            }

            UriBuilder uriBuilder = new UriBuilder(mConfiguration.GetSportEventsUrls[sport]);

            var httpResult = await mHttpClient.GetAsync(uriBuilder.ToString());

            if (!httpResult.IsSuccessStatusCode)
            {
                mLogger.LogWarning($"Could not get {sport} events from Unibet");
                return new List<BookkeeperEvent>();
            }

            dynamic? data = JsonConvert.DeserializeObject<dynamic>(await httpResult.Content.ReadAsStringAsync());

            if (data == null)
            {
                mLogger.LogWarning($"Could not deserialise Unibet events for {sport}");
                return new List<BookkeeperEvent>();
            }

            List<BookkeeperEvent> events = new List<BookkeeperEvent>();

            foreach (dynamic section in data["layout"]["sections"])
            {
                foreach (dynamic widget in section["widgets"])
                {
                    foreach (dynamic group in widget["matches"]["groups"])
                    {
                        List<dynamic> eventDatas = GetEventsFromGroup(group);

                        foreach (dynamic eventData in eventDatas)
                        {
                            string eventId = eventData.id.ToString();

                            List<string> participantIds = new List<string>();

                            if (eventData.ContainsKey("participants"))
                            {
                                foreach (dynamic participant in eventData["participants"])
                                {
                                    participantIds.Add(participant["name"].ToString());
                                }
                            }

                            DateTimeOffset commencementTime = DateTimeOffset.Parse((string)eventData["start"].ToString()).ToUniversalTime();

                            string url = new Uri(new Uri(mConfiguration.BaseEventUrl), eventId).ToString();

                            events.Add(new BookkeeperEvent()
                            {
                                Bookkeeper = Bookkeeper.Unibet,
                                BookkeeperEventId = eventId,
                                Name = eventData.name.ToString(),
                                Competition = string.Join(" ", ((IEnumerable<dynamic>)eventData.path).Select(x => x.name.ToString())),
                                Commencement = commencementTime,
                                Url = url
                            });

                            mEventParticipants.Add(eventId, participantIds);
                        }
                    }
                }
            }

            return events;
        }

        private static List<dynamic> GetEventsFromGroup(dynamic group)
        {
            List<dynamic> events = new List<dynamic>();

            dynamic subGroups = group["subGroups"];

            if (subGroups == null)
            {
                foreach (dynamic @event in group["events"])
                {
                    events.Add(@event["event"]);
                }

                return events;
            }

            foreach (dynamic subgroup in subGroups)
            {
                events.AddRange(GetEventsFromGroup(subgroup));
            }

            return events;
        }

        public override async Task<IEnumerable<BookkeeperMarket>> GetEventMarketsAsync(BookkeeperEvent bookkeeperEvent)
        {
            UriBuilder uriBuilder = new UriBuilder(new Uri(new Uri(mConfiguration.GetOddsUrl), bookkeeperEvent.BookkeeperEventId));

            var httpResult = await mHttpClient.GetAsync(uriBuilder.ToString());

            if (!httpResult.IsSuccessStatusCode)
            {
                mLogger.LogWarning($"Could not get Unibet markets for event {bookkeeperEvent.BookkeeperEventId}");
                return new List<BookkeeperMarket>();
            }

            dynamic? data = JsonConvert.DeserializeObject<dynamic>(await httpResult.Content.ReadAsStringAsync());

            if (data == null)
            {
                mLogger.LogWarning($"Could not deserialise Unibet markets for event {bookkeeperEvent.BookkeeperEventId}");
                return new List<BookkeeperMarket>();
            }

            OddsData = data;

            List<BookkeeperMarket> markets = new List<BookkeeperMarket>();

            foreach (dynamic market in OddsData["betOffers"])
            {
                string marketName = market["betOfferType"]["name"];

                if (market["criterion"] != null)
                {
                    marketName += " " + market["criterion"]["label"];
                }

                dynamic firstOutcome = market["outcomes"][0];

                if (firstOutcome.ContainsKey("line"))
                {
                    float line;
                    bool didParse = float.TryParse(firstOutcome["line"].ToString(), out line);

                    if (didParse)
                    {
                        marketName += " " + (line / 1000.0).ToString("0.0");
                    }
                }

                markets.Add(new BookkeeperMarket()
                {
                    Bookkeeper = Bookkeeper.Unibet,
                    BookkeeperMarketId = market.id,
                    Name = marketName,
                    Url = bookkeeperEvent.Url
                });
            }

            return markets;
        }

        public async override Task<IEnumerable<BookkeeperOdds>> GetMarketOddsAsync(Event @event, BookkeeperMarket bookkeeperMarket)
        {
            if (OddsData == null)
            {
                throw new Exception("OddsData property is not populated.");
            }

            dynamic betOffer = ((IEnumerable<dynamic>)OddsData["betOffers"]).First(x => x.id == bookkeeperMarket.BookkeeperMarketId);

            List<BookkeeperOdds> bookkeeperOdds = new List<BookkeeperOdds>();

            foreach (dynamic outcome in betOffer["outcomes"])
            {

                if (!outcome.ContainsKey("odds"))
                {
                    continue;
                }

                double value = double.Parse(outcome["odds"].ToString()) / 1000;

                string outcomeString = outcome["label"].ToString();

                List<int> validParticipantIndices = new List<int>();

                if (mEventParticipants.Keys.Contains(@event.TMPUnibetEventId))
                {
                    for (int i = 0; i < mEventParticipants[@event.TMPUnibetEventId].Count; i++)
                    {
                        validParticipantIndices.Add(i);
                    }
                }

                string newOutcomeString = outcomeString;

                foreach (string c in outcomeString.ToList().Select(x => x.ToString()))
                {
                    int representation = -1;

                    if (int.TryParse(c, out representation))
                    {
                        if (validParticipantIndices.Contains(representation - 1))
                        {
                            newOutcomeString = newOutcomeString.Replace(c, mEventParticipants[@event.TMPUnibetEventId][representation - 1]);
                        }
                    }
                    else if (c == "X")
                    {
                        newOutcomeString = newOutcomeString.Replace(c, "Draw");
                    }
                }

                bookkeeperOdds.Add(new BookkeeperOdds()
                {
                    Bookkeeper = Bookkeeper.Unibet,
                    BookkeeperOddsId = outcome["id"],
                    Outcome = newOutcomeString,
                    Value = value,
                    Url = bookkeeperMarket.Url
                });
            }

            return bookkeeperOdds;
        }
    }
}
