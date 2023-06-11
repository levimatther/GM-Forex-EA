//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * ArrayList Generic Class -- Interface *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2020-2021, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "2.00"

#include "Iterator.i.mqh"

#property strict


template<typename T>
class ArrayList : public Iterator<T>
{
public:
    ArrayList(T nullValue = NULL, int allocQuant = 0);
    ~ArrayList();
    int GetArray(T &array[]) const;
    bool IsEmpty() const;
    int Count() const;
    void Begin();
    void End();
    bool First();
    bool Last();
    bool Prev();
    bool Next();
    bool AtBegin() const;
    bool AtEnd() const;
    bool AtFirst() const;
    bool AtLast() const;
    int Index() const;
    bool Seek(int index);
    T Get() const;
    T Get(int index) const;
    bool Put(T obj);
    void Put(T &array[]);
    bool Insert(T obj, bool after = true);
    bool Insert(T obj, int index);
    bool Remove(bool autoCompact = false);
    T Top() const;
    void Top(T obj);
    void Push(T obj);
    T Pop(bool autoCompact = false);
    void Compact();
    void Clear();

private:
    T _array[];
    int _itPos;
    int _allocQuant;
    T _nullValue;
    bool _compactPending;
};
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
