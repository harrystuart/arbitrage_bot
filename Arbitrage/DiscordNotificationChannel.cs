using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Arbitrage
{
    public class DiscordNotificationChannel : INotificationChannel
    {
        private readonly DiscordSocketClient mClient;
        private SocketTextChannel? mChannel;

        public DiscordNotificationChannel()
        {
            mClient = new DiscordSocketClient();
        }

        public async Task InitialiseAsync()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("configuration.json")
                .Build();

            string token = config.GetRequiredSection("discord").GetValue<string>("token");

            await mClient.LoginAsync(TokenType.Bot, token);
            await mClient.StartAsync();

            ulong serverId = config.GetRequiredSection("discord").GetValue<ulong>("serverId");
            ulong channelId = config.GetRequiredSection("discord").GetValue<ulong>("channelId");
            
            SocketGuild? socketGuild = null;

            int maxNumRetries = 100;
            int numRetries = 0;

            while (socketGuild == null)
            {
                if (numRetries >= maxNumRetries)
                {
                    throw new Exception("Could not connect Discord bot.");
                }

                socketGuild = mClient.GetGuild(serverId);
                await Task.Delay(50);

                numRetries++;
            }

            mChannel = socketGuild.GetTextChannel(channelId);
        }

        public bool Notify(Sport sport, Event @event, Market market, IEnumerable<BookkeeperMarket> bookkeeperMarkets, double arbitrage, IEnumerable<BookkeeperOdds> bestBookkeeperOdds)
        {
            Thread.Sleep(2000);

            if (mChannel == null)
            {
                throw new Exception("Discord channel not connected");
            }

            string longFormat = "0.00";

            string header = "```yaml\nNEW ARBITRAGE OPPORTUNITY FOUND\n```";
            string sportSection = $"**Sport** {Enum.GetName(typeof(Sport), sport)}";
            string eventSection = $"**Event** {@event.Name}";
            string startSection = $"**Start Time** {@event.Commencement.ToString()}";
            string marketSection = $"**Market** {market.Name}";
            string arbitrageSection = $"**Arbitrage** {arbitrage.ToString(longFormat)}";
            string oddsSection = "__**Optimal Odds**__";

            List<string> bookkeeperSections = new List<string>();

            foreach (BookkeeperOdds bookkeeperOdds in bestBookkeeperOdds)
            {
                BookkeeperMarket bookkeeperMarket = bookkeeperMarkets.First(x => x.Bookkeeper == bookkeeperOdds.Bookkeeper);
                string bookkeeperSection = $"**Bookie** {Enum.GetName(typeof(Bookkeeper), bookkeeperOdds.Bookkeeper)} **Market** {bookkeeperMarket.Name} **Outcome** {bookkeeperOdds.Outcome} **Odds** {bookkeeperOdds.Value.ToString(longFormat)} **Url** {bookkeeperOdds.Url}";
                bookkeeperSections.Add(bookkeeperSection);
            }

            string content = header + "\n" + sportSection + "\n" + eventSection + "\n" + startSection + "\n" + marketSection + "\n" + arbitrageSection + "\n\n" + oddsSection + "\n\n";

            foreach (string bookkeeperSection in bookkeeperSections)
            {
                content += bookkeeperSection + "\n";
            }

            mChannel.SendMessageAsync(content);

            return true;
        }
    }
}
