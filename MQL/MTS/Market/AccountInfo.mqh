//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * AccountInfo Class *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2016-2021, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "2.00"

#include "AccountInfo.i.mqh"

#property strict


AccountInfo::AccountInfo()
{}

AccountInfo::~AccountInfo()
{}

long AccountInfo::Login()
{
    return AccountInfoInteger(ACCOUNT_LOGIN);
}

AccountInfo_TradeMode AccountInfo::TradeMode()
{
    AccountInfo_TradeMode tm = AI_TRADE_MODE_UNKNOWN;
    switch(ENUM_ACCOUNT_TRADE_MODE(AccountInfoInteger(ACCOUNT_TRADE_MODE)))
    {
        case ACCOUNT_TRADE_MODE_DEMO: tm = AI_TRADE_MODE_DEMO; break;
        case ACCOUNT_TRADE_MODE_CONTEST: tm = AI_TRADE_MODE_CONTEST; break;
        case ACCOUNT_TRADE_MODE_REAL: tm = AI_TRADE_MODE_REAL; break;
    };
    return tm;
}

long AccountInfo::Leverage()
{
    return AccountInfoInteger(ACCOUNT_LEVERAGE);
}

int AccountInfo::OrdersLimit()
{
    return int(AccountInfoInteger(ACCOUNT_LIMIT_ORDERS));
}

AccountInfo_StopOutMode AccountInfo::MarginStopOutMode()
{
    AccountInfo_StopOutMode som = AI_STOP_OUT_MODE_UNKNOWN;
    switch(ENUM_ACCOUNT_STOPOUT_MODE(AccountInfoInteger(ACCOUNT_MARGIN_SO_MODE)))
    {
        case ACCOUNT_STOPOUT_MODE_PERCENT: som = AI_STOP_OUT_MODE_PERCENT; break;
        case ACCOUNT_STOPOUT_MODE_MONEY: som = AI_STOP_OUT_MODE_MONEY; break;
    };
    return som;
}

bool AccountInfo::CanTrade()
{
    return bool(AccountInfoInteger(ACCOUNT_TRADE_ALLOWED));
}

bool AccountInfo::CanAutoTrade()
{
    return bool(AccountInfoInteger(ACCOUNT_TRADE_EXPERT));
}

double AccountInfo::Balance()
{
    return AccountInfoDouble(ACCOUNT_BALANCE);
}

double AccountInfo::Credit()
{
    return AccountInfoDouble(ACCOUNT_CREDIT);
}

double AccountInfo::Profit()
{
    return AccountInfoDouble(ACCOUNT_PROFIT);
}

double AccountInfo::Equity()
{
    return AccountInfoDouble(ACCOUNT_EQUITY);
}

double AccountInfo::Margin()
{
    return AccountInfoDouble(ACCOUNT_MARGIN);
}

double AccountInfo::FreeMargin()
{
    return AccountInfoDouble(ACCOUNT_MARGIN_FREE);
}

double AccountInfo::MarginLevelPercent()
{
    return AccountInfoDouble(ACCOUNT_MARGIN_LEVEL);
}

double AccountInfo::MarginCallLevel()
{
    return AccountInfoDouble(ACCOUNT_MARGIN_SO_CALL);
}

double AccountInfo::MarginStopOutLevel()
{
    return AccountInfoDouble(ACCOUNT_MARGIN_SO_SO);
}

double AccountInfo::InitialMargin()
{
    return AccountInfoDouble(ACCOUNT_MARGIN_INITIAL);
}

double AccountInfo::MaintenanceMargin()
{
    return AccountInfoDouble(ACCOUNT_MARGIN_MAINTENANCE);
}

double AccountInfo::Assets()
{
    return AccountInfoDouble(ACCOUNT_ASSETS);
}

double AccountInfo::Liabilities()
{
    return AccountInfoDouble(ACCOUNT_LIABILITIES);
}

double AccountInfo::BlockedCommision()
{
    return AccountInfoDouble(ACCOUNT_COMMISSION_BLOCKED);
}

string AccountInfo::Name()
{
    return AccountInfoString(ACCOUNT_NAME);
}

string AccountInfo::ServerName()
{
    return AccountInfoString(ACCOUNT_SERVER);
}

string AccountInfo::Currency()
{
    return AccountInfoString(ACCOUNT_CURRENCY);
}

string AccountInfo::CompanyName()
{
    return AccountInfoString(ACCOUNT_COMPANY);
}
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
