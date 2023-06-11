//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * ExpertAdvisor Abstract Class *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2020-2021, Geoffrey McGowan"
#property version   "2.00"

#include "MTS\\Lib\\ArrayList.i.mqh"
#include "MTS\\Market\\SymbolInfo.i.mqh"
#include "MTS\\ExpertAdvisor.i.mqh"

#property strict


enum Timeframe
{
    TF_Current = 0,     // Current
    TF_M1      = 1,     // M1
    TF_M2      = 2,     // M2
    TF_M5      = 5,     // M5
    TF_M15     = 15,    // M15
    TF_M30     = 30,    // M30
    TF_H1      = 60,    // H1
};

enum TimeOfDay15M
{
    TOD_00_00 = 0,    // 00:00
    TOD_00_15 = 15,   // 00:15
    TOD_00_30 = 30,   // 00:30
    TOD_00_45 = 45,   // 00:45
    TOD_01_00 = 60,   // 01:00
    TOD_01_15 = 75,   // 01:15
    TOD_01_30 = 90,   // 01:30
    TOD_01_45 = 105,  // 01:45
    TOD_02_00 = 120,  // 02:00
    TOD_02_15 = 135,  // 02:15
    TOD_02_30 = 150,  // 02:30
    TOD_02_45 = 165,  // 02:45
    TOD_03_00 = 180,  // 03:00
    TOD_03_15 = 195,  // 03:15
    TOD_03_30 = 210,  // 03:30
    TOD_03_45 = 225,  // 03:45
    TOD_04_00 = 240,  // 04:00
    TOD_04_15 = 255,  // 04:15
    TOD_04_30 = 270,  // 04:30
    TOD_04_45 = 285,  // 04:45
    TOD_05_00 = 300,  // 05:00
    TOD_05_15 = 315,  // 05:15
    TOD_05_30 = 330,  // 05:30
    TOD_05_45 = 345,  // 05:45
    TOD_06_00 = 360,  // 06:00
    TOD_06_15 = 375,  // 06:15
    TOD_06_30 = 390,  // 06:30
    TOD_06_45 = 405,  // 06:45
    TOD_07_00 = 420,  // 07:00
    TOD_07_15 = 435,  // 07:15
    TOD_07_30 = 450,  // 07:30
    TOD_07_45 = 465,  // 07:45
    TOD_08_00 = 480,  // 08:00
    TOD_08_15 = 495,  // 08:15
    TOD_08_30 = 510,  // 08:30
    TOD_08_45 = 525,  // 08:45
    TOD_09_00 = 540,  // 09:00
    TOD_09_15 = 555,  // 09:15
    TOD_09_30 = 570,  // 09:30
    TOD_09_45 = 585,  // 09:45
    TOD_10_00 = 600,  // 10:00
    TOD_10_15 = 615,  // 10:15
    TOD_10_30 = 630,  // 10:30
    TOD_10_45 = 645,  // 10:45
    TOD_11_00 = 660,  // 11:00
    TOD_11_15 = 675,  // 11:15
    TOD_11_30 = 690,  // 11:30
    TOD_11_45 = 705,  // 11:45
    TOD_12_00 = 720,  // 12:00
    TOD_12_15 = 735,  // 12:15
    TOD_12_30 = 750,  // 12:30
    TOD_12_45 = 765,  // 12:45
    TOD_13_00 = 780,  // 13:00
    TOD_13_15 = 795,  // 13:15
    TOD_13_30 = 810,  // 13:30
    TOD_13_45 = 825,  // 13:45
    TOD_14_00 = 840,  // 14:00
    TOD_14_15 = 855,  // 14:15
    TOD_14_30 = 870,  // 14:30
    TOD_14_45 = 885,  // 14:45
    TOD_15_00 = 900,  // 15:00
    TOD_15_15 = 915,  // 15:15
    TOD_15_30 = 930,  // 15:30
    TOD_15_45 = 945,  // 15:45
    TOD_16_00 = 960,  // 16:00
    TOD_16_15 = 975,  // 16:15
    TOD_16_30 = 990,  // 16:30
    TOD_16_45 = 1005, // 16:45
    TOD_17_00 = 1020, // 17:00
    TOD_17_15 = 1035, // 17:15
    TOD_17_30 = 1050, // 17:30
    TOD_17_45 = 1065, // 17:45
    TOD_18_00 = 1080, // 18:00
    TOD_18_15 = 1095, // 18:15
    TOD_18_30 = 1110, // 18:30
    TOD_18_45 = 1125, // 18:45
    TOD_19_00 = 1140, // 19:00
    TOD_19_15 = 1155, // 19:15
    TOD_19_30 = 1170, // 19:30
    TOD_19_45 = 1185, // 19:45
    TOD_20_00 = 1200, // 20:00
    TOD_20_15 = 1215, // 20:15
    TOD_20_30 = 1230, // 20:30
    TOD_20_45 = 1245, // 20:45
    TOD_21_00 = 1260, // 21:00
    TOD_21_15 = 1275, // 21:15
    TOD_21_30 = 1290, // 21:30
    TOD_21_45 = 1305, // 21:45
    TOD_22_00 = 1320, // 22:00
    TOD_22_15 = 1335, // 22:15
    TOD_22_30 = 1350, // 22:30
    TOD_22_45 = 1365, // 22:45
    TOD_23_00 = 1380, // 23:00
    TOD_23_15 = 1395, // 23:15
    TOD_23_30 = 1410, // 23:30
    TOD_23_45 = 1425  // 23:45
};

