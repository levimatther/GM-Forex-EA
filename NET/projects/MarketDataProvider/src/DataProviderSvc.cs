using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timers = System.Timers;
using HttpUtility = System.Web.HttpUtility;

using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Commons;
using static Commons.Extensions.TaskExtensions;


namespace MarketDataProvider
{
    using DirEnum = Mt5Bridge.MarketData.DirEnum;

    class DataProviderSvc
    {
        private const string ServiceURI = @"tcp://127.12.12.1:5555";
        private const string UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.141 Safari/537.36";
        private const string DomainName = "platform.chartingcenter.com";
        private static readonly Uri BaseURI = new($"https://{DomainName}");
        private static readonly Uri BaseWsURI = new($"wss://{DomainName}");
        private static readonly Uri RootPageURI = new(BaseURI, "/");
        private static readonly Uri LoginPageURI = new(BaseURI, "/login");
        private static readonly Uri ApiAuthURI = new(BaseURI, "/api/authtoken");
        private static readonly Func<string, Uri> WsUriFactory = (token) => new(BaseWsURI, $"/socket.io/?token={token}&EIO=3&transport=websocket");
        private static readonly Func<string, Mt5Bridge.Timeframe, string, DateTime, DateTime, Uri> HistoryXhrUriFactory =
            (instrument, timeframe, provider, dateFrom, dateTo) =>
                new
                (
                    BaseURI,
                    $@"/api/history?" +
                    $@"provider={HttpUtility.UrlEncode(provider).ToUpper()}" +
                    $@"&instrument={HttpUtility.UrlEncode(instrument).ToUpper()}" +
                    $@"&tf={HttpUtility.UrlEncode(timeframe.ToString()).ToUpper()}" +
                    $@"&dateFrom={HttpUtility.UrlEncode($"{dateFrom:dd.MM.yyyy HH:mm}:00").ToUpper()}" +
                    $@"&dateTo={HttpUtility.UrlEncode($"{dateTo:dd.MM.yyyy HH:mm}:00").ToUpper()}"
                );
        private static readonly Func<string, string, string, Mt5Bridge.Timeframe, DateTime, DateTime, Uri> IndicatorXhrUriFactory =
            (indicator, provider, instrument, timeframe, dateFrom, dateTo) =>
                new
                (
                    BaseURI,
                    $@"/api/indicator?" +
                    $@"indicator={indicator}" +
                    $@"&provider={HttpUtility.UrlEncode(provider).ToUpper()}" +
                    $@"&instrument={HttpUtility.UrlEncode(instrument).ToUpper()}" +
                    $@"&tf={HttpUtility.UrlEncode(timeframe.ToString()).ToUpper()}" + 
                    $@"&dateFrom={HttpUtility.UrlEncode($"{dateFrom:dd.MM.yyyy HH:mm}:00").ToUpper()}" +
                    $@"&dateTo={HttpUtility.UrlEncode($"{dateTo:dd.MM.yyyy HH:mm}:00").ToUpper()}"
                );
        private static readonly JsonSerializer JsonSerializer = new ()
        { 
            DateFormatString = "dd.MM.yyyy HH:mm:ss",
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Culture = CultureInfo.InvariantCulture
        };

        private static readonly uint Provider = 2;
        private static readonly Mt5Bridge.Timeframe QuotesTimeframe = Mt5Bridge.Timeframe.M1;
        private static readonly string[] IndicatorNames = { "custom_4" };

        private static readonly JToken loginJSON = new JObject { ["email"] = "geoffrey@geoff7.plus.com", ["password"] = "mcgowan" };

        private const string SvcThreadName = nameof(DataProviderSvc);
        private const int SvcQueueLimit = 100;
        private static readonly (int Recv, int Send) DataMemorySize = (0x40000, 0x100);
        private static readonly uint InteropBufferSize = 0x1000;
        private static readonly uint InteropBufferChunk = 0x400;
        private static readonly TimeSpan DataWsCheckInterval = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan DataWsInitTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan DataWsSendTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan IndicatorXhrRequestInterval = TimeSpan.FromSeconds(5);

        private readonly CookieContainer cookieContainer = new();
        private readonly HttpClientHandler httpHandler;
        private readonly HttpClient httpClient;
        private ClientWebSocket quotesWS = new();

        private record DataBuffer
        {
            private int next;

            public int Size;
            public byte[] Array;

            public int Next => next;

            public DataBuffer(int size) { Size = size; Array = new byte[size]; next = 0; }

            public ArraySegment<byte> Data => new(Array, 0, Next);

            public ArraySegment<byte> Memory => new(Array, Next, Size - Next);

            public void Reset() { next = 0; }

            public void Set(int value) => next = Math.Min(value, Size);

            public void Extend(int value) => next = Math.Min(next + value, Size);
        }

