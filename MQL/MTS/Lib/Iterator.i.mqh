//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * Iterator Abstract Class -- Interface *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2020-2021, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "2.00"

#property strict


template<typename T>
class Iterator
{
public:
    virtual int Count() const;
    virtual void Begin();
    virtual void End();
    virtual bool First();
    virtual bool Last();
    virtual bool Prev();
    virtual bool Next();
    virtual bool AtBegin() const;
    virtual bool AtEnd() const;
    virtual bool AtFirst() const;
    virtual bool AtLast() const;
    virtual T Get() const;

protected:
    int _count;
};
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
