using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpUtility = System.Web.HttpUtility;

using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Commons;
using static Commons.Extensions.TaskExtensions;


namespace TestDataGenerator
{
    public class DataFetcherSvc
    {
        private const string UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.141 Safari/537.36";
        private const string DomainName = "platform.chartingcenter.com";
        private static readonly Uri BaseURI = new($"https://{DomainName}");
        private static readonly Uri RootPageURI = new(BaseURI, "/");
        private static readonly Uri LoginPageURI = new(BaseURI, "/login");
        private static readonly Uri ApiAuthURI = new(BaseURI, "/api/authtoken");
        private static readonly Func<string, string, string, VO.Timeframe, DateTime, DateTime, Uri> IndicatorXhrUriFactory =
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

        private static Dictionary<string, int> _symbolsDigits = new()
        {
            ["AUDCAD"] = 5,
            ["AUDCHF"] = 5,
            ["AUDJPY"] = 3,
            ["AUDNZD"] = 5,
            ["AUDUSD"] = 5,
            ["CADCHF"] = 5,
            ["CADJPY"] = 3,
            ["CHFJPY"] = 3,
            ["EURAUD"] = 5,
            ["EURCAD"] = 5,
            ["EURCHF"] = 5,
            ["EURGBP"] = 5,
            ["EURJPY"] = 3,
            ["EURNZD"] = 5,
            ["EURUSD"] = 5,
            ["GBPAUD"] = 5,
            ["GBPCAD"] = 5,
            ["GBPCHF"] = 5,
            ["GBPJPY"] = 3,
            ["GBPNZD"] = 5,
            ["GBPUSD"] = 5,
            ["NZDCAD"] = 5,
            ["NZDCHF"] = 5,
            ["NZDJPY"] = 3,
            ["NZDUSD"] = 5,
            ["USDCAD"] = 5,
            ["USDCHF"] = 5,
            ["USDJPY"] = 3,
            ["GER30"] = 2,
            ["SPX500"] = 2,
            ["UKOIL"] = 3,
            ["US30"] = 2,
            ["USOIL"] = 3,
            ["XAGUSD"] = 3,
            ["XAUUSD"] = 2
        };

        private const int ExtraIndicatorDigits = 3;
        private static readonly string[] IndicatorNames = { "custom_4" };

        private static readonly JToken LoginJSON = new JObject { ["email"] = "geoffrey@geoff7.plus.com", ["password"] = "mcgowan" };

        private const string SvcThreadName = nameof(DataFetcherSvc);
        private const int SvcQueueLimit = 100;
        private static readonly TimeSpan IndicatorXhrRequestInterval = TimeSpan.FromSeconds(1);

        private readonly string _outputPath;
        private readonly CookieContainer _cookieContainer = new();
        private readonly HttpClientHandler _httpHandler;
        private readonly HttpClient _httpClient;
        private bool _ready = false;

        private AsyncService _svc = new (SvcQueueLimit);

        private CancellationTokenSource _svcCancelSource = new();

        private record AuthInfo(string Id, string AccessToken);

        private AuthInfo? _authInfo = null;

        private record BatchDesc(string Symbol, VO.Timeframe Timeframe, DateTime TimeFrom, DateTime TimeTo, TimeSpan TimeStep, uint Count);

        private Queue<BatchDesc> _batches = new();
        private Dictionary<(string Symbol, VO.Timeframe Timeframe, int Year), DataFile> _symbolsFiles = new();

        private record IndicatorDataCtx(string Indicator, BatchDesc Batch, int RoundDigits);

        delegate void IndicatorDataDispatcher(IndicatorDataCtx ctx, JObject dataJSON);

        public record Params(string[] Symbols, VO.Timeframe[] Timeframes, (DateTime From, DateTime Till) DateRange);

        private decimal _initialBatchesCount = 0;

        public delegate void ProgressEventDelegate(decimal value);
        public delegate void DoneEventDelegate();

        public event ProgressEventDelegate? OnProgress;
        public event DoneEventDelegate? OnDone;

        private Logger _logger = LogManager.GetLogger(nameof(DataFetcherSvc));

        public DataFetcherSvc(string outputPath)
        {
            _outputPath = outputPath;
            _httpHandler = new() { CookieContainer = _cookieContainer };
            _httpClient = new(_httpHandler);
            var defaultHeaders = _httpClient.DefaultRequestHeaders;
            defaultHeaders.UserAgent.Clear(); defaultHeaders.UserAgent.ParseAdd(UserAgent);
            defaultHeaders.Referrer = BaseURI;
        }

        public void Run(Params parameters)
        {
            _logger.Info("History data fetcher started.");
            _svc.Run(SvcThreadName);
            _svc.Post(() => Init(parameters));
        }

        private void Init(Params parameters)
        {
            QueueBatches(parameters);
            CreateFiles(parameters);
            _svc.Post(Start);
        }