struct DataParams
{
    struct TradeSetup
    {
        uint TrendPeriod;
    };

    Timeframe MainTimeframe;
    Timeframe TrendTimeframe;
    TradeSetup WithTrend;
    TradeSetup AgainstTrend;
};

enum Dir { DIR_NONE = 0, DIR_UP = +1, DIR_DOWN = -1 };

struct MarketData
{
    enum WhatEnum
    {
        NONE = 0,
        TICK = 0x01,
        SIGNAL = 0x02
    };
    
    struct TickRec
    {
        datetime Time;
        double Price;
    };
    
    struct PriceAndDir
    {
        double Price;
        Dir Dir;
    };

    struct SignalRec
    {
        struct Median
        {
            PriceAndDir M_0;
            PriceAndDir M_1;
        };
    
        datetime Time;
        PriceAndDir VA_0;
        PriceAndDir VA_1;
        PriceAndDir VA_2;
        Median MedianW;
        Median MedianA;
        double ATR;
    };

    uint What;
    TickRec Tick;
    SignalRec Signal;
};

#ifdef _TEST_MODE_
    bool MT5C_Init(const string &, const string &, const DataParams &, int) { return false; }
    bool MT5C_QueryData(bool &, MarketData &) { return false; }
    bool MT5C_Shutdown() { return false; }
    bool MT5C_GetLastMessage(int &, ushort &[], uint) { return false; }
#else
    #import "mt5bridge_c.dll"
        bool MT5C_Init(const string & serviceURI, const string & symbol, const DataParams & params, int utcShift);
        bool MT5C_QueryData(bool & haveData, MarketData & data);
        bool MT5C_Shutdown();
        bool MT5C_GetLastMessage(int & errNo, ushort & buffer[], uint size);
    #import
#endif 

const string TestDataFolderRelPath = "GMForexEA";
const uint DisplayNumberLimit = 100000;

class InputParams
{
public:
    struct TradeSetupRec
    {
        double TakeProfitATRFactor;
        double StopLossATRFactor;
        bool UseTrailingStop;
        bool CloseIfReversed;
        uint TrendPeriod;
    };

public:
    string ServiceURI;
    ulong Magic;
    uint QueryInterval;
    string CCSymbol;
    double Lot;
    ulong Slippage;
    uint MaxSpread;
    Timeframe TrendTimeframe;
    bool TradeWithTrend;
    TradeSetupRec WithTrend;
    bool TradeAgainstTrend;
    TradeSetupRec AgainstTrend;
    int DayTradingStart;
    int DayTradingStop;
    bool IsForTester;
};

class TesterParams
{
public:
    int UTCShift;
    datetime DSTBegin;
    datetime DSTEnd;
};

//+------------------------------------------------------------------------------------------------------------------------------------------------------------+

class GMExpertAdvisor : public ExpertAdvisor<InputParams>
{
public:
    class PositionDesc
    {
    public:
        ulong Id;
        double SlOffset;
        bool Visited;
    public:
        PositionDesc(ulong id, double slOffset) { Id = id; SlOffset = slOffset; Visited = false; }
    };

public:
    GMExpertAdvisor(string name, uchar versionSerial);
    bool Setup(const InputParams & inputParams, const TesterParams & testerParams, bool isTesting);
    bool OnInit();
    void OnTick();
    void OnTimer();
    void OnTestTick();

protected:
    void OnExitAny();

private:
    struct TestData
    {
        struct FileHeader
        {
            char Magic[8];
            uint Timeframe;
            datetime YearTimestamp;
            long EntryCount;
        };

        struct Entry
        { 
            uchar Tag;
            char Dir;
            double VA;
            double Median;
        };

        int DataFile;
        datetime TimeFrom;
        datetime TimeTill;
        datetime TimeLast;
        Entry Entries[];
    };

    struct PointCoords
    {
        datetime Time;
        double Price;
    };

private:
    bool CheckAndSetParams();
    int GetUtcShift();
    datetime GetUtcTime(datetime serverTime);
    bool IsInTradingInterval(datetime time);
    ulong OpenPosition(bool isWithTrend);
    void SetTestUtcShift(datetime time);
    string GetLastMessageString();
    bool EnsureTestData(Timeframe timeframe, datetime time, TestData & tdd);
    bool FillTestMarketData(const TickInfo & tick, MarketData & data);
    Dir GetTestMedianDir(uint period, uint offset);
    string GetTimeframeName(Timeframe timeframe);
    void CreateTextDisplay();
    void UpdateTextDisplay(uint spreadPts, uint atrPts);
    void EraseTextDisplay();
    void DrawLines();
    void DrawVaLine(string namePrefix, datetime time, double price, Dir dir);
    void DrawMedianLine(string namePrefix, datetime time, double price);

private:
    uchar _version;
    int _utcShift;
    SymbolInfo _si;
    bool _haveTick;
    MarketData::TickRec _lastTick;
    bool _haveSignal;
    MarketData::SignalRec _lastSignal;
    bool _isNewCandle;
    int _atr;
    double _atrData[];
    Timeframe _mainTimeframe;
    Timeframe _trendTimeframe;
    ulong _trendChartId;
    PointCoords _lastVaPoint;
    PointCoords _lastMedianPoint;
    const TesterParams * _testerParams;
    TestData _tddMain;
    TestData _tddTrend;
    ArrayList<PositionDesc *> _positions;
};

