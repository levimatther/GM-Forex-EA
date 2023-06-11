//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
//| * Util Class - Miscellaneous common utility functions *
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
#property copyright "© 2016-2021, Neo Eureka, neo@zeartis.com"
#property link      "http://zeartis.com"
#property version   "2.00"

#property strict


class Util
{
public:
static int CmpReal(double v1, double v2);
static string Hex(const uchar& bytes[]);
static string Cvt32BytesToText36(const uchar & bytes[]);
private:
    union Util_32BytesTo4Longs
    {
        uchar _32Bytes[32];
        ulong _4Longs[4];
    };
private:
    static const double EPSILON;
    static const uchar HEX_TAB[16];
    static const uchar TEXT36_TAB[36];
    static const int TEXT36_PART_LEN;
};

static const double Util::EPSILON = 1e-6;

static const uchar Util::HEX_TAB[16] = {'0', '1', '2' ,'3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

static const uchar Util::TEXT36_TAB[36] =
    {
        '0', '1', '2' ,'3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
        'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
    };

static const int Util::TEXT36_PART_LEN = int(ceil(log(double(ULONG_MAX)) / log(36)));

static int Util::CmpReal(double v1, double v2)
{
    return fabs(v1 - v2) < EPSILON ? 0 : v1 < v2 ? -1 : 1;
}

static string Util::Hex(const uchar& bytes[])
{
    uchar hex[];
    ArrayResize(hex, 2 * ArraySize(bytes));
    int len = ArraySize(bytes);
    for (int i = 0 ; i < len ; i ++)
    {
        uchar b = bytes[i];
        hex[i * 2] = HEX_TAB[(b >> 4) & 0xF]; hex[i * 2 + 1] = HEX_TAB[b & 0xF];
    }
    return CharArrayToString(hex);
}

static string Util::Cvt32BytesToText36(const uchar &bytes[])
{
    Util_32BytesTo4Longs data;
    ArrayCopy(data._32Bytes, bytes);
    string txt = "";
    uchar part[];
    ArrayResize(part, TEXT36_PART_LEN);
    for (int p = 0 ; p <= 3 ; p ++)
    {
        ArrayInitialize(part, '0');
        ulong ld = data._4Longs[p];
        for (int i = TEXT36_PART_LEN - 1 ; 0 <= i ; i --)
        {
            int d = int(ld % 36);
            ld = (ld - d) / 36;
            part[i] = TEXT36_TAB[d];
        }
        txt += CharArrayToString(part);
    }
    return txt;
}
//+------------------------------------------------------------------------------------------------------------------------------------------------------------+