        private void QueueBatches(Params parameters)
        {
            var utcToday = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Unspecified);
            foreach (var symbol in parameters.Symbols)
                foreach (var timeframe in parameters.Timeframes)
                {
                    var timeStep = TimeSpan.FromMinutes((uint) timeframe);
                    (uint Min, uint Max) chunkRange = timeframe switch
                    {
                        VO.Timeframe.M1 => (120, 240),
                        VO.Timeframe.M2 => (60, 120),
                        VO.Timeframe.M5 => (144, 288),
                        VO.Timeframe.M15 => (96, 192),
                        VO.Timeframe.M30 => (24, 48),
                        VO.Timeframe.H1 => (48, 120),
                        _ => throw new ApplicationException($"Unexpected timeframe: {timeframe}")
                    };
                    var dateTill = parameters.DateRange.Till <= utcToday ? parameters.DateRange.Till : utcToday;
                    for (var timeFrom = parameters.DateRange.From ; timeFrom < dateTill ; )
                    {
                        DateTime timeTo = timeFrom + Rand.NextUInt32(chunkRange.Min, chunkRange.Max) * timeStep;
                        if (dateTill <= timeTo) timeTo = dateTill - timeStep;
                        if (timeTo.Year != timeFrom.Year) timeTo = new DateTime(timeFrom.Year + 1, 1, 1) - timeStep;
                        uint count = (uint) (timeTo - timeFrom + timeStep).TotalMinutes / (uint) timeframe;
                        var batch = new BatchDesc(symbol, timeframe, timeFrom, timeTo, timeStep, count);
                        _batches.Enqueue(batch);
                        timeFrom = timeTo + timeStep;
                    }
                }
            _initialBatchesCount = _batches.Count;
        }

        void CreateFiles(Params parameters)
        {
            Directory.CreateDirectory(_outputPath);
            foreach (var symbol in parameters.Symbols)
            {
                var yearFrom = parameters.DateRange.From.Year;
                var yearTo = parameters.DateRange.Till.Year - (1 < parameters.DateRange.Till.Month ? 0 : 1);
                for (var year = yearFrom ; year <= yearTo ; year ++)
                    foreach (var timeframe in parameters.Timeframes)
                    {
                        var fileName = $"{symbol}_{timeframe}_{year:D04}.dat";
                        var filePath = Path.Combine(_outputPath, fileName);
                        var fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                        var dataFile = new DataFile(fs, symbol, timeframe, year);
                        dataFile.Init();
                        _symbolsFiles[(symbol, timeframe, year)] = dataFile;
                    }
            }
        }

        private void CloseAllFiles()
        { 
            foreach (var df in _symbolsFiles.Values) df.Close();
        }

        public void Shutdown()
        {
            _logger.Info("Shutting down. Please wait...");
            void doShutdown()
            {
                _svc.Stop();
                _svcCancelSource.Cancel();
                CloseAllFiles();
            }
            _svc.Post(doShutdown);
        }

        private void Start()
        {
            void onLogin(Task<bool> task)
            {
                if (task.IsCompletedSuccessfully && task.Result)
                {
                    _ready = true;
                    _svc.Post(() => RequestIndicatorsData(DispatchIndicatorData));
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            _logger.Error($"Exception @01 <{innerEx.GetType().FullName}> {innerEx.Message}");
                }
            }
            ProcLogin().ContinueWithPost(_svc, onLogin);
        }

        private async Task<bool> ProcLogin()
        {
            var rspLoginPage = await _httpClient.GetAsync(LoginPageURI);
            if (rspLoginPage.IsSuccessStatusCode)
            {
                var authContent = new StringContent(LoginJSON.ToString(Formatting.None), Encoding.UTF8, "application/json");
                var rspAuth = await _httpClient.PostAsync(ApiAuthURI, authContent);
                if (rspAuth.IsSuccessStatusCode)
                {
                    var content = await rspAuth.Content.ReadAsStringAsync();
                    var authInfoJSON = JObject.Parse(content);
                    _authInfo = new(Id: authInfoJSON.Value<string>("id"), AccessToken: authInfoJSON.Value<string>("accessToken"));
                    var rspRootPage = await _httpClient.GetAsync(RootPageURI);
                    if (rspRootPage.IsSuccessStatusCode)
                        return true;
                    else
                    {
                        _logger.Error($"Server responce: {(int) rspRootPage.StatusCode} {rspRootPage.ReasonPhrase}");
                        return false;
                    }
                }
                else
                {
                    _logger.Error($"Server responce: {(int) rspAuth.StatusCode} {rspAuth.ReasonPhrase}");
                    return false;
                }
            }
            else
            {
                _logger.Error($"Server responce: {(int) rspLoginPage.StatusCode} {rspLoginPage.ReasonPhrase}");
                return false;
            }
        }

