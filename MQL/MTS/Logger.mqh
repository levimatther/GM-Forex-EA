//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * Logger Class *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2016-2017, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "1.00"

#include "Logger.i.mqh"

#property strict


static Logger::LevelEnum Logger::_level = Logger::ERROR;

static string Logger::_rootName = "";

static uint Logger::_options = SHOW_ALL;

static string Logger::_jPrefix = "";

static const string Logger::_LEVEL_NAMES[6] = { NULL, "CRITICAL", "ERROR", "WARNING", "INFO", "DEBUG" };

static Logger::_LogRec Logger::_logs[6] =
    {
        { NULL, INVALID_HANDLE },
        { NULL, INVALID_HANDLE },
        { NULL, INVALID_HANDLE },
        { NULL, INVALID_HANDLE },
        { NULL, INVALID_HANDLE },
        { NULL, INVALID_HANDLE }
    };

static int Logger::_loggersCnt = 0;

static Logger::LevelEnum Logger::Level()
{
    return _level;
}

static void Logger::Level(Logger::LevelEnum level)
{
    _level = level;
}

static uint Logger::Options()
{
    return _options;
}

static void Logger::Options(uint opts)
{
    _options = opts;
}

static string Logger::RootName()
{
    return _rootName;
}

static void Logger::RootName(string name)
{
    _rootName = name;
}

static string Logger::JournalPrefix()
{
    return _jPrefix;
}

static void Logger::JournalPrefix(string prefix)
{
    _jPrefix = prefix;
}

static void Logger::AttachLogFile(Logger::LevelEnum level, string outputFile)
{
    if (level == NONE) return;
    if (_logs[level].fHandle != INVALID_HANDLE) FileClose(_logs[level].fHandle);
    _logs[level].outputFile = outputFile;
    _logs[level].fHandle = FileOpen(outputFile, FILE_WRITE | FILE_SHARE_READ | FILE_TXT | FILE_ANSI);
    for (LevelEnum lvl = CRITICAL ; lvl < level ; lvl ++)
        if (_logs[lvl].fHandle == INVALID_HANDLE)
        {
            _logs[lvl].outputFile = _logs[level].outputFile;
            _logs[lvl].fHandle = _logs[level].fHandle;
        }
}

static void Logger::DetachLogFile(Logger::LevelEnum level)
{
    if (level == NONE) return;
    _logs[level].outputFile = NULL;
    if (_logs[level].fHandle != INVALID_HANDLE)
    {
        FileClose(_logs[level].fHandle);
        _logs[level].fHandle = INVALID_HANDLE;
    }
}

static void Logger::DetachAllLogs()
{
    int lastLevel = ArraySize(Logger::_logs) - 1;
    for (int level = 1 ; level <= lastLevel ; level ++)
        Logger::DetachLogFile((LevelEnum) level);
}

Logger::Logger(string srcName)
{
    _srcName = srcName;
    _loggersCnt ++;
}

Logger::~Logger()
{}

void Logger::Message(Logger::LevelEnum msgLevel, string message, string file = NULL, string func = NULL, int lineNo = 0)
{
    datetime now = TimeCurrent();
    if (NONE < Logger::_level && NONE < msgLevel && msgLevel <= Logger::_level)
    {
        string outTxt = "";
        if ((_options & SHOW_TIME) != 0) outTxt += TimeToString(now, TIME_DATE|TIME_MINUTES|TIME_SECONDS) + " ";
        if ((_options & SHOW_SOURCE) != 0) outTxt += StringFormat("%s::%s", _rootName, _srcName);
        if ((_options & SHOW_LOCATION) != 0)
            outTxt += (file != NULL ? "(" + file + (func != NULL ? "|" + func : "") + (lineNo != 0 ? "|" + IntegerToString(lineNo) : "") + ")" : "");
        if ((_options & (SHOW_SOURCE|SHOW_LOCATION)) != 0) outTxt += ": ";
        if ((_options & SHOW_LEVEL) != 0) outTxt += StringFormat("[%s] ", _LEVEL_NAMES[msgLevel]);
        outTxt += message;
        int fHandle = _logs[msgLevel].fHandle;
        if (fHandle == INVALID_HANDLE) printf("%s%s", _jPrefix, outTxt);
        else { FileWriteString(fHandle, outTxt + "\r\n"); FileFlush(fHandle); }
    }
}

void Logger::Message(Logger::LevelEnum msgLevel, ExcDescriptor &exc, string file = NULL, string func = NULL, int lineNo = 0)
{
    if (NONE < Logger::_level && NONE < msgLevel && msgLevel <= Logger::_level)
    {
        int linkedErr = exc.GetLinkedErr();
        string linkedErrTxt = (linkedErr != ERR_SUCCESS ? StringFormat("; (#%d) %s", linkedErr, _GetErrorMessage(linkedErr)) : "");
        string info = (exc.GetInfo() != NULL ? StringFormat(" %s", exc.GetInfo()) : "");
        string message = StringFormat("%04X%s%s", exc.GetState(), info, linkedErrTxt);
        Message(msgLevel, message, file, func, lineNo);
    }
}

void Logger::Critical(string message, string file = NULL, string func = NULL, int lineNo = 0)
{
    Message(CRITICAL, message, file, func, lineNo);
}

void Logger::Critical(ExcDescriptor &exc, string file = NULL, string func = NULL, int lineNo = 0)
{
    Message(CRITICAL, exc, file, func, lineNo);
}

void Logger::Error(string message, string file = NULL, string func = NULL, int lineNo = 0)
{
    Message(ERROR, message, file, func, lineNo);
}

void Logger::Error(ExcDescriptor &exc, string file = NULL, string func = NULL, int lineNo = 0)
{
    Message(ERROR, exc, file, func, lineNo);
}

void Logger::Warning(string message, string file = NULL, string func = NULL, int lineNo = 0)
{
    Message(WARNING, message, file, func, lineNo);
}

void Logger::Warning(ExcDescriptor &exc, string file = NULL, string func = NULL, int lineNo = 0)
{
    Message(WARNING, exc, file, func, lineNo);
}

void Logger::Info(string message, string file = NULL, string func = NULL, int lineNo = 0)
{
    Message(INFO, message, file, func, lineNo);
}

void Logger::Info(ExcDescriptor &exc, string file = NULL, string func = NULL, int lineNo = 0)
{
    Message(INFO, exc, file, func, lineNo);
}

void Logger::Debug(string message, string file = NULL, string func = NULL, int lineNo = 0)
{
    Message(DEBUG, message, file, func, lineNo);
}

void Logger::Debug(ExcDescriptor &exc, string file = NULL, string func = NULL, int lineNo = 0)
{
    Message(DEBUG, exc, file, func, lineNo);
}

string Logger::_GetErrorMessage(int err)
{
    return StringFormat("Error #%d", err);
}

//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
