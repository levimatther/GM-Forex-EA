//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * LoggedState Class -- Interface *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2016-2021, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "2.00"

#include "ExcDescriptor.i.mqh"
#include "Logger.i.mqh"

#property strict


class LoggedState
{
public:
    ExcDescriptor * Exc() { return &_exc; }

protected:
    LoggedState(string src_name) : _logger(src_name) {}

protected:
    ExcDescriptor _exc;
    Logger _logger;
};
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
