using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Arbitrage
{
    public class LadbrokesService : BookkeeperService
    {
        private LadbrokesServiceConfiguration mConfiguration { get; set; }
        private dynamic? EventsListData { get; set; }
        private List<dynamic> EventsData { get; set; } = new List<dynamic>();

        public LadbrokesService()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("configuration.json")
                .Build();

            mConfiguration = config.GetRequiredSection("ladbrokesService").Get<LadbrokesServiceConfiguration>();
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
            if (!mConfiguration.Categories.Select(x => x.Sport).Contains(sport))
            {
                mLogger.LogWarning($"Ladbroke servive cannot get {sport}.");
                return new List<BookkeeperEvent>();
            }

            Dictionary<string, string> queryDictionary = new Dictionary<string, string>()
            {
                {"category_ids", $"[\"{mConfiguration.Categories.First(x => x.Sport == sport).Id}\"]"}
            };

            var queryString = new FormUrlEncodedContent(queryDictionary)
                    .ReadAsStringAsync().Result;

            UriBuilder uriBuilder = new UriBuilder(mConfiguration.GetEventsUrl);
            uriBuilder.Query = queryString;

            var httpResult = await mHttpClient.GetAsync(uriBuilder.ToString());

            if (!httpResult.IsSuccessStatusCode)
            {
                mLogger.LogWarning($"Could not get events from Ladbroke.");
                return new List<BookkeeperEvent>();
            }

            dynamic? data = JsonConvert.DeserializeObject<dynamic>(await httpResult.Content.ReadAsStringAsync());

            if (data == null)
            {
                mLogger.LogWarning("Could not deserialise Ladbrokes get events response content.");
                return new List<BookkeeperEvent>();
            }

            EventsListData = data;

            List<BookkeeperEvent> events = new List<BookkeeperEvent>();

            foreach (var x in EventsListData["events"])
            {
                var obj = ((IEnumerable<dynamic>)x).First();

                DateTimeOffset commencementTime = DateTimeOffset.Parse((string)obj["actual_start"].ToString()).ToUniversalTime();

                string eventId = obj["id"].ToString();

                string url = new Uri(new Uri(mConfiguration.BaseEventUrl), $"{sport}/nice/website/{eventId}").ToString(); //TODO: add proper URL

                events.Add(new BookkeeperEvent()
                {
                    Bookkeeper = Bookkeeper.Ladbrokes,
                    BookkeeperEventId = eventId,
                    Name = obj["name"].ToString(),
                    Competition = obj["competition"]?["name"]?.ToString(),
                    Commencement = commencementTime,
                    Url = url
                });
            }

            return events;
        }

        public override async Task<IEnumerable<BookkeeperMarket>> GetEventMarketsAsync(BookkeeperEvent bookkeeperEvent)
        {
            Dictionary<string, string> queryDictionary = new Dictionary<string, string>()
            {
                {"id", bookkeeperEvent.BookkeeperEventId}
            };

            var queryString = new FormUrlEncodedContent(queryDictionary)
                    .ReadAsStringAsync().Result;

            UriBuilder uriBuilder = new UriBuilder(mConfiguration.GetOddsUrl);
            uriBuilder.Query = queryString;

            var httpResult = await mHttpClient.GetAsync(uriBuilder.ToString());

            if (!httpResult.IsSuccessStatusCode)
            {
                mLogger.LogWarning($"Could not get event from Ladbrokes for event {bookkeeperEvent.BookkeeperEventId}.");
                return new List<BookkeeperMarket>();
            }

            dynamic? data = JsonConvert.DeserializeObject<dynamic>(await httpResult.Content.ReadAsStringAsync());

            if (data == null)
            {
                mLogger.LogWarning($"Could not deserialise Ladbrokes get event for {bookkeeperEvent.BookkeeperEventId}.");
                return new List<BookkeeperMarket>();
            }

            EventsData.Add(data);

            List<BookkeeperMarket> markets = new List<BookkeeperMarket>();

            foreach (var market in data["markets"])
            {
                markets.Add(new BookkeeperMarket()
                {
                    Bookkeeper = Bookkeeper.Ladbrokes,
                    BookkeeperMarketId = market.Value.id,
                    Name = market.Value.name,
                    Url = bookkeeperEvent.Url
                });
            }

            return markets;
        }

        public async override Task<IEnumerable<BookkeeperOdds>> GetMarketOddsAsync(Event @event, BookkeeperMarket bookkeeperMarket)
        {
            dynamic? eventDynamic = null;
            List<string> entrantIds = new List<string>();

            foreach (dynamic eventData in EventsData)
            {
                foreach (dynamic eventMarket in eventData["markets"])
                {
                    if (eventMarket.Value.id == bookkeeperMarket.BookkeeperMarketId)
                    {
                        eventDynamic = eventData;

                        foreach (string entrantId in eventMarket.Value["entrant_ids"])
                        {
                            entrantIds.Add(entrantId);
                        }
                    }
                }
            }

            if (eventDynamic == null)
            {
                throw new Exception($"EventsData does not contain data for market {bookkeeperMarket.BookkeeperMarketId}");
            }

            List<BookkeeperOdds> bookkeeperOdds = new List<BookkeeperOdds>();

            foreach (string entrantId in entrantIds)
            {
                string name = eventDynamic["entrants"][entrantId]["name"].ToString();
                dynamic price = ((IEnumerable<dynamic>)eventDynamic["prices"]).First(x => x.Name.Split(":")[0] == entrantId).Value["odds"];
                double value = double.Parse(price["numerator"].ToString()) / double.Parse(price["denominator"].ToString());

                bookkeeperOdds.Add(new BookkeeperOdds()
                {
                    Bookkeeper = Bookkeeper.Ladbrokes,
                    BookkeeperOddsId = entrantId,
                    Outcome = name,
                    Value = value + 1,
                    Url = bookkeeperMarket.Url
                });
            }

            return bookkeeperOdds;
        }
    }
}