GMExpertAdvisor::GMExpertAdvisor(string name, uchar versionSerial)
: ExpertAdvisor(name), _version(versionSerial), _si(Symbol()),
  _haveTick(false), _haveSignal(false), _isNewCandle(false), _atr(0), _trendChartId(0)
{
    _lastVaPoint.Time = 0; _lastVaPoint.Price = 0;
    _lastMedianPoint.Time = 0; _lastMedianPoint.Price = 0;
    _tddMain.DataFile = INVALID_HANDLE; _tddMain.TimeFrom = 0; _tddMain.TimeTill = 0; _tddMain.TimeLast = 0;
    _tddTrend.DataFile = INVALID_HANDLE; _tddTrend.TimeFrom = 0; _tddTrend.TimeTill = 0; _tddTrend.TimeLast = 0;
}

bool GMExpertAdvisor::Setup(const InputParams & inputParams, const TesterParams & testerParams, bool isTesting)
{
    if (ExpertAdvisor<InputParams>::Setup(inputParams, isTesting))
    {
        _testerParams = &testerParams;
        return true;
    }
    return false;
}

bool GMExpertAdvisor::OnInit()
{
    CreateTextDisplay();
    if (_inputParams.IsForTester)
    {
        if (_isTesting)
        {
            if (CheckAndSetParams())
            {
                _trendChartId = _trendTimeframe != _mainTimeframe ? ChartOpen(_si.Name(), _trendTimeframe) : 0;
                _atr = iATR(_si.Name(), Period(), 14);
                ArrayResize(_atrData, 1);
                return true;
            }
        }
        else
            _logger.Error("Please run this version of the EA in MT5 Tester ONLY.");
    }
    else
    {
        if (!_isTesting)
        {
            _utcShift = GetUtcShift();
            DataParams dataParams;
            if (CheckAndSetParams())
            {
                dataParams.MainTimeframe = _mainTimeframe;
                dataParams.TrendTimeframe = _trendTimeframe;
                dataParams.WithTrend.TrendPeriod = _inputParams.TradeWithTrend ? _inputParams.WithTrend.TrendPeriod : 0;
                dataParams.AgainstTrend.TrendPeriod = _inputParams.TradeAgainstTrend ? _inputParams.AgainstTrend.TrendPeriod : 0;
                string serviceURI = _inputParams.ServiceURI; string symbol = _inputParams.CCSymbol;
                if (MT5C_Init(serviceURI, symbol, dataParams, _utcShift))
                {
                    _trendChartId = dataParams.TrendTimeframe != dataParams.MainTimeframe ? ChartOpen(_si.Name(), dataParams.TrendTimeframe) : 0;
                    _atr = iATR(_si.Name(), Period(), 14);
                    ArrayResize(_atrData, 1);
                    return true;
                }
                else
                {
                    _logger.Error(GetLastMessageString());
                    return false;
                }
            }
        }
        else
            _logger.Error("This version of the EA CANNOT be run in MT5 Tester.");
    }    
    return false;
}

void GMExpertAdvisor::OnTick()
{
    if (_positions.Count() == 0) return;
    TickInfo tick = _si.GetLastTickInfo();
    for (int pi = 0, count = PositionsTotal() ; pi < count ; pi ++)
    {
        ulong ticket = PositionGetTicket(pi);
        if (ticket != 0 && PositionGetInteger(POSITION_MAGIC) == _inputParams.Magic && PositionGetString(POSITION_SYMBOL) == _si.Name())
        {
            ulong positionId = PositionGetInteger(POSITION_IDENTIFIER);
            for (_positions.Begin() ; _positions.Next() ; )
            {
                PositionDesc * pd = _positions.Get();
                if (pd.Id == positionId)
                {
                    pd.Visited = true;
                    ENUM_POSITION_TYPE side = (ENUM_POSITION_TYPE) PositionGetInteger(POSITION_TYPE);
                    double price = side == POSITION_TYPE_BUY ? tick.Bid : tick.Ask;
                    double stopLoss = PositionGetDouble(POSITION_SL);
                    double takeProfit = PositionGetDouble(POSITION_TP);
                    double trailingSL = NormalizeDouble(price + pd.SlOffset, _si.Digits());
                    if (side == POSITION_TYPE_BUY ? stopLoss < trailingSL : trailingSL < stopLoss)
                    {
                        MqlTradeRequest request;
                        MqlTradeResult result;
                        request.action = TRADE_ACTION_SLTP;
                        request.magic = _inputParams.Magic;
                        request.order = 0;
                        request.symbol = _si.Name();
                        request.volume = 0;
                        request.price = 0;
                        request.stoplimit = 0;
                        request.sl = trailingSL;
                        request.tp = takeProfit;
                        request.deviation = 0;
                        request.type = side == POSITION_TYPE_BUY ? ORDER_TYPE_BUY : ORDER_TYPE_SELL;
                        request.type_filling = ORDER_FILLING_IOC;
                        request.type_time = ORDER_TIME_GTC;
                        request.expiration = 0;
                        request.comment = "";
                        request.position = ticket;
                        request.position_by = 0;
                        bool success = OrderSend(request, result);
                        if (success)
                        {
                            string fmt = StringFormat("TrailingSL: retcode = %%u, position #%%I64u; SL %%.%df -> %%.%df ", _si.Digits(), _si.Digits());
                            //_logger.Debug(StringFormat(fmt, result.retcode, ticket, stopLoss, trailingSL));
                        }
                        else
                            _logger.Error(StringFormat("TrailingSL: Error #%d", GetLastError()));
                    }
                }
            }
        }
    }
    for (_positions.Begin() ; _positions.Next() ; )
    {
        PositionDesc * pd = _positions.Get();
        if (pd.Visited) pd.Visited = false; else _positions.Remove();
    }
    _positions.Compact();
}

