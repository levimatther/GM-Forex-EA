//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * AccountInfo Class -- Interface *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2016-2021, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "2.00"

#property strict


class AccountInfo
{
public:
    enum AccountInfo_TradeMode
    {
        AI_TRADE_MODE_UNKNOWN = 0,
        AI_TRADE_MODE_DEMO = 1,
        AI_TRADE_MODE_CONTEST = 2,
        AI_TRADE_MODE_REAL = 3
    };

    enum AccountInfo_StopOutMode
    {
        AI_STOP_OUT_MODE_UNKNOWN = 0,
        AI_STOP_OUT_MODE_PERCENT = 1,
        AI_STOP_OUT_MODE_MONEY = 2
    };

public:
    AccountInfo();
    ~AccountInfo();
    long Login();
    AccountInfo_TradeMode TradeMode();
    long Leverage();
    int OrdersLimit();
    AccountInfo_StopOutMode MarginStopOutMode();
    bool CanTrade();
    bool CanAutoTrade();
    double Balance();
    double Credit();
    double Profit();
    double Equity();
    double Margin();
    double FreeMargin();
    double MarginLevelPercent();
    double MarginCallLevel();
    double MarginStopOutLevel();
    double InitialMargin();
    double MaintenanceMargin();
    double Assets();
    double Liabilities();
    double BlockedCommision();
    string Name();
    string ServerName();
    string Currency();
    string CompanyName();
};
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
