//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * TickInfo & SymbolInfo Classes *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2016-2021, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "1.00"

#include "SymbolInfo.i.mqh"

#property strict


bool TickInfo::IsBidChanged()
{
    return (Flags & TICK_FLAG_BID) != 0;
}

bool TickInfo::IsAskChanged()
{
    return (Flags & TICK_FLAG_ASK) != 0;
}

bool TickInfo::IsLastChanged()
{
    return (Flags & TICK_FLAG_LAST) != 0;
}

bool TickInfo::IsVolumeChanged()
{
    return (Flags & TICK_FLAG_VOLUME) != 0;
}

bool TickInfo::IsBuyDeal()
{
    return (Flags & TICK_FLAG_BUY) != 0;
}

bool TickInfo::IsSellDeal()
{
    return (Flags & TICK_FLAG_SELL) != 0;
}
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+

SymbolInfo::SymbolInfo(string sym)
: LoggedState("SymbolInfo")
{
    _sym = (sym != NULL && sym != "" ? sym : Symbol());
}

SymbolInfo::~SymbolInfo()
{}

string SymbolInfo::Name()
{
    return _sym;
}

bool SymbolInfo::IsInMarketWatch()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_SELECT, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return bool(result);
}

long SymbolInfo::SessionDeals()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_SESSION_DEALS, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

long SymbolInfo::SessionBuyOrders()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_SESSION_BUY_ORDERS, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}
 
long SymbolInfo::SessionSellOrders()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_SESSION_SELL_ORDERS, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

long SymbolInfo::LastDealVolume()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_VOLUME, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

long SymbolInfo::MaxDayVolume()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_VOLUMEHIGH, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}
 
long SymbolInfo::MinDayVolume()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_VOLUMELOW, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}
 
datetime SymbolInfo::LastQouteTime()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_TIME, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return datetime(result);
}
 
int SymbolInfo::Digits()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_DIGITS, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return int(result);
}
 
int SymbolInfo::Pip()
{
    return Digits() % 2 == 0 ? 1 : 10;
}
 
bool SymbolInfo::IsSpreadFloating()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_SPREAD_FLOAT, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return bool(result);
}

int SymbolInfo::Spread() 
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_SPREAD, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return int(result);
}

int SymbolInfo::MaxMarketDepth()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_TICKS_BOOKDEPTH, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return int(result);
}

SymbolInfo::TradeMode SymbolInfo::TradeMode()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_TRADE_MODE, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return (TradeMode) result;
}
 
datetime SymbolInfo::TradeStartDate()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_START_TIME, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return datetime(result);
}
 
datetime SymbolInfo::TradeEndDate()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_EXPIRATION_TIME, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return datetime(result);
}
 
int SymbolInfo::TradeStopsLevel()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_TRADE_STOPS_LEVEL, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return int(result);
}

int SymbolInfo::TradeFreezeLevel()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_TRADE_FREEZE_LEVEL, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return int(result);
}

SymbolInfo::TradeExecMode SymbolInfo::TradeExecMode()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_TRADE_EXEMODE, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return (TradeExecMode) result;
}

int SymbolInfo::SwapMode()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_SWAP_MODE, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return int(result);
}

SymbolInfo::DayOfWeek SymbolInfo::SwapRollover3Days()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_SWAP_ROLLOVER3DAYS, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return (DayOfWeek) result;
}

int SymbolInfo::ExpirationMode()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_EXPIRATION_MODE, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return int(result);
}

int SymbolInfo::FillingMode()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_FILLING_MODE, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return int(result);
}

int SymbolInfo::OrderMode()
{
    long result;
    if (!SymbolInfoInteger(_sym, SYMBOL_ORDER_MODE, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return int(result);
}

double SymbolInfo::Bid()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_BID, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::MaxDayBid()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_BIDHIGH, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::MinDayBid()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_BIDLOW, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::Ask()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_ASK, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::MaxDayAsk()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_ASKHIGH, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::MinDayAsk()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_ASKLOW, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::LastDealPrice()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_LAST, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::MaxDayDealPrice()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_LASTHIGH, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::MinDayDealPrice()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_LASTLOW, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::Point()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_POINT, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::TradeTickValue()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_TRADE_TICK_VALUE, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::TradeTickValueProfit()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_TRADE_TICK_VALUE_PROFIT, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::TradeTickValueLoss()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_TRADE_TICK_VALUE_LOSS, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::TradeTickSize()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_TRADE_TICK_SIZE, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::TradeContractSize()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_TRADE_CONTRACT_SIZE, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::MinVolume()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_VOLUME_MIN, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::MaxVolume()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_VOLUME_MAX, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::MinVolumeStep()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_VOLUME_STEP, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::VolumeLimit()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_VOLUME_LIMIT, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::SwapLong()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_SWAP_LONG, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::SwapShort()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_SWAP_SHORT, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::InitialMargin()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_MARGIN_INITIAL, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

double SymbolInfo::MaintenanceMargin()
{
    double result;
    if (!SymbolInfoDouble(_sym, SYMBOL_MARGIN_MAINTENANCE, result))
    {
        _exc.SetState(SYMBOLINFO_EXC_MT_ERROR, NULL, GetLastError());
        _logger.Error(_exc, __FILE__, __FUNCTION__, __LINE__);
    }
    return result;
}

TickInfo SymbolInfo::GetLastTickInfo()
{
    MqlTick mqlTick;
    SymbolInfoTick(_sym, mqlTick);
    TickInfo tick;
    tick.Time = mqlTick.time;
    tick.Bid = mqlTick.bid;
    tick.Ask = mqlTick.ask;
    tick.Last = mqlTick.last;
    tick.Volume = mqlTick.volume;
    tick.TimeMSec = mqlTick.time_msc;
    tick.Flags = mqlTick.flags;
    return tick;
}
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