void GMExpertAdvisor::OnTimer()
{
    TickInfo tick = _si.GetLastTickInfo();
    int atrDataCount = CopyBuffer(_atr, 0, 0, 1, _atrData);
    if (atrDataCount < 1)
    {
        _logger.Error("Failed to get ATR data");
        return;
    }
    uint spreadPts = uint(MathRound((tick.Ask - tick.Bid) / _si.Point()));
    uint atrPts = uint(MathRound(_atrData[0] / _si.Point()));
    UpdateTextDisplay(spreadPts, atrPts);
    MarketData data; bool haveData;
    do
    {
        bool dataRet = MT5C_QueryData(haveData, data);
        if (dataRet)
        {
            if (haveData)
            {
                switch (data.What)
                {
                    case MarketData::TICK:
                    {
                        _lastTick = data.Tick;
                        _haveTick = true;
                        break;
                    }
                    case MarketData::SIGNAL:
                    {
                        if (_haveSignal && data.Signal.Time != _lastSignal.Time) _isNewCandle = true;
                        _lastSignal = data.Signal;
                        string timeTxt = TimeToString(_lastSignal.Time, TIME_DATE|TIME_MINUTES|TIME_SECONDS);
                        _logger.Debug
                        (
                            StringFormat
                            (
                                "Time = %s; VA = (%d %d %d); Median = (%d %d) / (%d %d)",
                                timeTxt,
                                _lastSignal.VA_0.Dir, _lastSignal.VA_1.Dir, _lastSignal.VA_2.Dir,
                                _lastSignal.MedianW.M_0.Dir, _lastSignal.MedianW.M_1.Dir, _lastSignal.MedianA.M_0.Dir, _lastSignal.MedianA.M_1.Dir
                            )
                        );
                        _haveSignal = true;
                        break;
                    }
                    default:
                        _logger.Warning("Unknown data type.");
                }
            }
        }
        else
            _logger.Error(GetLastMessageString());
    }
    while (haveData);
    if (_haveSignal && _isNewCandle)
    {
        DrawLines();
        if (_isNewCandle)
        {
            _isNewCandle = false;
            if (IsInTradingInterval(tick.Time) && (_inputParams.MaxSpread == 0 || spreadPts <= _inputParams.MaxSpread))
            {
                if (_inputParams.TradeWithTrend) OpenPosition(true);
                if (_inputParams.TradeAgainstTrend) OpenPosition(false);
            }
        }
    }
}

void GMExpertAdvisor::OnTestTick()
{
    TickInfo tick = _si.GetLastTickInfo();
    SetTestUtcShift(tick.Time);
    int atrDataCount = CopyBuffer(_atr, 0, 0, 1, _atrData);
    if (atrDataCount < 1)
    {
        _logger.Error("Failed to get ATR data");
        return;
    }
    uint spreadPts = uint(MathRound((tick.Ask - tick.Bid) / _si.Point()));
    uint atrPts = uint(MathRound(_atrData[0] / _si.Point()));
    UpdateTextDisplay(spreadPts, atrPts);
    if (EnsureTestData(_mainTimeframe, tick.Time, _tddMain) && EnsureTestData(_trendTimeframe, tick.Time, _tddTrend))
    {
        MarketData data;
        _haveSignal = FillTestMarketData(tick, data);
        if (_haveSignal && data.Signal.Time != _lastSignal.Time) _isNewCandle = true;
        _lastSignal = data.Signal;
        if (_haveSignal && _isNewCandle)
        {
            DrawLines();
            if (_isNewCandle)
            {
                _isNewCandle = false;
                if (IsInTradingInterval(tick.Time) && (_inputParams.MaxSpread == 0 || spreadPts <= _inputParams.MaxSpread))
                {
                    if (_inputParams.TradeWithTrend) OpenPosition(true);
                    if (_inputParams.TradeAgainstTrend) OpenPosition(false);
                }
            }
        }
    }
    OnTick();
}

