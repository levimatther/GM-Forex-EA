using System;

using Newtonsoft.Json;


namespace TestDataGenerator.VO
{
    public enum Timeframe : uint { M1 = 1, M2 = 2, M5 = 5, M15 = 15, M30 = 30, H1 = 60 }

    public enum DirEnum : int { None = 0, Up = +1, Down = -1 };

    public record Candle(decimal Open, decimal High, decimal Low, decimal Close, decimal Voulme, DateTime Date);

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
