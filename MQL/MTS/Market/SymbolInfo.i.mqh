//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * TickInfo & SymbolInfo Classes -- Interface *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2016-2017, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "1.00"

#include "../LoggedState.i.mqh"

#property strict


struct TickInfo
{
    datetime Time; 
    double   Bid;
    double   Ask;
    double   Last;
    ulong    Volume;
    long     TimeMSec;
    uint     Flags;

    bool IsBidChanged();
    bool IsAskChanged();
    bool IsLastChanged();
    bool IsVolumeChanged();
    bool IsBuyDeal();
    bool IsSellDeal();
};
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+

class SymbolInfo : public LoggedState
{
public:
    enum EXC
    {
        SYMBOLINFO_EXC_MT_ERROR = 0x01
    };
    enum TradeMode
    {
        SI_TM_DISABLED = 0,
        SI_TM_CLOSE_ONLY = 1,
        SI_TM_FULL = 2,
        SI_TM_LONG_ONLY = 3,
        SI_TM_SHORT_ONLY = 4
    };
    enum TradeExecMode
    {
        SI_TEM_REQUEST = 0,
        SI_TEM_INSTANT = 1,
        SI_TEM_MARKET = 2,
        SI_TEM_EXCHANGE = 3
    };
    enum DayOfWeek
    {
        SI_DOW_SUNDAY = 0,
        SI_DOW_MONDAY = 1,
        SI_DOW_TUESDAY = 2,
        SI_DOW_WEDNESDAY = 3,
        SI_DOW_THURSDAY = 4,
        SI_DOW_FRIDAY = 5,
        SI_DOW_SATURDAY = 6
    };
public:
    SymbolInfo(string sym);
    ~SymbolInfo();
    string Name();
    bool IsInMarketWatch();
    long SessionDeals();
    long SessionBuyOrders();
    long SessionSellOrders();
    long LastDealVolume();
    long MaxDayVolume();
    long MinDayVolume();
    datetime LastQouteTime();
    int Digits();
    int Pip();
    bool IsSpreadFloating();
    int Spread();
    int MaxMarketDepth();
    TradeMode TradeMode();
    datetime TradeStartDate();
    datetime TradeEndDate();
    int TradeStopsLevel();
    int TradeFreezeLevel();
    TradeExecMode TradeExecMode();
    int SwapMode();
    DayOfWeek SwapRollover3Days();
    int ExpirationMode();
    int FillingMode();
    int OrderMode();
    double Bid();
    double MaxDayBid();
    double MinDayBid();
    double Ask();
    double MaxDayAsk();
    double MinDayAsk();
    double LastDealPrice();
    double MaxDayDealPrice();
    double MinDayDealPrice();
    double Point();
    double TradeTickValue();
    double TradeTickValueProfit();
    double TradeTickValueLoss();
    double TradeTickSize();
    double TradeContractSize();
    double MinVolume();
    double MaxVolume();
    double MinVolumeStep();
    double VolumeLimit();
    double SwapLong();
    double SwapShort();
    double InitialMargin();
    double MaintenanceMargin();
    TickInfo GetLastTickInfo();
    
private:
    string _sym;
};
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