bool GMExpertAdvisor::CheckAndSetParams()
{
    uint mtf = PeriodSeconds() / 60;
    uint ttf = _inputParams.TrendTimeframe != TF_Current ? _inputParams.TrendTimeframe : mtf;
    if (mtf != TF_M1 && mtf != TF_M2 && mtf != TF_M5 && mtf != TF_M15 && mtf != TF_M30 && mtf != TF_H1)
    {
        _logger.Error("Invalid 'Main timeframe' parameter.");
        return false;
    }
    if (ttf != TF_M1 && ttf != TF_M2 && ttf != TF_M5 && ttf != TF_M15 && ttf != TF_M30 && ttf != TF_H1)
    {
        _logger.Error("Invalid 'Trend timeframe' parameter.");
        return false;
    }
    if (ttf < mtf)
    {
        _logger.Error("'Trend timeframe' must not be below 'Main timeframe'.");
        return false;
    }
    _mainTimeframe = (Timeframe) mtf;
    _trendTimeframe = (Timeframe) ttf;
    if (!_inputParams.WithTrend.UseTrailingStop && _inputParams.WithTrend.TakeProfitATRFactor == 0)
    {
        _logger.Error("Please either choose 'Use trailing stop' or set 'Take profit ATR factor' to a non-zero value in 'TRADING WITH TREND' section.");
        return false;
    }
    if (!_inputParams.AgainstTrend.UseTrailingStop && _inputParams.AgainstTrend.TakeProfitATRFactor == 0)
    {
        _logger.Error("Please either choose 'Use trailing stop' or set 'Take profit ATR factor' to a non-zero value in 'TRADING AGAINST TREND' section.");
        return false;
    }
    return true;
}

int GMExpertAdvisor::GetUtcShift()
{
    long utcTime = long(TimeGMT());
    long serverTime = long(TimeTradeServer());
    int utcShift = int(((serverTime - utcTime) / (15 * 60)) * 15 * 60);
    return utcShift;
}

datetime GMExpertAdvisor::GetUtcTime(datetime serverTime)
{
    return datetime(long(serverTime) - _utcShift);
}

bool GMExpertAdvisor::IsInTradingInterval(datetime time)
{
    MqlDateTime dt; TimeToStruct(time, dt);
    int timeOfDay = 3600 * dt.hour + 60 * dt.min + dt.sec;
    int fromTime = _inputParams.DayTradingStart;
    int tillTime = _inputParams.DayTradingStop;
    return
        fromTime < tillTime ? fromTime <= timeOfDay && timeOfDay < tillTime :
        tillTime < fromTime ? timeOfDay < tillTime || fromTime <= timeOfDay :
        true;
}

ulong GMExpertAdvisor::OpenPosition(bool isWithTrend)
{
    ulong order = 0;
    Dir trendDir = isWithTrend ? _lastSignal.MedianW.M_1.Dir : _lastSignal.MedianA.M_1.Dir;
    Dir vaDir = (_lastSignal.VA_1.Dir != _lastSignal.VA_2.Dir ? _lastSignal.VA_1.Dir : DIR_NONE);
    if (trendDir != DIR_NONE && vaDir != DIR_NONE)
    {
        if ((isWithTrend && vaDir == trendDir) || (!isWithTrend && vaDir != trendDir))
        {
            TickInfo tick = _si.GetLastTickInfo();
            ENUM_ORDER_TYPE op = (trendDir == DIR_UP ? (isWithTrend ? ORDER_TYPE_BUY : ORDER_TYPE_SELL) : (isWithTrend ? ORDER_TYPE_SELL : ORDER_TYPE_BUY));
            double slATRFactor = isWithTrend ? _inputParams.WithTrend.StopLossATRFactor : _inputParams.AgainstTrend.StopLossATRFactor;
            double tpATRFactor = isWithTrend ? _inputParams.WithTrend.TakeProfitATRFactor : _inputParams.AgainstTrend.TakeProfitATRFactor;
            double price = op == ORDER_TYPE_BUY ? tick.Ask : tick.Bid;
            double slOffset = NormalizeDouble(slATRFactor * (op == ORDER_TYPE_BUY ? - _atrData[0] : + _atrData[0]), _si.Digits());
            double tpOffset = tpATRFactor != 0 ? NormalizeDouble(tpATRFactor * (op == ORDER_TYPE_BUY ? + _atrData[0] : - _atrData[0]), _si.Digits()) : 0;
            double stopLoss = NormalizeDouble(price + slOffset, _si.Digits());
            double takeProfit = tpOffset != 0 ? NormalizeDouble(price + tpOffset, _si.Digits()) : 0;
            MqlTradeRequest request;
            MqlTradeResult result;
            request.action = TRADE_ACTION_DEAL;
            request.magic = _inputParams.Magic;
            request.order = 0;
            request.symbol = _si.Name();
            request.volume = _inputParams.Lot;
            request.price = price;
            request.stoplimit = 0;
            request.sl = stopLoss;
            request.tp = takeProfit;
            request.deviation = _inputParams.Slippage;
            request.type = op;
            request.type_filling = ORDER_FILLING_IOC;
            request.type_time = ORDER_TIME_GTC;
            request.expiration = 0;
            request.comment = StringFormat("ID=%I64u", _inputParams.Magic);
            request.position = 0;
            request.position_by = 0;
            bool success = OrderSend(request, result);
            if (success)
            {
                bool useTrailingStop = isWithTrend ? _inputParams.WithTrend.UseTrailingStop : _inputParams.AgainstTrend.UseTrailingStop;
                if (useTrailingStop)
                {
                    if (HistoryDealSelect(result.deal))
                    {
                        PositionDesc * pd = new PositionDesc(HistoryDealGetInteger(result.deal, DEAL_POSITION_ID), slOffset);
                        _positions.Push(pd);
                    }
                }
                //_logger.Debug(StringFormat("OpenPosition: retcode = %u, deal #%I64u, order #%I64u", result.retcode, result.deal, result.order));
            }
            else
                _logger.Error(StringFormat("OpenPosition: Error #%d", GetLastError()));
        }
    }
    return order;
}

