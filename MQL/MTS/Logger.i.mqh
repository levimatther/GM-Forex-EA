//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * Logger Class -- Interface *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2016-2021, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "2.00"

#include "ExcDescriptor.i.mqh"

#property strict


class Logger
{
public:
    enum LevelEnum
    {
        NONE = 0,       // None
        CRITICAL = 1,   // Critical
        ERROR = 2,      // Error
        WARNING = 3,    // Warning
        INFO = 4,       // Info
        DEBUG = 5       // Debug
    };

    enum OptionsEnum
    {
        TEXT_ONLY = 0x00,
        SHOW_TIME = 0x01,
        SHOW_LEVEL = 0x02,
        SHOW_SOURCE = 0x04,
        SHOW_LOCATION = 0x08,
        SHOW_ALL = 0x0F
    };

public:
    static LevelEnum Level();
    static void Level(LevelEnum level);
    static uint Options();
    static void Options(uint opts);
    static string RootName();
    static void RootName(string name);
    static string JournalPrefix();
    static void JournalPrefix(string prefix);
    static void AttachLogFile(LevelEnum level, string outputFile);
    static void DetachLogFile(LevelEnum level);
    static void DetachAllLogs();

public:
    Logger(string srcName);
    ~Logger();
    void Message(LevelEnum msgLevel, string message, string file = NULL, string func = NULL, int lineNo = 0);
    void Message(LevelEnum msgLevel, ExcDescriptor &exc, string file = NULL, string func = NULL, int lineNo = 0);
    void Critical(string message, string file = NULL, string func = NULL, int lineNo = 0);
    void Critical(ExcDescriptor &exc, string file = NULL, string func = NULL, int lineNo = 0);
    void Error(string message, string file = NULL, string func = NULL, int lineNo = 0);
    void Error(ExcDescriptor &exc, string file = NULL, string func = NULL, int lineNo = 0);
    void Warning(string message, string file = NULL, string func = NULL, int lineNo = 0);
    void Warning(ExcDescriptor &exc, string file = NULL, string func = NULL, int lineNo = 0);
    void Info(string message, string file = NULL, string func = NULL, int lineNo = 0);
    void Info(ExcDescriptor &exc, string file = NULL, string func = NULL, int lineNo = 0);
    void Debug(string message, string file = NULL, string func = NULL, int lineNo = 0);
    void Debug(ExcDescriptor &exc, string file = NULL, string func = NULL, int lineNo = 0);

private:
    struct _LogRec
    {
    public:
        string outputFile;
        int fHandle;
    };

private:
    string _GetErrorMessage(int err);

private:
    static LevelEnum _level;
    static string _rootName;
    static uint _options;
    static string _jPrefix;
    static const string _LEVEL_NAMES[6];
    static _LogRec _logs[6];
    static int _loggersCnt;
private:
    string _srcName;
};
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
