using System;

using Newtonsoft.Json;


namespace MarketDataProvider.VO
{
    public record Subscription(string ClientId, string Symbol, Mt5Bridge.DataParams DataParams);

    public record Candle(decimal Open, decimal High, decimal Low, decimal Close, decimal Voulme, DateTime Date);

    public record QuoteMsg(string Provider, string Instrument, [JsonProperty("tf")] string Timeframe, DateTime TickTime, Candle[] Data);

    public record HistoryData(Candle[] Data);

    public record IndCustom4Data(IndCustom4Data.Values[] Data)
    {
        public record Values
        (
            [JsonProperty("a")] decimal? VA,
            [JsonProperty("b")] decimal? GreenVA,
            [JsonProperty("c")] decimal? RedVA,
            [JsonProperty("e")] decimal? Median,
            [JsonProperty("Date")] DateTime Time
        );
    };
}