void GMExpertAdvisor::OnExitAny()
{
    IndicatorRelease(_atr);
    ArrayFree(_atrData);
    if (_isTesting)
    {
        ArrayFree(_tddMain.Entries);
        ArrayFree(_tddTrend.Entries);
        if (_tddMain.DataFile != INVALID_HANDLE) FileClose(_tddMain.DataFile);
        if (_tddTrend.DataFile != INVALID_HANDLE) FileClose(_tddTrend.DataFile);
    }
    if (!_isTesting) MT5C_Shutdown();
    EraseTextDisplay();
}

void GMExpertAdvisor::SetTestUtcShift(datetime time)
{
    int dstShift = _testerParams.DSTBegin <= time && time < _testerParams.DSTEnd ? 3600 : 0;
    _utcShift = _testerParams.UTCShift + dstShift;
}

string GMExpertAdvisor::GetLastMessageString()
{
    int errNo; ushort msgBuffer[256];
    bool success = MT5C_GetLastMessage(errNo, msgBuffer, ArraySize(msgBuffer));
    string msgTxt = success ? (errNo != 0 ? StringFormat("#%d: ", errNo) : "") + ShortArrayToString(msgBuffer) : "Failed to obtain error message";
    return msgTxt;
}

bool GMExpertAdvisor::EnsureTestData(Timeframe timeframe, datetime time, TestData & tdd)
{
    datetime dataTime = GetUtcTime(time);
    if (tdd.TimeFrom != 0 && (tdd.TimeFrom <= dataTime && dataTime < tdd.TimeTill)) return true;
    MqlDateTime dt; TimeToStruct(dataTime, dt);
    string fileName = StringFormat("%s_%s_%04d.dat", _inputParams.CCSymbol, GetTimeframeName(timeframe), dt.year);
    string filePath = StringFormat("%s\\%s", TestDataFolderRelPath, fileName);
    if (tdd.DataFile != INVALID_HANDLE)
    {
        FileClose(tdd.DataFile);
        tdd.DataFile = INVALID_HANDLE;
    }
    int f = FileOpen(filePath, FILE_COMMON|FILE_BIN|FILE_READ|FILE_SHARE_READ);
    if (f != INVALID_HANDLE)
    {
        TestData::FileHeader header;
        FileReadStruct(f, header);
        ArrayResize(tdd.Entries, int(header.EntryCount));
        FileReadArray(f, tdd.Entries);
        tdd.DataFile = f;
        tdd.TimeFrom = header.YearTimestamp;
        tdd.TimeTill = datetime(tdd.TimeFrom + (60 * uint(timeframe) * header.EntryCount));
        tdd.TimeLast = 0;
        return true;
    }
    else
        _logger.Error(StringFormat("Failed to read test data file '%s' (%d)", fileName, GetLastError()));
    return false;
}

bool GMExpertAdvisor::FillTestMarketData(const TickInfo & tick, MarketData & data)
{
    datetime dataTime = GetUtcTime(tick.Time);
    bool haveSignal = false;
    data.What = MarketData::TICK;
    data.Tick.Time = tick.Time;
    data.Tick.Price = NormalizeDouble((tick.Bid + tick.Ask) / 2, _si.Digits());
    long offsetMain = (dataTime - _tddMain.TimeFrom) / (60 * _mainTimeframe);
    long offsetTrend = (dataTime - _tddTrend.TimeFrom) / (60 * _trendTimeframe);
    data.Signal.Time = datetime(long(tick.Time) - long(tick.Time) % (60 * _mainTimeframe));
    data.Signal.VA_0.Dir = DIR_NONE;
    data.Signal.VA_0.Price = 0;
    data.Signal.VA_1.Dir = DIR_NONE;
    data.Signal.VA_1.Price = 0;
    data.Signal.VA_2.Dir = DIR_NONE;
    data.Signal.VA_2.Price = 0;
    data.Signal.MedianW.M_0.Price = 0;
    data.Signal.MedianW.M_0.Dir = DIR_NONE;
    data.Signal.MedianW.M_1.Price = 0;
    data.Signal.MedianW.M_1.Dir = DIR_NONE;
    data.Signal.MedianA.M_0.Price = 0;
    data.Signal.MedianA.M_0.Dir = DIR_NONE;
    data.Signal.MedianA.M_1.Price = 0;
    data.Signal.MedianA.M_1.Dir = DIR_NONE;
    bool isVaFilled = false;
    if (2 <= offsetMain)
    {
        TestData::Entry entry0 = _tddMain.Entries[int(offsetMain)];
        TestData::Entry entry1 = _tddMain.Entries[int(offsetMain) - 1];
        TestData::Entry entry2 = _tddMain.Entries[int(offsetMain) - 2];
        if (entry0.Tag != 0 && entry1.Tag != 0 && entry2.Tag != 0)
        {
            data.Signal.VA_0.Dir = (Dir) entry0.Dir;
            data.Signal.VA_0.Price = entry0.VA;
            data.Signal.VA_1.Dir = (Dir) entry1.Dir;
            data.Signal.VA_1.Price = entry1.VA;
            data.Signal.VA_2.Dir = (Dir) entry2.Dir;
            data.Signal.VA_2.Price = entry2.VA;
            isVaFilled = true;
        }
    }
    if (isVaFilled)
    {
        bool isMedianFilled = false;
        uint requredW = _inputParams.TradeWithTrend ? _inputParams.WithTrend.TrendPeriod : 0;
        uint requredA = _inputParams.TradeAgainstTrend ? _inputParams.AgainstTrend.TrendPeriod : 0;
        if (MathMax(3, MathMax(requredW, requredA)) <= offsetTrend)
        {
            if (0 < requredW)
            {
                data.Signal.MedianW.M_0.Price = _tddTrend.Entries[int(offsetTrend)].Median;
                data.Signal.MedianW.M_0.Dir = GetTestMedianDir(requredW, uint(offsetTrend));
                data.Signal.MedianW.M_1.Price = _tddTrend.Entries[int(offsetTrend) - 1].Median;
                data.Signal.MedianW.M_1.Dir = GetTestMedianDir(requredW, uint(offsetTrend) - 1);
            }
            if (0 < requredA)
            {
                data.Signal.MedianA.M_0.Price = _tddTrend.Entries[int(offsetTrend)].Median;
                data.Signal.MedianA.M_0.Dir = GetTestMedianDir(requredA, uint(offsetTrend));
                data.Signal.MedianA.M_1.Price = _tddTrend.Entries[int(offsetTrend) - 1].Median;
                data.Signal.MedianA.M_1.Dir = GetTestMedianDir(requredA, uint(offsetTrend) - 1);
            }
            isMedianFilled = true;
        }
        if (isMedianFilled)
        {
            data.What |= MarketData::SIGNAL;
            haveSignal = true;
        }
    }
    return haveSignal;
}

