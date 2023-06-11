//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * GM Expert Advisor *
//| Neo Eureka, neo@zeartis.com 
//| http://zeartis.com
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2020-2021, Geoffrey McGowan"
#property version   "2.00"

#include "MTS\\Logger.mqh"
#include "MTS\\Lib\\ArrayList.mqh"
#include "MTS\\Market\\SymbolInfo.mqh"
#include "MTS\\ExpertAdvisor.mqh"
#include "GM.mqh"

#property strict

const   string          ShortName                           = "GM";
const   uchar           VersionSerial                       = 0x02;
const   string          ServiceURI                          = "tcp://127.12.12.1:5555";

enum YesNo
{
    YN_NO = int(false), // No
    YN_YES = int(true)  // Yes
};

sinput  string          __MainSettings__                    = "------------";       // [MAIN SETTINGS]
input   ulong           Magic                               = 5577;                 // Expert advisor ID
input   uint            QueryInterval                       = 1000;                 // Data check interval (in milliseconds)
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
input   YesNo           WithTrend_CloseIfReversed           = YN_YES;               // Close on VA reversal
input   uint            WithTrend_TrendPeriod               = 5;                    // Trend period
sinput  string          __AgainstTrendSettings__            = "------------";       // [TRADING AGAINST TREND]
input   double          AgainstTrend_TakeProfitATRFactor    = 1.0;                  // Take profit ATR factor (0 = no TP)
input   double          AgainstTrend_StopLossATRFactor      = 1.0;                  // Stop loss ATR factor
input   YesNo           AgainstTrend_UseTrailingStop        = YN_YES;               // Use trailing stop
input   YesNo           AgainstTrend_CloseIfReversed        = YN_YES;               // Close on VA reversal
input   uint            AgainstTrend_TrendPeriod            = 3;                    // Trend period
sinput  string          __TradingTimeSettings__             = "------------";       // [TRADING TIME]
input   TimeOfDay15M    DayTradingStart                     = TOD_08_00;            // Start trading at (server time)
input   TimeOfDay15M    DayTradingStop                      = TOD_22_00;            // Stop trading at (server time)

GMExpertAdvisor ea(ShortName, VersionSerial);
InputParams inp;

int OnInit()
{
    Logger::Options(Logger::SHOW_LEVEL);
    Logger::Level(Logger::DEBUG);
    inp.ServiceURI = ServiceURI;
    inp.Magic = Magic;
    inp.QueryInterval = QueryInterval;
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
    inp.WithTrend.CloseIfReversed = WithTrend_CloseIfReversed;
    inp.WithTrend.TrendPeriod = WithTrend_TrendPeriod;
    inp.AgainstTrend.TakeProfitATRFactor = AgainstTrend_TakeProfitATRFactor;
    inp.AgainstTrend.StopLossATRFactor = AgainstTrend_StopLossATRFactor;
    inp.AgainstTrend.UseTrailingStop = bool(AgainstTrend_UseTrailingStop);
    inp.AgainstTrend.CloseIfReversed = AgainstTrend_CloseIfReversed;
    inp.AgainstTrend.TrendPeriod = AgainstTrend_TrendPeriod;
    inp.DayTradingStart = 60 * DayTradingStart;
    inp.DayTradingStop = 60 * DayTradingStop;
    inp.IsForTester = false;
    
    bool success = ea.Setup(inp, MQLInfoInteger(MQL_TESTER));
    if (success) EventSetMillisecondTimer(QueryInterval);
    return success ? INIT_SUCCEEDED : INIT_FAILED;
}

void OnDeinit(const int reason)
{
    EventKillTimer();
    ea.OnExit(reason);
}

void OnTick()
{
    ea.OnTick();
}

void OnTimer()
{
    ea.OnTimer();
}
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