        private readonly (DataBuffer Recv, DataBuffer Send) dataBuffers = (new(DataMemorySize.Recv), new(DataMemorySize.Send));
        private readonly IntPtr InteropBuffer = Marshal.AllocHGlobal((int) InteropBufferSize);

        private AsyncService svc = new (SvcQueueLimit);
        private Timers.Timer wsPingTimer = new() { AutoReset = true };
        private Timers.Timer wsCheckTimer = new() { AutoReset = true };
        private Timers.Timer xhrRequestTimer = new() { AutoReset = true };

        private CancellationTokenSource svcCancelSource = new();

        private record AuthInfo(string Id, string AccessToken);

        private class DataWsSession
        {
            public readonly string SID;
            public readonly TimeSpan PingInterval;
            public DateTime LastPing;

            public DataWsSession(string sid, TimeSpan pingInterval)
            { 
                SID = sid;
                PingInterval = pingInterval;
                LastPing = DateTime.MinValue;
            }
        }

        private AuthInfo? authInfo = null;
        private DataWsSession? dataWsSession = null;
        private bool isResetting = false;

        private class SymbolDataDescriptor
        {
            public abstract record TfDataDescriptor
            {
                public uint RefCount = 0;
                public ulong BatchSerial = 0;
            }

            public record MainTfDataDescriptor : TfDataDescriptor
            {
                public DateTime Time = DateTime.MinValue;
                public (DateTime Time, decimal Price, bool? Colour)[] VA = Array.Empty<(DateTime Time, decimal Price, bool? Colour)>();
            }

            public record TrendTfDataDescriptor : TfDataDescriptor
            {
                public uint MaxPeriod = 0;
                public (DateTime Time, decimal Price)[] Median = Array.Empty<(DateTime Time, decimal Price)>();
            }

            public record TfDataDescriptorPair
            { 
                public MainTfDataDescriptor? Main;
                public TrendTfDataDescriptor? Trend;
            }

            public uint RefCount = 0;
            public DateTime LastTickTime = DateTime.MinValue;
            public Dictionary<Mt5Bridge.Timeframe, TfDataDescriptorPair> TimeframesMap = new();
        }

        private Dictionary<string, SymbolDataDescriptor> symbolsDataMap = new();
        private Dictionary<string, VO.Subscription> subscriptionsMap = new();
        private ulong _batchSerial = 0;

        private record DataWsMessage(uint TypeCode, JToken? ContentJSON = null)
        {
            public static DataWsMessage? Parse(string msgText)
            {
                var typeCodeTxt = string.Concat(msgText.TakeWhile((c) => '0' <= c && c <= '9'));
                if (typeCodeTxt != string.Empty)
                {
                    var typeCode = uint.Parse(typeCodeTxt);
                    var contentTxt = msgText.Substring(typeCodeTxt.Length);
                    try { JToken? contentJSON = typeCode switch { 0 or 42 => JToken.Parse(contentTxt),  _ => null }; return new(typeCode, contentJSON); }
                    catch (JsonReaderException) { return null; }
                }
                return null;
            }

            public static string Format(DataWsMessage msg) => $"{msg.TypeCode}{msg.ContentJSON?.ToString(Formatting.None) ?? ""}";
        }

        private record HistoryDataCtx(string Symbol, Mt5Bridge.Timeframe Timeframe);
        private record IndicatorDataCtx(ulong BatchSerial, string Indicator, string Symbol, Mt5Bridge.Timeframe Timeframe, DateTime Time);

        delegate void HistoryDataDispatcher(HistoryDataCtx ctx, JObject dataJSON);
        delegate void IndicatorDataDispatcher(IndicatorDataCtx ctx, JObject dataJSON);

        private Logger logger = LogManager.GetLogger(nameof(DataProviderSvc));

        public DataProviderSvc()
        {
            httpHandler = new() { CookieContainer = cookieContainer };
            httpClient = new(httpHandler);
            var defaultHeaders = httpClient.DefaultRequestHeaders;
            defaultHeaders.UserAgent.Clear(); defaultHeaders.UserAgent.ParseAdd(UserAgent);
            defaultHeaders.Referrer = BaseURI;
            quotesWS.Options.Cookies = cookieContainer;
            wsPingTimer.Elapsed += (_, _) => svc.Post(SendPing);
            wsCheckTimer.Elapsed += (_, _) => svc.Post(DoCheck);
            xhrRequestTimer.Elapsed += (_, _) => svc.Post(() => RequestIndicatorsData(DispatchIndicatorData));
        }

        public void Run()
        {
            logger.Info("Market data provider started.");
            svc.Run(SvcThreadName);
            svc.Post(Start);
        }

        public void Shutdown()
        {
            logger.Info("Shutting down. Please wait...");
            svc.Stop();
            Mt5Bridge.MT5S_Shutdown();
            svcCancelSource.Cancel();
        }