        void DispatchIndicatorData(IndicatorDataCtx ctx, JObject dataJSON)
        {
            var indCustom4 = dataJSON.ToObject<VO.IndCustom4Data>(JsonSerializer);
            if (indCustom4 == null)
            {
                _logger.Warn("Received invalid/unexpected JSON format.");
                return;
            }
            var entries = new DataFile.Entry[ctx.Batch.Count];
            DataFile.Entry createEntry(VO.IndCustom4Data.Values v)
            {
                if (v.VA.HasValue && v.Median.HasValue)
                {
                    var vaDir =
                        v.RedVA.HasValue && !v.GreenVA.HasValue ?
                            DataFile.Dir.Down :
                            v.GreenVA.HasValue && !v.RedVA.HasValue ?
                                DataFile.Dir.Up
                        : DataFile.Dir.None;
                    var va = decimal.ToDouble(decimal.Round(v.VA.Value, ctx.RoundDigits, MidpointRounding.AwayFromZero));
                    var median = decimal.ToDouble(decimal.Round(v.Median.Value, ctx.RoundDigits, MidpointRounding.AwayFromZero));
                    return new DataFile.Entry(vaDir, va, median);
                }
                return new DataFile.Entry();
            }
            var entriesLen = entries.Length;
            var dataLen = indCustom4.Data.Length;
            var timeStep = ctx.Batch.TimeStep;
            var time = ctx.Batch.TimeFrom;
            int idx = 0;
            for ( ; idx < dataLen && indCustom4.Data[idx].Time < time ; idx ++) ;
            for (int i = 0 ; i < entriesLen && idx < dataLen ; i ++, time += timeStep)
            { 
                var values = indCustom4.Data[idx];
                if (time == values.Time)
                {
                    entries[i] = createEntry(values);
                    idx ++;
                }
            }
            if (_symbolsFiles.TryGetValue((ctx.Batch.Symbol, ctx.Batch.Timeframe, ctx.Batch.TimeFrom.Year), out var dataFile))
                dataFile.WriteEntries(ctx.Batch.TimeFrom, entries);
            else
                throw new ApplicationException($"Data file for {ctx.Batch.Symbol}/{ctx.Batch.Timeframe}({ctx.Batch.TimeFrom.Year}) not found.");
            ReportProgress();
            if (_batches.Count == 0) OnDone?.Invoke();
        }

        private void RequestIndicatorsData(IndicatorDataDispatcher dispatcher)
        {
            void onSendAsync(Task<HttpResponseMessage> task, IndicatorDataCtx ctx)
            {
                if (task.IsCompletedSuccessfully)
                {
                    var rsp = task.Result;
                    if (rsp.IsSuccessStatusCode)
                        rsp.Content.ReadAsStringAsync().ContinueWithPost(_svc, (task) => onReadAsStringAsync(task, ctx));
                    else
                    {
                        _logger.Error($"Server response: {(int) rsp.StatusCode} {rsp.ReasonPhrase}");
                        _ready = false;
                    }
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            _logger.Error($"Exception @08 <{innerEx.GetType().FullName}> {innerEx.Message}");
                    _ready = false;
                }
                if (!_ready) _svc.Post(Start);
            }
            void onReadAsStringAsync(Task<string> task, IndicatorDataCtx ctx)
            {
                if (task.IsCompletedSuccessfully)
                {
                    var content = task.Result;
                    var dataJSON = JObject.Parse(content);
                    dispatcher.Invoke(ctx, dataJSON);
                    _svc.Post(() => RequestIndicatorsData(dispatcher));
                }
                else if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                        foreach (var innerEx in ex.InnerExceptions)
                            _logger.Error($"Exception @09 <{innerEx.GetType().FullName}> {innerEx.Message}");
                    _ready = false;
                    if (!_ready) _svc.Post(Start);
                }
            }
            if (_ready && _authInfo != null && _batches.TryDequeue(out var batch))
            {
                if (_symbolsDigits.TryGetValue(batch.Symbol, out var digits))
                    foreach (var indicator in IndicatorNames)
                    {
                        int roundDigits = digits + ExtraIndicatorDigits;
                        var ctx = new IndicatorDataCtx(indicator, batch, roundDigits);
                        var timeFrom = batch.TimeFrom - batch.TimeStep;
                        var timeTo = batch.TimeTo + batch.TimeStep;
                        var uri = IndicatorXhrUriFactory(indicator, Provider.ToString(), batch.Symbol, batch.Timeframe, timeFrom, timeTo);
                        var req = new HttpRequestMessage(HttpMethod.Get, uri);
                        req.Headers.Authorization = new("Token", _authInfo.AccessToken);
                        _httpClient.SendAsync(req).ContinueWithPost(_svc, (task) => onSendAsync(task, ctx));
                    }
            }
        }

        void ReportProgress()
        {
            decimal value = _initialBatchesCount != 0 ? (_initialBatchesCount - _batches.Count) / _initialBatchesCount : 0M;
            OnProgress?.Invoke(value);
        }
    }
}
