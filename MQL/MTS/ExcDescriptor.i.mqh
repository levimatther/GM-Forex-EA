//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * ExcDescriptor Class -- Interface *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2016-2021, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "2.00"

#property strict


class ExcDescriptor
{
public:
    ExcDescriptor() { Clear(); }
    ~ExcDescriptor() {}
    bool IsSuccess() const { return(_state == 0); }
    bool IsFail() const { return(_state != 0); }
    uint GetState() const { return(_state); }
    string GetInfo() const { return(_info); }
    int GetLinkedErr() const { return(_linkedErr); }
    void Clear() { _state = 0; _info = NULL; _linkedErr = ERR_SUCCESS; }
    void Set(const ExcDescriptor& exc) { _state = exc._state; _info = exc._info; _linkedErr = exc._linkedErr; }
    void SetState(uint state, string info = NULL, int linkedErr = 0) { _state = state; _info = info; _linkedErr = linkedErr; }

private:
    uint _state;
    string _info;
    int _linkedErr;
};
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