Dir GMExpertAdvisor::GetTestMedianDir(uint period, uint offset)
{
    Dir dir = DIR_NONE;
    for (uint i = offset - period + 1 ; i <= offset ; i ++)
    {
        if (_tddTrend.Entries[i].Tag == 0) { dir = DIR_NONE; break; }
        if (_tddTrend.Entries[i].Median < _tddTrend.Entries[i - 1].Median)
        {
            if (dir == DIR_UP) { dir = DIR_NONE; break; }
            dir = DIR_DOWN;
        }
        else if (_tddTrend.Entries[i - 1].Median < _tddTrend.Entries[i].Median)
        {
            if (dir == DIR_DOWN) { dir = DIR_NONE; break; }
            dir = DIR_UP;
        }
    }
    return dir;
}

string GMExpertAdvisor::GetTimeframeName(Timeframe timeframe)
{
    string name;
    switch (timeframe)
    {
        case TF_M1: name = "M1"; break;
        case TF_M2: name = "M2"; break;
        case TF_M5: name = "M5"; break;
        case TF_M15: name = "M15"; break;
        case TF_M30: name = "M30"; break;
        case TF_H1: name = "H1"; break;
        default: name = "";
    }
    return name;
}

void GMExpertAdvisor::CreateTextDisplay()
{
    EraseTextDisplay();

    ObjectCreate(0, "GM_DisplayBG", OBJ_RECTANGLE_LABEL, 0, 0, 0);
    ObjectSetInteger(0, "GM_DisplayBG", OBJPROP_CORNER, CORNER_RIGHT_UPPER);
    ObjectSetInteger(0, "GM_DisplayBG", OBJPROP_XDISTANCE, 280);
    ObjectSetInteger(0, "GM_DisplayBG", OBJPROP_YDISTANCE, 0);
    ObjectSetInteger(0, "GM_DisplayBG", OBJPROP_XSIZE, 180);
    ObjectSetInteger(0, "GM_DisplayBG", OBJPROP_YSIZE, 72);
    ObjectSetInteger(0, "GM_DisplayBG", OBJPROP_BGCOLOR, clrLightBlue);
    ObjectSetInteger(0, "GM_DisplayBG", OBJPROP_BORDER_TYPE, BORDER_FLAT);
    ObjectSetInteger(0, "GM_DisplayBG", OBJPROP_WIDTH, 0);

    ObjectCreate(0, "GM_Spread_Title", OBJ_LABEL, 0, 0, 0);
    ObjectSetInteger(0, "GM_Spread_Title", OBJPROP_CORNER, CORNER_RIGHT_UPPER);
    ObjectSetInteger(0, "GM_Spread_Title", OBJPROP_ANCHOR, ANCHOR_RIGHT_LOWER);
    ObjectSetInteger(0, "GM_Spread_Title", OBJPROP_XDISTANCE, 180);
    ObjectSetInteger(0, "GM_Spread_Title", OBJPROP_YDISTANCE, 36);
    ObjectSetString(0, "GM_Spread_Title", OBJPROP_FONT, "Consolas");
    ObjectSetInteger(0, "GM_Spread_Title", OBJPROP_FONTSIZE, 12);
    ObjectSetInteger(0, "GM_Spread_Title", OBJPROP_COLOR, clrBlack);
    ObjectSetString(0, "GM_Spread_Title", OBJPROP_TEXT, "Spread:");

    ObjectCreate(0, "GM_ATR_Title", OBJ_LABEL, 0, 0, 0);
    ObjectSetInteger(0, "GM_ATR_Title", OBJPROP_CORNER, CORNER_RIGHT_UPPER);
    ObjectSetInteger(0, "GM_ATR_Title", OBJPROP_ANCHOR, ANCHOR_RIGHT_LOWER);
    ObjectSetInteger(0, "GM_ATR_Title", OBJPROP_XDISTANCE, 180);
    ObjectSetInteger(0, "GM_ATR_Title", OBJPROP_YDISTANCE, 60);
    ObjectSetString(0, "GM_ATR_Title", OBJPROP_FONT, "Consolas");
    ObjectSetInteger(0, "GM_ATR_Title", OBJPROP_FONTSIZE, 12);
    ObjectSetInteger(0, "GM_ATR_Title", OBJPROP_COLOR, clrBlack);
    ObjectSetString(0, "GM_ATR_Title", OBJPROP_TEXT, "ATR:");

    ObjectCreate(0, "GM_Spread_Value", OBJ_LABEL, 0, 0, 0);
    ObjectSetInteger(0, "GM_Spread_Value", OBJPROP_CORNER, CORNER_RIGHT_UPPER);
    ObjectSetInteger(0, "GM_Spread_Value", OBJPROP_ANCHOR, ANCHOR_RIGHT_LOWER);
    ObjectSetInteger(0, "GM_Spread_Value", OBJPROP_XDISTANCE, 108);
    ObjectSetInteger(0, "GM_Spread_Value", OBJPROP_YDISTANCE, 36);
    ObjectSetString(0, "GM_Spread_Value", OBJPROP_FONT, "Consolas");
    ObjectSetInteger(0, "GM_Spread_Value", OBJPROP_FONTSIZE, 12);
    ObjectSetInteger(0, "GM_Spread_Value", OBJPROP_COLOR, clrBlack);
    ObjectSetString(0, "GM_Spread_Value", OBJPROP_TEXT, "--/--");

    ObjectCreate(0, "GM_ATR_Value", OBJ_LABEL, 0, 0, 0);
    ObjectSetInteger(0, "GM_ATR_Value", OBJPROP_CORNER, CORNER_RIGHT_UPPER);
    ObjectSetInteger(0, "GM_ATR_Value", OBJPROP_ANCHOR, ANCHOR_RIGHT_LOWER);
    ObjectSetInteger(0, "GM_ATR_Value", OBJPROP_XDISTANCE, 108);
    ObjectSetInteger(0, "GM_ATR_Value", OBJPROP_YDISTANCE, 60);
    ObjectSetString(0, "GM_ATR_Value", OBJPROP_FONT, "Consolas");
    ObjectSetInteger(0, "GM_ATR_Value", OBJPROP_FONTSIZE, 12);
    ObjectSetInteger(0, "GM_ATR_Value", OBJPROP_COLOR, clrBlack);
    ObjectSetString(0, "GM_ATR_Value", OBJPROP_TEXT, "--/--");
}

