//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * ArrayList Generic Class *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2020-2021, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "2.00"

#include "ArrayList.i.mqh"

#property strict


template<typename T>
ArrayList::ArrayList(T nullValue = NULL, int allocQuant = 0)
{
    _nullValue = nullValue;
    _allocQuant = allocQuant;
    _count = 0;
    _itPos = -1;
    _compactPending = false;
}

template<typename T>
ArrayList::~ArrayList()
{
    ArrayFree(_array);
}

template<typename T>
int ArrayList::GetArray(T &array[]) const
{
    ArrayResize(array, _count);
    for (int i = 0 ; i < _count ; i ++) array[i] = _array[i];
    return _count;
}

template<typename T>
bool ArrayList::IsEmpty() const
{
    return _count == 0;
}

template<typename T>
int ArrayList::Count() const
{
    return _count;
}

template<typename T>
void ArrayList::Begin()
{
    _itPos = -1;
}

template<typename T>
void ArrayList::End()
{
    _itPos = _count;
}

template<typename T>
bool ArrayList::First()
{
    bool hasFirst = (0 < _count);
    _itPos = (hasFirst ? 0 : -1);
    return hasFirst;
}

template<typename T>
bool ArrayList::Last()
{
    bool hasLast = (0 < _count);
    _itPos = (hasLast ? _count - 1 : 0);
    return hasLast;
}

template<typename T>
bool ArrayList::Prev()
{
    bool hasPrev = (0 < _count && 0 < _itPos);
    if (-1 < _itPos) _itPos --;
    return hasPrev;
}

template<typename T>
bool ArrayList::Next()
{
    bool hasNext = (0 < _count && _itPos < _count - 1);
    if (_itPos < _count) _itPos ++;
    return hasNext;
}

template<typename T>
bool ArrayList::AtBegin() const
{
    return _itPos == -1;
}

template<typename T>
bool ArrayList::AtEnd() const
{
    return _itPos == _count;
}
template<typename T>
bool ArrayList::AtFirst() const
{
    return 0 < _count && _itPos == 0;
}

template<typename T>
bool ArrayList::AtLast() const
{
    return 0 < _count && _itPos == _count - 1;
}

template<typename T>
int ArrayList::Index() const
{
    return _itPos;
}

template<typename T>
bool ArrayList::Seek(int index)
{
    bool isValid = (index == -1 || (0 <= index && index <= _count));
    if (isValid) _itPos = index;
    return isValid;
}

template<typename T>
T ArrayList::Get() const
{
    if (0 < _count && 0 <= _itPos && _itPos <= _count - 1) return(_array[_itPos]);
    return _nullValue;
}

template<typename T>
T ArrayList::Get(int index) const
{
    if (0 < _count && 0 <= index && index <= _count - 1) return(_array[index]);
    return _nullValue;
}

template<typename T>
bool ArrayList::Put(T obj)
{
    if (0 < _count && 0 <= _itPos && _itPos <= _count - 1)
    {
        _array[_itPos] = obj;
        return true;
    }
    return false;
}

template<typename T>
void ArrayList::Put(T &array[])
{
    _count = ArraySize(array);
    ArrayResize(_array, _count);
    for (int i = 0 ; i < _count ; i ++) _array[i] = array[i];
    _itPos = -1;
    _compactPending = false;
}

template<typename T>
bool ArrayList::Insert(T obj, bool after = true)
{
    if (after)
    {
        int index = fmin(_itPos + 1, _count);
        ArrayResize(_array, _count + 1, _allocQuant);
        for (int i = _count ; index < i ; i --) _array[i] = _array[i - 1];
        _array[index] = obj;
        _count ++;
        _itPos = index;
    }
    else
    {
        int index = fmax(0, _itPos);
        ArrayResize(_array, _count + 1, _allocQuant);
        for (int i = _count ; index < i ; i --) _array[i] = _array[i - 1];
        _array[index] = obj;
        _count ++;
        _itPos = index;
    }
    return true;
}

template<typename T>
bool ArrayList::Insert(T obj, int index)
{
    if (index < 0 || _count < index) return(false);
    ArrayResize(_array, _count + 1, _allocQuant);
    for (int i = _count ; index < i ; i --) _array[i] = _array[i - 1];
    _array[index] = obj;
    _count ++;
    if (index <= _itPos) _itPos ++;
    return true;
}

template<typename T>
bool ArrayList::Remove(bool autoCompact = false)
{
    if (0 < _count && 0 <= _itPos && _itPos <= _count - 1)
    {
        _array[_itPos] = _nullValue;
        _compactPending = true;
        if (autoCompact) Compact();
        return true;
    }
    return false;
}

template<typename T>
T ArrayList::Top() const
{
    return (0 < _count ? _array[_count - 1] : _nullValue);
}

template<typename T>
void ArrayList::Top(T obj)
{
    if (0 < _count) _array[_count - 1] = obj;
}

template<typename T>
void ArrayList::Push(T obj)
{
    _count ++;
    ArrayResize(_array, _count, _allocQuant);
    _array[_count - 1] = obj;
}

template<typename T>
T ArrayList::Pop(bool autoCompact = false)
{
    if (0 < _count)
    {
        T obj = _array[_count - 1];
        _array[_count - 1] = _nullValue;
        if (autoCompact)
        {
            _count --;
            if (_count < _itPos) _itPos = _count;
            ArrayResize(_array, _count, _allocQuant);
        }
        return obj;
    }
    return _nullValue;
}

template<typename T>
void ArrayList::Compact()
{
    if (!_compactPending || _count == 0) return;
    int lastFilledI = -1;
    for (int filledI = 0 ; filledI < _count ; filledI ++)
        if (_array[filledI] == _nullValue)
        {
            for (int i = filledI + 1 ; i < _count ; i ++)
                if (_array[i] != _nullValue)
                {
                    _array[filledI] = _array[i];
                    _array[i] = _nullValue;
                    lastFilledI = filledI;
                    break;
                }
        }
        else
            lastFilledI = filledI;
    _count = lastFilledI + 1;
    _itPos = -1;
    ArrayResize(_array, _count, _allocQuant);  
    _compactPending = false;
}

template<typename T>
void ArrayList::Clear()
{
    _count = 0;
    _itPos = -1;
    _compactPending = false;
    ArrayFree(_array);  
}
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
