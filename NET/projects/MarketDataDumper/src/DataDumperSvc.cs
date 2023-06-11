using System;
using System.Collections.Generic;
using HttpUtility = System.Web.HttpUtility;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timers = System.Timers;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Commons;
using static Commons.Extensions.TaskExtensions;
using Util;


namespace MarketDataProvider
{
    class DataDumperSvc
    {
        private const string UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.141 Safari/537.36";
        private const string DomainName = "platform.chartingcenter.com";
        private static readonly Uri BaseURI = new($"https://{DomainName}");
        private static readonly Uri BaseWsURI = new($"wss://{DomainName}");
        private static readonly Uri RootPageURI = new(BaseURI, "/");
        private static readonly Uri LoginPageURI = new(BaseURI, "/login");
        private static readonly Uri ApiAuthURI = new(BaseURI, "/api/authtoken");
        private static readonly Func<string, Uri> WsUriFactory = (token) => new(BaseWsURI, $"/socket.io/?token={token}&EIO=3&transport=websocket");
        private static readonly Func<string, string, string, string, DateTime, DateTime, Uri> IndicatorXhrUriFactory =
            (indicator, instrument, timeframe, provider, dateFrom, dateTo) =>
                new
                (
                    BaseURI,
                    $@"/api/indicator?" +
                    $@"indicator={indicator}" +
                    $@"&instrument={HttpUtility.UrlEncode(instrument).ToUpper()}" +
                    $@"&tf={HttpUtility.UrlEncode(timeframe).ToUpper()}" + 
                    $@"&provider={HttpUtility.UrlEncode(provider).ToUpper()}" +
                    $@"&dateFrom={HttpUtility.UrlEncode($"{dateFrom:dd.MM.yyyy HH:mm}:00").ToUpper()}" +
                    $@"&dateTo={HttpUtility.UrlEncode($"{dateTo:dd.MM.yyyy HH:mm}:00").ToUpper()}"
                );
        private static readonly string[] IndicatorNames = { "custom_2", "custom_4" };

        private static readonly JToken loginJSON = new JObject { ["email"] = "geoffrey@geoff7.plus.com", ["password"] = "mcgowan" };

        private const string SvcThreadName = nameof(DataDumperSvc);
        private const int SvcQueueLimit = 100;
        private static readonly (int Recv, int Send) DataMemorySize = (0x40000, 0x100);

        private static readonly TimeSpan DataWsCheckInterval = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan DataWsInitTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan DataWsSendTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan IndicatorXhrRequestInterval = TimeSpan.FromSeconds(1);

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

        private Dictionary<VO.Subscription, DateTime> lastTicksTime = new(); 

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

        private VO.DataDumperParams parameters;

        public DataDumperSvc(VO.DataDumperParams parameters)
        {
            this.parameters = parameters;
            httpHandler = new() { CookieContainer = cookieContainer };
            httpClient = new(httpHandler);
            var defaultHeaders = httpClient.DefaultRequestHeaders;
            defaultHeaders.UserAgent.Clear(); defaultHeaders.UserAgent.ParseAdd(UserAgent);
            defaultHeaders.Referrer = BaseURI;
            quotesWS.Options.Cookies = cookieContainer;
            wsPingTimer.Elapsed += (_, _) => svc.Post(SendPing);
            wsCheckTimer.Elapsed += (_, _) => svc.Post(SendCheck);
            xhrRequestTimer.Elapsed += (_, _) => svc.Post(RequestIndicatorsData);
        }

        public void Run()
        {
            ConsoleLogger.Announce("Market data dumper started.");
            svc.Run(SvcThreadName);
            svc.Post(Start);
            svcCancelSource.Token.WaitHandle.WaitOne();
        }

