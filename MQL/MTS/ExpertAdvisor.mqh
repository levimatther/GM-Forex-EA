//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * ExpertAdvisor Abstract Class *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2016-2021, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "2.00"

#include "ExpertAdvisor.i.mqh"

#property strict


template<typename TInputParams>
ExpertAdvisor::ExpertAdvisor(string name)
: LoggedState(name)
{
    _initialized = false;
}

template<typename TInputParams>
ExpertAdvisor::~ExpertAdvisor()
{
    if (CheckPointer(_inputParams) == POINTER_DYNAMIC) delete _inputParams;
}

template<typename TInputParams>
bool ExpertAdvisor::Setup(const TInputParams & inputParams, bool isTesting)
{
    _inputParams = &inputParams;
    _isTesting = isTesting;
    return OnInit();
}

template<typename TInputParams>
void ExpertAdvisor::Stop()
{
    ExpertRemove();
}

template<typename TInputParams>
bool ExpertAdvisor::OnInit()
{
    return true;
}

template<typename TInputParams>
void ExpertAdvisor::OnTick()
{}

template<typename TInputParams>
void ExpertAdvisor::OnTimer()
{}

template<typename TInputParams>
void ExpertAdvisor::OnTrade()
{}

template<typename TInputParams>
void ExpertAdvisor::OnTradeTransaction(const MqlTradeTransaction& trans, const MqlTradeRequest& request, const MqlTradeResult& result)
{}

template<typename TInputParams>
void ExpertAdvisor::OnExit(const int reason)
{
    OnExitAny();
    switch (reason)
    {
        case REASON_PROGRAM: OnExitStopped(); break;
        case REASON_REMOVE: OnExitRemoved(); break;
        case REASON_RECOMPILE: OnExitStopped(); break;
        case REASON_CHARTCHANGE: OnExitChartChanged(); break;
        case REASON_CHARTCLOSE: OnExitClosed(); break;
        case REASON_PARAMETERS: OnExitParamsChanged(); break;
        case REASON_ACCOUNT: OnExitAccount(); break;
        case REASON_TEMPLATE: OnExitRemoved(); break;
        case REASON_INITFAILED: OnExitInitFailed(); break;
        case REASON_CLOSE: OnExitClosed(); break;
        default: break;
    }
}

template<typename TInputParams>
void ExpertAdvisor::OnExitAny()
{}

template<typename TInputParams>
void ExpertAdvisor::OnExitInitFailed()
{}

template<typename TInputParams>
void ExpertAdvisor::OnExitStopped()
{}

template<typename TInputParams>
void ExpertAdvisor::OnExitClosed()
{}

template<typename TInputParams>
void ExpertAdvisor::OnExitRemoved()
{}

template<typename TInputParams>
void ExpertAdvisor::OnExitChartChanged()
{}

template<typename TInputParams>
void ExpertAdvisor::OnExitParamsChanged()
{}

template<typename TInputParams>
void ExpertAdvisor::OnExitAccount()
{}
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
