#pragma once

#pragma pack(push, 1)

struct data_params_t
{
    struct trade_setup_t
    {
        uint32_t trend_period;
    };

    uint32_t main_timeframe;
    uint32_t trend_timeframe;
    trade_setup_t with_trend;
    trade_setup_t against_trend;
};

struct market_data_t
{
    enum class what_t : uint32_t { none = 0, tick = 0x01, signal = 0x02 };

    enum class dir_t : int32_t { none = 0, up = +1, down = -1 };

    struct tick_t
    {
        uint64_t time;
        double price;
    };

    struct price_and_dir_t
    {
        double price;
        dir_t dir;
    };

    struct signal_t
    {

        struct median_t
        {
            price_and_dir_t m_0;
            price_and_dir_t m_1;
        };

        uint64_t time;
        price_and_dir_t va_0;
        price_and_dir_t va_1;
        price_and_dir_t va_2;
        median_t median_w;
        median_t median_a;
        double atr;
    };

    what_t what;
    tick_t tick;
    signal_t signal;
};

#pragma pack(pop)