        private void Start()
        {
            void onLogin(Task<bool> task)
            {
                if (task.IsCompletedSuccessfully && task.Result)
                    svc.Post(ConnectToDataWS);
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            logger.Error($"Exception @01 <{innerEx.GetType().FullName}> {innerEx.Message}");
                }
            }
            if (isResetting || Mt5Bridge.MT5S_Init(ServiceURI))
                ProcLogin().ContinueWithPost(svc, onLogin);
        }

        private async Task<bool> ProcLogin()
        {
            var rspLoginPage = await httpClient.GetAsync(LoginPageURI);
            if (rspLoginPage.IsSuccessStatusCode)
            {
                var authContent = new StringContent(loginJSON.ToString(Formatting.None), Encoding.UTF8, "application/json");
                var rspAuth = await httpClient.PostAsync(ApiAuthURI, authContent);
                if (rspAuth.IsSuccessStatusCode)
                {
                    var content = await rspAuth.Content.ReadAsStringAsync();
                    var authInfoJSON = JObject.Parse(content);
                    authInfo = new(Id: authInfoJSON.Value<string>("id"), AccessToken: authInfoJSON.Value<string>("accessToken"));
                    var rspRootPage = await httpClient.GetAsync(RootPageURI);
                    if (rspRootPage.IsSuccessStatusCode)
                        return true;
                    else
                    {
                        logger.Error($"Server responce: {(int) rspRootPage.StatusCode} {rspRootPage.ReasonPhrase}");
                        return false;
                    }
                }
                else
                {
                    logger.Error($"Server responce: {(int) rspAuth.StatusCode} {rspAuth.ReasonPhrase}");
                    return false;
                }
            }
            else
            {
                logger.Error($"Server responce: {(int) rspLoginPage.StatusCode} {rspLoginPage.ReasonPhrase}");
                return false;
            }
        }

