namespace MarketDataProvider.VO
{
    public record Subscription(string Provider, string Symbol, string Timeframe);
    
    public record DataDumperParams(Subscription[] Subscriptions);
}
