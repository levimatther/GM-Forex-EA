//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * ExpertAdvisor Abstract Class -- Interface *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2016-2021, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "2.00"

#include "LoggedState.i.mqh"

#property strict


template<typename TInputParams>
class ExpertAdvisor : public LoggedState
{
public:
    ExpertAdvisor(string name);
    ~ExpertAdvisor();
    bool Setup(const TInputParams & inputParams, bool isTesting);
    void Stop();
    virtual void OnTick();
    virtual void OnTimer();
    virtual void OnTrade();
    virtual void OnTradeTransaction(const MqlTradeTransaction& trans, const MqlTradeRequest& request, const MqlTradeResult& result);
    virtual void OnExit(const int reason);

protected:
    virtual bool OnInit();
    virtual void OnExitAny();
    virtual void OnExitInitFailed();
    virtual void OnExitStopped();
    virtual void OnExitClosed();
    virtual void OnExitRemoved();
    virtual void OnExitChartChanged();
    virtual void OnExitParamsChanged();
    virtual void OnExitAccount();

protected:
    const TInputParams * _inputParams;
    bool _isTesting;
    bool _initialized;
};
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