        private void ConnectToDataWS()
        {
            void onConnectAsync(Task task)
            {
                if (task.IsCompletedSuccessfully)
                    svc.Post(RecvDataWsInitMessage);
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            logger.Error($"Exception @02 <{innerEx.GetType().FullName}> {innerEx.Message}");
                    ResetWsSession();
                }
            }
            if (authInfo is AuthInfo ai)
                quotesWS.ConnectAsync(WsUriFactory(ai.AccessToken), CancellationToken.None).ContinueWithPost(svc, onConnectAsync);
        }

        private void RecvDataWsInitMessage()
        {
            if (authInfo is null) return;
            CancellationTokenSource cancelSource = new(DataWsInitTimeout);
            void onReceiveAsync(Task<WebSocketReceiveResult> task)
            {
                if (task.IsCompletedSuccessfully)
                {
                    var wsResult = task.Result;
                    if (wsResult.MessageType == WebSocketMessageType.Text)
                    {
                        if (!wsResult.EndOfMessage)
                        {
                            dataBuffers.Recv.Extend(wsResult.Count);
                            quotesWS.ReceiveAsync(dataBuffers.Recv.Memory, cancelSource.Token).ContinueWithPost(svc, onReceiveAsync);
                        }
                        var msgText = Encoding.UTF8.GetString(dataBuffers.Recv.Array, 0, wsResult.Count);
                        var msg = DataWsMessage.Parse(msgText);
                        if (msg != null && msg.TypeCode == 0 && msg.ContentJSON != null && msg.ContentJSON is JObject jsonObj)
                        {
                            var sid = jsonObj.Value<string>("sid");
                            var pingInterval = jsonObj.Value<double>("pingInterval");
                            dataWsSession = new(sid, TimeSpan.FromMilliseconds(pingInterval));
                            if (isResetting)
                            { 
                                RestoreSubscriptions();
                                isResetting = false;
                            }
                            startTimers();
                            svc.Post(RecvData);
                        }
                    }
                    else
                        logger.Error($"Received unexpected message type.");
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            logger.Error($"Exception @03 <{innerEx.GetType().FullName}> {innerEx.Message}");
                    ResetWsSession();
                }
            }
            void startTimers()
            {
                wsPingTimer.Interval = dataWsSession.PingInterval.TotalMilliseconds;
                wsCheckTimer.Interval = DataWsCheckInterval.TotalMilliseconds;
                xhrRequestTimer.Interval = IndicatorXhrRequestInterval.TotalMilliseconds;
                wsPingTimer.Start();
                wsCheckTimer.Start();
                xhrRequestTimer.Start();
            }
            dataBuffers.Recv.Reset();
            quotesWS.ReceiveAsync(dataBuffers.Recv.Memory, cancelSource.Token).ContinueWithPost(svc, onReceiveAsync);
        }

        private void SendPing()
        {
            var msg = new DataWsMessage(2);
            var msgBytes = Encoding.UTF8.GetBytes(DataWsMessage.Format(msg));
            CancellationTokenSource cancelSource = new(DataWsSendTimeout);
            void onSendAsync(Task task)
            {
                if (task.IsCompletedSuccessfully && dataWsSession != null)
                    dataWsSession.LastPing = DateTime.UtcNow;
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            logger.Error($"Exception @04 <{innerEx.GetType().FullName}> {innerEx.Message}");
                    ResetWsSession();
                }
            }
            if (dataWsSession != null)
                quotesWS.SendAsync(new(msgBytes), WebSocketMessageType.Text, true, cancelSource.Token).ContinueWithPost(svc, onSendAsync);
        }

        private void DoCheck()
        {
            void onCheckSent()
            {
                if (PollSubscription(out var subscription) && subscription is VO.Subscription subj) InitSubscription(subj);
            }
            SendCheck(onCheckSent);
        }

        private bool PollSubscription(out VO.Subscription? subscription)
        {
            subscription = null;
            var rawClientId = new Mt5Bridge.RawBuffer(InteropBuffer, InteropBufferChunk);
            var rawSymbol = new Mt5Bridge.RawBuffer(InteropBuffer + (int) InteropBufferChunk, InteropBufferChunk);
            var dataParams = new Mt5Bridge.DataParams();
            var success = Mt5Bridge.MT5S_PollRequest(out var haveRequest, ref rawClientId, ref rawSymbol, ref dataParams);
            if (success && haveRequest)
            { 
                byte[] clientIdBytes = new byte[rawClientId.DataSize];
                Marshal.Copy(rawClientId.DataPtr, clientIdBytes, 0, (int) rawClientId.DataSize);
                var clientId = Util.BytesToHex(clientIdBytes);
                byte[] symbolBytes = new byte[rawSymbol.DataSize];
                Marshal.Copy(rawSymbol.DataPtr, symbolBytes, 0, (int) rawSymbol.DataSize);
                var symbol = Encoding.UTF8.GetString(symbolBytes);
                subscription = new VO.Subscription(clientId, symbol, dataParams);
            }
            return success;
        }

        void InitSubscription(VO.Subscription subj)
        {
            subscriptionsMap[subj.ClientId] = subj;
            if (!symbolsDataMap.TryGetValue(subj.Symbol, out SymbolDataDescriptor? sdd))
                symbolsDataMap[subj.Symbol] = sdd = new SymbolDataDescriptor();
            ++ sdd.RefCount;
            SymbolDataDescriptor.TfDataDescriptorPair? tddp = null;
            if (!sdd.TimeframesMap.TryGetValue(subj.DataParams.MainTimeframe, out tddp))
                sdd.TimeframesMap[subj.DataParams.MainTimeframe] = tddp = new() { Main = new SymbolDataDescriptor.MainTfDataDescriptor() };
            else if (tddp.Main == null)
                tddp.Main = new();
            ++ tddp.Main.RefCount;
            if (!sdd.TimeframesMap.TryGetValue(subj.DataParams.TrendTimeframe, out tddp))
                sdd.TimeframesMap[subj.DataParams.TrendTimeframe] = tddp = new() { Trend = new SymbolDataDescriptor.TrendTfDataDescriptor() };
            else if (tddp.Trend == null)
                tddp.Trend = new();
            ++ tddp.Trend.RefCount;
            tddp.Trend.MaxPeriod = Math.Max(Math.Max(subj.DataParams.WithTrend.TrendPeriod, subj.DataParams.AgainstTrend.TrendPeriod), tddp.Trend.MaxPeriod);
            Subscribe(subj.Symbol);
        }

        private void SendCheck(Action? onCompleted = null)
        {
            var msg = new DataWsMessage(42, new JArray() { "check" });
            var msgBytes = Encoding.UTF8.GetBytes(DataWsMessage.Format(msg));
            CancellationTokenSource cancelSource = new(DataWsSendTimeout);
            void onSendAsync(Task task)
            {
                if (task.IsCompletedSuccessfully)
                    onCompleted?.Invoke();
                if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            logger.Error($"Exception @05 <{innerEx.GetType().FullName}> {innerEx.Message}");
                    ResetWsSession();
                }
            }
            if (dataWsSession != null)
                quotesWS.SendAsync(new(msgBytes), WebSocketMessageType.Text, true, cancelSource.Token).ContinueWithPost(svc, onSendAsync);
        }

        private void RecvData()
        {
            void onReceiveAsync(Task<WebSocketReceiveResult> task)
            { 
                if (task.IsCompletedSuccessfully && dataWsSession != null)
                {
                    var wsResult = task.Result;
                    if (wsResult.MessageType == WebSocketMessageType.Text)
                    {
                        if (!wsResult.EndOfMessage)
                        {
                            dataBuffers.Recv.Extend(wsResult.Count);
                            quotesWS.ReceiveAsync(dataBuffers.Recv.Memory, svcCancelSource.Token).ContinueWithPost(svc, onReceiveAsync);
                        }
                        var msgText = Encoding.UTF8.GetString(dataBuffers.Recv.Array, 0, wsResult.Count);
                        var msg = DataWsMessage.Parse(msgText);
                        if (msg != null) ProcessDataWsMessage(msg);
                    }
                    else
                        logger.Error($"Received unexpected message type.");
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            logger.Error($"Exception @06 <{innerEx.GetType().FullName}> {innerEx.Message}");
                    ResetWsSession();
                }
                svc.Post(RecvData);
            }
            if (dataWsSession != null)
            {
                dataBuffers.Recv.Reset();
                quotesWS.ReceiveAsync(dataBuffers.Recv.Memory, svcCancelSource.Token).ContinueWithPost(svc, onReceiveAsync);
            }
        }

        void ProcessDataWsMessage(DataWsMessage msg)
        {
            if (msg.TypeCode == 42)
            {
                if (msg.ContentJSON is JArray contentJSON)
                {
                    if (2 <= contentJSON.Count)
                    {
                        if (contentJSON[0] is JValue nameJSON && nameJSON.Value<string>() == "newPrice" && contentJSON[1] is JObject quoteJSON)
                        {
                            var quoteMsg = quoteJSON.ToObject<VO.QuoteMsg>(JsonSerializer);
                            if (quoteMsg != null) DispatchQuoteMessage(quoteMsg);
                        }
                    }
                }
            }
        }

        void DispatchQuoteMessage(VO.QuoteMsg quoteMsg)
        {
            var symbol = quoteMsg.Instrument;
            var tickTime = quoteMsg.TickTime;
            if (symbolsDataMap.TryGetValue(symbol, out var symbolDesc) && symbolDesc.LastTickTime < tickTime)
            {
                symbolDesc.LastTickTime = tickTime;
                var lastCandle = quoteMsg.Data[^1];
                var lastPrice = lastCandle.Close;
                var clientIds = subscriptionsMap.Where((kv) => kv.Value.Symbol == symbol).Select((kv) => kv.Key);
                foreach (var clientId in clientIds)
                {
                    var clientIdBytes = Util.HexToBytes(clientId);
                    var dataSize = (uint) clientIdBytes.Length;
                    var rawClientId = new Mt5Bridge.RawBuffer(InteropBuffer, dataSize) { DataSize = dataSize };
                    Marshal.Copy(clientIdBytes, 0, rawClientId.DataPtr, (int) rawClientId.DataSize);
                    var data = new Mt5Bridge.MarketData()
                    { 
                        What = Mt5Bridge.MarketData.WhatEnum.Tick,
                        Tick = new Mt5Bridge.MarketData.TickData()
                        { 
                            Time = (ulong) new DateTimeOffset(tickTime).ToUnixTimeSeconds(),
                            Price = decimal.ToDouble(lastPrice)
                        }
                    };
                    Mt5Bridge.MT5S_SendData(in rawClientId, in data);
                }
            }
        }

        void DispatchIndicatorData(IndicatorDataCtx ctx, JObject dataJSON)
        {
            bool success = true;
            var indCustom4 = dataJSON.ToObject<VO.IndCustom4Data>(JsonSerializer);
            if (indCustom4 != null)
            {
                if (symbolsDataMap.TryGetValue(ctx.Symbol, out var sdd) && sdd.TimeframesMap.TryGetValue(ctx.Timeframe, out var tddp))
                {
                    if (tddp.Main != null)
                    {
                        tddp.Main.BatchSerial = ctx.BatchSerial;
                        tddp.Main.Time = ctx.Time;
                        bool? getVaColourTag(VO.IndCustom4Data.Values v) =>
                            v.RedVA.HasValue && !v.GreenVA.HasValue ? false : v.GreenVA.HasValue && !v.RedVA.HasValue ? true : null;
                        int count = indCustom4.Data.Length;
                        if (0 < count)
                        {
                            /* DEBUG >>>
                            logger.Debug
                                (
                                    "DispatchIndicatorData(): #{0}, symbol = {1}, TF = {2}, MAIN, count = {3}",
                                    ctx.BatchSerial, ctx.Symbol, ctx.Timeframe, count
                                );
                            <<<DEBUG */
                            int extCount = tddp.Main.VA.Length;
                            var newVA = new (DateTime Time, decimal Price, bool? Colour)[extCount + count];
                            for (int i = 1 ; i <= count ; i ++)
                            {
                                var values = indCustom4.Data[^i];
                                var timeShift = TimeSpan.FromMinutes((i - 1) * (double) ctx.Timeframe);
                                var time = ctx.Time - timeShift;
                                if (values.VA.HasValue && values.VA.Value != 0 && values.Time == time)
                                    newVA[i - 1] = (time, values.VA.Value, getVaColourTag(values));
                                else
                                {
                                    logger.Warn("Data error: symbol '{0}', timeframe '{1}'.", ctx.Symbol, ctx.Timeframe.ToString());
                                    success = false;
                                    break;
                                }
                            }
                            if (success)
                            {
                                int newCount = count;
                                bool proceed = true;
                                for (int i = 0 ; proceed && i < extCount ; i ++, newCount ++)
                                {
                                    var timeShift = TimeSpan.FromMinutes((count + i) * (double) ctx.Timeframe);
                                    var time = ctx.Time - timeShift;
                                    proceed = false;
                                    for (int j = 0 ; j < extCount ; j ++)
                                        if (tddp.Main.VA[j].Time == time)
                                        { 
                                            newVA[count + i] = tddp.Main.VA[j];
                                            proceed = true;
                                            break;
                                        }
                                }
                                if (3 < newCount) newCount = 3;
                                tddp.Main.VA = newVA[0..newCount];
                            }
                        }
                        else
                        {
                            logger.Error("No data for '{1}' timeframe of '{0}' symbol.", ctx.Symbol, ctx.Timeframe.ToString());
                            success = false;
                        }
                    }
                    if (tddp.Trend != null)
                    {
                        tddp.Trend.BatchSerial = ctx.BatchSerial;
                        int count = indCustom4.Data.Length;
                        /* DEBUG >>>
                        logger.Debug
                            (
                                "DispatchIndicatorData(): #{0}, symbol = {1}, TF = {2}, TREND, count = {3}",
                                ctx.BatchSerial, ctx.Symbol, ctx.Timeframe, count
                            );
                        <<<DEBUG */
                        if (0 < count)
                        {


                            int extCount = tddp.Trend.Median.Length;
                            var newMedian = new (DateTime Time, decimal Price)[extCount + count];
                            for (int i = 1; i <= count; i++)
                            {
                                var values = indCustom4.Data[^i];
                                var timeShift = TimeSpan.FromMinutes((i - 1) * (double)ctx.Timeframe);
                                var time = ctx.Time - timeShift;
                                if (values.Median.HasValue && values.Median.Value != 0 && values.Time == time)
                                    newMedian[i - 1] = (time, values.Median.Value);
                                else
                                {
                                    logger.Warn("Data error: symbol '{0}', timeframe '{1}'.", ctx.Symbol, ctx.Timeframe.ToString());
                                    success = false;
                                    break;
                                }
                            }
                            if (success)
                            {
                                int newCount = count;
                                bool proceed = true;
                                for (int i = 0; proceed && i < extCount; i++, newCount++)
                                {
                                    var timeShift = TimeSpan.FromMinutes((count + i) * (double) ctx.Timeframe);
                                    var time = ctx.Time - timeShift;
                                    proceed = false;
                                    for (int j = 0; j < extCount; j++)
                                        if (tddp.Trend.Median[j].Time == time)
                                        {
                                            newMedian[count + i] = tddp.Trend.Median[j];
                                            proceed = true;
                                            break;
                                        }
                                }
                                if (tddp.Trend.MaxPeriod + 1 < newCount) newCount = (int) (tddp.Trend.MaxPeriod + 1);
                                tddp.Trend.Median = newMedian[0..newCount];
                            }
                        }
                        else
                        {
                            logger.Error("No data for '{1}' timeframe of '{0}' symbol.", ctx.Symbol, ctx.Timeframe.ToString());
                            success = false;
                        }
                    }
                }
                else
                {
                    logger.Error("No subscription for '{1}' timeframe of '{0}' symbol.", ctx.Symbol, ctx.Timeframe.ToString());
                    success = false;
                }
            }
            else
            {
                logger.Warn("Received invalid/unexpected JSON format.");
                success = false;
            }
            if (success)
            {
                bool filterSubscription(VO.Subscription subj) =>
                    ctx.Symbol == subj.Symbol && (ctx.Timeframe == subj.DataParams.MainTimeframe || ctx.Timeframe == subj.DataParams.TrendTimeframe);
                foreach (var (clientId, subj) in subscriptionsMap.Where((kv) => filterSubscription(kv.Value)))
                {
                    var clientIdBytes = Util.HexToBytes(clientId);
                    var dataSize = (uint) clientIdBytes.Length;
                    var rawClientId = new Mt5Bridge.RawBuffer(InteropBuffer, dataSize) { DataSize = dataSize };
                    Marshal.Copy(clientIdBytes, 0, rawClientId.DataPtr, (int) rawClientId.DataSize);
                    if (symbolsDataMap.TryGetValue(subj.Symbol, out var sdd))
                    {
                        if 
                            (
                                sdd.TimeframesMap.TryGetValue(subj.DataParams.MainTimeframe, out var tddpMain) &&
                                sdd.TimeframesMap.TryGetValue(subj.DataParams.TrendTimeframe, out var tddpTrend) &&
                                tddpMain.Main != null && tddpTrend.Trend != null &&
                                (tddpMain.Main.BatchSerial != 0 && tddpMain.Main.BatchSerial == tddpTrend.Trend.BatchSerial)
                            )
                        {
                            var tfdMain = tddpMain.Main;
                            var tfdTrend = tddpTrend.Trend;
                            var data = new Mt5Bridge.MarketData()
                            { 
                                What = Mt5Bridge.MarketData.WhatEnum.Signal,
                                Signal = new Mt5Bridge.MarketData.SignalData()
                            };
                            /* DEBUG >>>
                            logger.Debug("DispatchIndicatorData(): #{0}, VA.Length = {1}", ctx.BatchSerial, tfdMain.VA.Length);
                            <<< DEBUG */
                            if (3 <= tfdMain.VA.Length)
                            {
                                Mt5Bridge.MarketData.PriceAndDir getVA(int index)
                                {
                                    var va = tfdMain.VA[index];
                                    DirEnum dir = va.Colour.HasValue ? va.Colour.Value ? DirEnum.Up : DirEnum.Down : DirEnum.None;
                                    return new() { Dir = dir, Price = decimal.ToDouble(va.Price) };
                                }
                                data.Signal.Time = (ulong) new DateTimeOffset(tfdMain.Time).ToUnixTimeSeconds();
                                data.Signal.VA_0 = getVA(0);
                                data.Signal.VA_1 = getVA(1);
                                data.Signal.VA_2 = getVA(2);
                                data.Signal.ATR = 0;                // FIXME!
                            }
                            else
                            {
                                logger.Warn("Insufficient data for symbol #1 '{0}'.", subj.Symbol);
                                success = false;
                            }
                            {
                                DirEnum getMedianDir(uint period, uint start)
                                {
                                    DirEnum dir = DirEnum.None;
                                    for (uint i = period + start - 1 ; start < i ; i --)
                                        if (tfdTrend.Median[i].Price < tfdTrend.Median[i - 1].Price)
                                        {
                                            if (dir == DirEnum.Down) { dir = DirEnum.None; break; }
                                            dir = DirEnum.Up;
                                        }
                                        else if (tfdTrend.Median[i - 1].Price < tfdTrend.Median[i].Price)
                                        {
                                            if (dir == DirEnum.Up) { dir = DirEnum.None; break; }
                                            dir = DirEnum.Down;
                                        }
                                    return dir;
                                }
                                var count = tfdTrend.Median.Length;
                                var trendPeriodW = subj.DataParams.WithTrend.TrendPeriod;
                                var trendPeriodA = subj.DataParams.AgainstTrend.TrendPeriod;
                                /* DEBUG >>>
                                logger.Debug
                                    (
                                        "DispatchIndicatorData(): #{0}, Median.Length = {1}, trendPeriodW = {2}, trendPeriodA = {3}",
                                        ctx.BatchSerial, count, trendPeriodW, trendPeriodA
                                    );
                                <<< DEBUG */
                                if (2 <= trendPeriodW)
                                    if (trendPeriodW + 1 <= count)
                                    {
                                        data.Signal.MedianW.M_0 = new()
                                        { 
                                            Price = decimal.ToDouble(tfdTrend.Median[0].Price),
                                            Dir = getMedianDir(trendPeriodW, 0)
                                        };
                                        data.Signal.MedianW.M_1 = new()
                                        { 
                                            Price = decimal.ToDouble(tfdTrend.Median[1].Price),
                                            Dir = getMedianDir(trendPeriodW, 1)
                                        };
                                    }
                                    else
                                    {
                                        logger.Warn("Insufficient data for symbol #2 '{0}'.", subj.Symbol);
                                        success = false;
                                    }
                                if (2 <= trendPeriodA)
                                    if (trendPeriodA + 1 <= count)
                                    {
                                        data.Signal.MedianA.M_0 = new()
                                        { 
                                            Price = decimal.ToDouble(tfdTrend.Median[0].Price),
                                            Dir = getMedianDir(trendPeriodA, 0)
                                        };
                                        data.Signal.MedianA.M_1 = new()
                                        { 
                                            Price = decimal.ToDouble(tfdTrend.Median[1].Price),
                                            Dir = getMedianDir(trendPeriodA, 1)
                                        };
                                    }
                                    else
                                    {
                                        logger.Warn("Insufficient data for symbol #3 '{0}'.", subj.Symbol);
                                        success = false;
                                    }
                            }
                            if (success) Mt5Bridge.MT5S_SendData(in rawClientId, in data);
                        }
                    }
                    else
                    {
                        logger.Error("Data for symbol '{0}' not found.", subj.Symbol);
                        success = false;
                    }
                }
            }
        }

        private void ResetWsSession()
        {
            logger.Info("Resetting session.");
            void stopTimers()
            {
                wsPingTimer.Stop();
                wsCheckTimer.Stop();
            }
            dataWsSession = null;
            authInfo = null;
            stopTimers();
            svc.ClearQueue();
            quotesWS = new();
            isResetting = true;
            svc.Post(Start);
        }

        private void Subscribe(string symbol, Action? onCompleted = null)
        {
            byte[] buildSubscribeMessage()
            { 
                var msg = new DataWsMessage(42, new JArray() { "subscribe", $"{Provider}:{symbol}", QuotesTimeframe.ToString() });
                var msgBytes = Encoding.UTF8.GetBytes(DataWsMessage.Format(msg));
                return msgBytes;
            }
            var msgBytes = buildSubscribeMessage();
            CancellationTokenSource cancelSource = new(DataWsSendTimeout);
            void onSendAsync(Task task)
            {
                if (task.IsCompletedSuccessfully)
                    onCompleted?.Invoke();
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            logger.Error($"Exception @07 <{innerEx.GetType().FullName}> {innerEx.Message}");
                    ResetWsSession();
                }
            }
            if (dataWsSession != null)
                quotesWS.SendAsync(new(msgBytes), WebSocketMessageType.Text, true, cancelSource.Token).ContinueWithPost(svc, onSendAsync);
        }

        private void RestoreSubscriptions()
        {
            var enumerator = symbolsDataMap.Keys.GetEnumerator();
            void onStartAndCompleted()
            {
                if (enumerator.MoveNext())
                { 
                    var symbol = enumerator.Current;
                    Subscribe(symbol, onStartAndCompleted);
                }
            }
            onStartAndCompleted();
        }

        private void RequestIndicatorsData(IndicatorDataDispatcher dispatcher)
        {
            void onSendAsync(Task<HttpResponseMessage> task, IndicatorDataCtx ctx)
            {
                if (task.IsCompletedSuccessfully)
                {
                    var rsp = task.Result;
                    if (rsp.IsSuccessStatusCode)
                        rsp.Content.ReadAsStringAsync().ContinueWithPost(svc, (task) => onReadAsStringAsync(task, ctx));
                    else
                        logger.Error($"Server response: {(int) rsp.StatusCode} {rsp.ReasonPhrase}");
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            logger.Error($"Exception @08 <{innerEx.GetType().FullName}> {innerEx.Message}");
                }
            }
            void onReadAsStringAsync(Task<string> task, IndicatorDataCtx ctx)
            {
                if (task.IsCompletedSuccessfully)
                {
                    var content = task.Result;
                    var dataJSON = JObject.Parse(content);
                    dispatcher.Invoke(ctx, dataJSON);
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            logger.Error($"Exception @09 <{innerEx.GetType().FullName}> {innerEx.Message}");
                }
            }
            if (authInfo is not null)
            {
                ++ _batchSerial;
                foreach (var (symbol, symbolDesc) in symbolsDataMap)
                {
                    void processTimeframe(Mt5Bridge.Timeframe timeframe, uint period)
                    {
                        var time = symbolDesc.LastTickTime;
                        if (time == DateTime.MinValue) return;
                        double tfMinutes = (double) timeframe;
                        double tfShift = Math.Floor((time - time.Date).TotalMinutes / tfMinutes) * tfMinutes;
                        time = time.Date + TimeSpan.FromMinutes(tfShift);
                        var timeTo = time + TimeSpan.FromMinutes(tfMinutes);
                        var timeFrom = timeTo - TimeSpan.FromMinutes((period + 1) * tfMinutes);
                        /* DEBUG >>>
                        logger.Debug
                            (
                                "RequestIndicatorsData(): #{0}, symbol = {1}, TF = {2}, period = {3}, timeFrom = {4}, timeTo = {5}",
                                _batchSerial, symbol, timeframe, period, timeFrom, timeTo
                            );
                        <<< DEBUG */
                        foreach (var indicator in IndicatorNames)
                        {
                            var ctx = new IndicatorDataCtx(_batchSerial, indicator, symbol, timeframe, time);
                            var uri = IndicatorXhrUriFactory(indicator, Provider.ToString(), symbol, timeframe, timeFrom, timeTo);
                            var req = new HttpRequestMessage(HttpMethod.Get, uri);
                            req.Headers.Authorization = new("Token", authInfo.AccessToken);
                            httpClient.SendAsync(req).ContinueWithPost(svc, (task) => onSendAsync(task, ctx));
                        }
                    }
                    foreach (var (tf, tfDescPair) in symbolDesc.TimeframesMap)
                    {
                        uint period = Math.Max(tfDescPair.Main != null ? 4U : 0U, tfDescPair.Trend != null ? tfDescPair.Trend.MaxPeriod + 1 : 0U);
                        if (period != 0) processTimeframe(tf, period);
                    }
                }
            }
        }
    }
}