        public void Shutdown()
        {
            ConsoleLogger.Announce("Shutting down. Please wait...");
            svc.Stop();
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
                            ConsoleLogger.Error($"Exception <{innerEx.GetType().FullName}> {innerEx.Message}");
                }
            }
            ProcLogin().ContinueWithPost(svc, onLogin);
        }

        private async Task<bool> ProcLogin()
        {
            ConsoleLogger.Info("Loading ChartingCenter login page...", false);
            var rspLoginPage = await httpClient.GetAsync(LoginPageURI);
            if (rspLoginPage.IsSuccessStatusCode)
            {
                ConsoleLogger.SetStatus(true);
                ConsoleLogger.Info("Logging on to ChartingCenter platform...", false);
                var authContent = new StringContent(loginJSON.ToString(Formatting.None), Encoding.UTF8, "application/json");
                var rspAuth = await httpClient.PostAsync(ApiAuthURI, authContent);
                if (rspAuth.IsSuccessStatusCode)
                {
                    ConsoleLogger.SetStatus(true);
                    ConsoleLogger.Details($"Server response: {(int)rspAuth.StatusCode} {rspAuth.ReasonPhrase}");
                    var content = await rspAuth.Content.ReadAsStringAsync();
                    var authInfoJSON = JObject.Parse(content);
                    authInfo = new(Id: authInfoJSON.Value<string>("id"), AccessToken: authInfoJSON.Value<string>("accessToken"));
                    ConsoleLogger.Info("Fetching the home page...", false);
                    var rspRootPage = await httpClient.GetAsync(RootPageURI);
                    if (rspRootPage.IsSuccessStatusCode)
                    {
                        ConsoleLogger.SetStatus(true);
                        return true;
                    }
                    else
                    {
                        ConsoleLogger.SetStatus(false);
                        ConsoleLogger.Error($"Server responce: {(int) rspRootPage.StatusCode} {rspRootPage.ReasonPhrase}");
                        return false;
                    }
                }
                else
                {
                    ConsoleLogger.SetStatus(false);
                    ConsoleLogger.Error($"Server responce: {(int) rspAuth.StatusCode} {rspAuth.ReasonPhrase}");
                    return false;
                }
            }
            else
            {
                ConsoleLogger.SetStatus(false);
                ConsoleLogger.Error($"Server responce: {(int) rspLoginPage.StatusCode} {rspLoginPage.ReasonPhrase}");
                return false;
            }
        }

        private void ConnectToDataWS()
        {
            ConsoleLogger.Info("Connecting to quotes data web-socket...", false);
            void onConnectAsync(Task task)
            {
                if (task.IsCompletedSuccessfully)
                {
                    ConsoleLogger.SetStatus(true);
                    svc.Post(RecvDataWsInitMessage);
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            ConsoleLogger.Error($"Exception <{innerEx.GetType().FullName}> {innerEx.Message}");
                    ResetWsSession();
                }
            }
            if (authInfo is AuthInfo ai)
                quotesWS.ConnectAsync(WsUriFactory(ai.AccessToken), CancellationToken.None).ContinueWithPost(svc, onConnectAsync);
        }

        private void RecvDataWsInitMessage()
        {
            if (authInfo is null) return;
            ConsoleLogger.Info("Waiting for init message...", false);
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
                            ConsoleLogger.SetStatus(true);
                            startTimers();
                            svc.Post(RecvData);
                            svc.Post(Subscribe);
                        }
                    }
                    else
                        ConsoleLogger.Error($"Received unexpected message type.");
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            ConsoleLogger.Error($"Exception <{innerEx.GetType().FullName}> {innerEx.Message}");
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
                            ConsoleLogger.Error($"Exception <{innerEx.GetType().FullName}> {innerEx.Message}");
                    ResetWsSession();
                }
            }
            if (dataWsSession != null)
                quotesWS.SendAsync(new(msgBytes), WebSocketMessageType.Text, true, cancelSource.Token).ContinueWithPost(svc, onSendAsync);
        }

        private void SendCheck()
        {
            var msg = new DataWsMessage(42, new JArray() { "check" });
            var msgBytes = Encoding.UTF8.GetBytes(DataWsMessage.Format(msg));
            CancellationTokenSource cancelSource = new(DataWsSendTimeout);
            void onSendAsync(Task task)
            { 
                if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            ConsoleLogger.Error($"Exception <{innerEx.GetType().FullName}> {innerEx.Message}");
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
                        if (msg != null)
                        {
                            ConsoleLogger.Info($"Quotes data:");
                            ConsoleLogger.Details(msg.ContentJSON?.ToString(Formatting.None) ?? "");
                        }
                    }
                    else
                        ConsoleLogger.Error($"Received unexpected message type.");
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            ConsoleLogger.Error($"Exception <{innerEx.GetType().FullName}> {innerEx.Message}");
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

        private void ResetWsSession()
        {
            ConsoleLogger.Info("Resetting session...", false);
            void stopTimers()
            {
                wsPingTimer.Stop();
                wsCheckTimer.Stop();
            }
            dataWsSession = null;
            stopTimers();
            svc.ClearQueue();
            quotesWS = new();
            ConsoleLogger.SetStatus(true);
            svc.Post(ConnectToDataWS);
        }

        private void Subscribe()
        {
            ConsoleLogger.Info("Subscribing to market quotes...");
            using var subjEnumerator = ((IList<VO.Subscription>) parameters.Subscriptions).GetEnumerator();
            byte[] buildSubscribeMessage(in VO.Subscription subj)
            { 
                var msg = new DataWsMessage(42, new JArray() { "subscribe", $"{subj.Provider}:{subj.Symbol}", subj.Timeframe });
                var msgBytes = Encoding.UTF8.GetBytes(DataWsMessage.Format(msg));
                return msgBytes;
            }
            CancellationTokenSource cancelSource = new(DataWsSendTimeout);
            void onStartAndSendAsync(Task task)
            {
                if (task.IsCompletedSuccessfully && dataWsSession != null)
                {
                    if (subjEnumerator.MoveNext())
                    {
                        var msgBytes = buildSubscribeMessage(subjEnumerator.Current);
                        quotesWS.SendAsync(new(msgBytes), WebSocketMessageType.Text, true, cancelSource.Token).ContinueWithPost(svc, onStartAndSendAsync);
                    }
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            ConsoleLogger.Error($"Exception <{innerEx.GetType().FullName}> {innerEx.Message}");
                    ResetWsSession();
                }
            }
            onStartAndSendAsync(Task.CompletedTask);
        }

        private void RequestIndicatorsData()
        {
            void onSendAsync(Task<HttpResponseMessage> task)
            {
                if (task.IsCompletedSuccessfully)
                {
                    var rsp = task.Result;
                    if (rsp.IsSuccessStatusCode)
                    {
                        ConsoleLogger.Details($"Server response: {(int) rsp.StatusCode} {rsp.ReasonPhrase}");
                        rsp.Content.ReadAsStringAsync().ContinueWithPost(svc, onReadAsStringAsync);
                    }
                    else
                        ConsoleLogger.Details($"Server response: {(int) rsp.StatusCode} {rsp.ReasonPhrase}");
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            ConsoleLogger.Error($"Exception <{innerEx.GetType().FullName}> {innerEx.Message}");
                }
            }
            void onReadAsStringAsync(Task<string> task)
            {
                if (task.IsCompletedSuccessfully)
                {
                    var content = task.Result;
                    var dataJSON = JObject.Parse(content);
                    ConsoleLogger.Info($"Indicator data:");
                    ConsoleLogger.Details(dataJSON.ToString(Formatting.None));
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            ConsoleLogger.Error($"Exception <{innerEx.GetType().FullName}> {innerEx.Message}");
                }
            }
            if (authInfo is not null)
                foreach (var indicator in IndicatorNames)
                    foreach (var subj in parameters.Subscriptions)
                    {
                        var dateTo = DateTime.UtcNow;
                        var dateFrom = dateTo - TimeSpan.FromMinutes(4);
                        var uri = IndicatorXhrUriFactory(indicator, subj.Symbol, subj.Timeframe, subj.Provider, dateFrom, dateTo);
                        var req = new HttpRequestMessage(HttpMethod.Get, uri);
                        req.Headers.Authorization = new("Token", authInfo.AccessToken);
                        httpClient.SendAsync(req).ContinueWithPost(svc, onSendAsync);
                    }
        }
    }
}
