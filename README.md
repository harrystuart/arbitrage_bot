# arbitrage_bot
Project that automatically finds arbitrage opportunities between bookmakers across various sports and posts results into a custom Discord server. Backend developed using ASP.NET Core C#.

## Process
1. Web-scrape the online sites of multiple bookmakers by reverse engineering their internal API calls.
2. Correlate markets and events between different bookmakers by using Natural Language Processing. This is required because there is no universal identifier for any sporting event and thus, string matching must be performed on the entities returned by different bookkmakers.
3. Calculate arbitrage opportunities. Refer to [this](https://en.wikipedia.org/wiki/Arbitrage_betting) for an understanding as to how these calculations are performed.
4. Post new opportunities to a custom Discord server as a mechanism by which users can be notified

_This application was discontinued in December 2022 due to a change in priorities and is now open-source._
