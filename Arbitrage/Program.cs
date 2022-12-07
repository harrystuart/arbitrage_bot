using Arbitrage;
using Newtonsoft.Json;
using System.Reflection;

BaseDataService dataService = new BaseDataService();
ConsoleNotificationChannel consoleNotificationChannel = new ConsoleNotificationChannel();
DiscordNotificationChannel discordNotificationChannel = new DiscordNotificationChannel();
await discordNotificationChannel.InitialiseAsync();

Engine engine = new Engine(dataService, new List<INotificationChannel>() { consoleNotificationChannel, discordNotificationChannel });

while (true)
{
    await engine.RunAsync(new List<Sport>() { Sport.AFL, Sport.Baseball, Sport.Soccer, Sport.Boxing, Sport.TableTennis, Sport.Cricket });
    Thread.Sleep(900000);
}