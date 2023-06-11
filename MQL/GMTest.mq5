//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * GM Expert Advisor *
//| Neo Eureka, neo@zeartis.com 
//| http://zeartis.com
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2020-2021, Geoffrey McGowan"
#property version   "2.00"

#define _TEST_MODE_

#include "MTS\\Logger.mqh"
#include "MTS\\Lib\\ArrayList.mqh"
#include "MTS\\Market\\SymbolInfo.mqh"
#include "MTS\\ExpertAdvisor.mqh"
#include "GM.mqh"

#property strict

const   string          ShortName                           = "GM";
const   uchar           VersionSerial                       = 0x02;

enum YesNo
{
    YN_NO = int(false), // No
    YN_YES = int(true)  // Yes
};

sinput  string          __MainSettings__                    = "------------";       // [MAIN SETTINGS]
input   string          CCSymbol                            = "";                   // ChartingCenter symbol name
input   double          Lot                                 = 0.1;                  // Lot size
input   ulong           Slippage                            = 5;                    // Slippage (in points)
input   uint            MaxSpread                           = 5;                    // Maximum spread (in points)
input   Timeframe       TrendTimeframe                      = TF_M2;                // Trend timeframe
input   YesNo           TradeWithTrend                      = YN_YES;               // Trade with trend
input   YesNo           TradeAgainstTrend                   = YN_YES;               // Trade against trend
sinput  string          __WithTrendSettings__               = "------------";       // [TRADING WITH TREND]
input   double          WithTrend_TakeProfitATRFactor       = 2.0;                  // Take profit ATR factor (0 = no TP)
input   double          WithTrend_StopLossATRFactor         = 2.0;                  // Stop loss ATR factor
input   YesNo           WithTrend_UseTrailingStop           = YN_YES;               // Use trailing stop
input   uint            WithTrend_TrendPeriod               = 5;                    // Trend period
sinput  string          __AgainstTrendSettings__            = "------------";       // [TRADING AGAINST TREND]
input   double          AgainstTrend_TakeProfitATRFactor    = 1.0;                  // Take profit ATR factor (0 = no TP)
input   double          AgainstTrend_StopLossATRFactor      = 1.0;                  // Stop loss ATR factor
input   YesNo           AgainstTrend_UseTrailingStop        = YN_YES;               // Use trailing stop
input   uint            AgainstTrend_TrendPeriod            = 3;                    // Trend period
sinput  string          __TradingTimeSettings__             = "------------";       // [TRADING TIME]
input   TimeOfDay15M    DayTradingStart                     = TOD_08_00;            // Start trading at (server time)
input   TimeOfDay15M    DayTradingStop                      = TOD_22_00;            // Stop trading at (server time)
sinput  string          __TimeZoneSettings__                = "------------";       // [SERVER TIME ZONE]
input   double          UTCShift                            = 2.0;                  // Shift in hours from UTC
input   datetime        DSTBegin                            = D'2021.03.14 06:00';  // DST begins on
input   datetime        DSTEnd                              = D'2021.11.07 06:00';  // DST ends on

GMExpertAdvisor ea(ShortName, VersionSerial);
InputParams inp;
TesterParams tst;

int OnInit()
{
    Logger::Options(Logger::SHOW_LEVEL);
    Logger::Level(Logger::DEBUG);
    inp.ServiceURI = "";
    inp.Magic = 1;
    inp.QueryInterval = 0;
    inp.CCSymbol = CCSymbol != "" ? CCSymbol : Symbol();
    inp.Lot = Lot;
    inp.Slippage = Slippage;
    inp.MaxSpread = MaxSpread;
    inp.TrendTimeframe = TrendTimeframe;
    inp.TradeWithTrend = bool(TradeWithTrend);
    inp.TradeAgainstTrend = bool(TradeAgainstTrend);
    inp.WithTrend.TakeProfitATRFactor = WithTrend_TakeProfitATRFactor;
    inp.WithTrend.StopLossATRFactor = WithTrend_StopLossATRFactor;
    inp.WithTrend.UseTrailingStop = bool(WithTrend_UseTrailingStop);
    inp.WithTrend.CloseIfReversed = false;
    inp.WithTrend.TrendPeriod = WithTrend_TrendPeriod;
    inp.AgainstTrend.TakeProfitATRFactor = AgainstTrend_TakeProfitATRFactor;
    inp.AgainstTrend.StopLossATRFactor = AgainstTrend_StopLossATRFactor;
    inp.AgainstTrend.UseTrailingStop = bool(AgainstTrend_UseTrailingStop);
    inp.AgainstTrend.CloseIfReversed = false;
    inp.AgainstTrend.TrendPeriod = AgainstTrend_TrendPeriod;
    inp.DayTradingStart = 60 * DayTradingStart;
    inp.DayTradingStop = 60 * DayTradingStop;
    inp.IsForTester = true;
    tst.UTCShift = int(MathRound(3600 * UTCShift));
    tst.DSTBegin = DSTBegin;
    tst.DSTEnd = DSTEnd;
    bool success = ea.Setup(inp, tst, MQLInfoInteger(MQL_TESTER));
    return success ? INIT_SUCCEEDED : INIT_FAILED;
}

void OnDeinit(const int reason)
{
    EventKillTimer();
    ea.OnExit(reason);
}

void OnTick()
{
    ea.OnTestTick();
}
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