void GMExpertAdvisor::UpdateTextDisplay(uint spreadPts, uint atrPts)
{
    ObjectSetString(0, "GM_Spread_Value", OBJPROP_TEXT, spreadPts < DisplayNumberLimit ? string(spreadPts) : "--/--");
    ObjectSetString(0, "GM_ATR_Value", OBJPROP_TEXT, atrPts < DisplayNumberLimit ? string(atrPts) : "--/--");
}

void GMExpertAdvisor::EraseTextDisplay()
{
    ObjectDelete(0, "GM_DisplayBG");
    ObjectDelete(0, "GM_Spread_Title");
    ObjectDelete(0, "GM_ATR_Title");
    ObjectDelete(0, "GM_Spread_Value");
    ObjectDelete(0, "GM_ATR_Value");
}

void GMExpertAdvisor::DrawLines()
{
    datetime time = _lastSignal.Time - PeriodSeconds();
    MqlDateTime dt; TimeToStruct(time, dt);
    string namePrefix = StringFormat("GM_%04d%02d%02d_%02d%02d", dt.year, dt.mon, dt.day, dt.hour, dt.min);
    DrawVaLine(namePrefix, time, _lastSignal.VA_1.Price, _lastSignal.VA_1.Dir);
    DrawMedianLine(namePrefix, time, _lastSignal.MedianW.M_1.Price);
}

void GMExpertAdvisor::DrawVaLine(string namePrefix, datetime time, double price, Dir dir)
{
    if (_lastVaPoint.Time != 0)
    {
        string name = StringFormat("%s_VA", namePrefix);
        color lnColor = dir == DIR_DOWN ? clrRed : dir == DIR_UP ? clrGreen : clrGray;
        ObjectDelete(0, name);
        ObjectCreate(0, name, OBJ_TREND, 0, _lastVaPoint.Time, _lastVaPoint.Price, time, price);
        ObjectSetInteger(0, name, OBJPROP_WIDTH, 6);
        ObjectSetInteger(0, name, OBJPROP_COLOR, lnColor);
    }
    _lastVaPoint.Time = time;
    _lastVaPoint.Price = price;
}

void GMExpertAdvisor::DrawMedianLine(string namePrefix, datetime time, double price)
{
    if (_lastMedianPoint.Time != 0)
    {
        string name = StringFormat("%s_M", namePrefix);
        ObjectDelete(_trendChartId, name);
        ObjectCreate(_trendChartId, name, OBJ_TREND, 0, _lastMedianPoint.Time, _lastMedianPoint.Price, time, price);
        ObjectSetInteger(_trendChartId, name, OBJPROP_WIDTH, 6);
        ObjectSetInteger(_trendChartId, name, OBJPROP_COLOR, clrOrange);
    }
    _lastMedianPoint.Time = time;
    _lastMedianPoint.Price = price;
}
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
