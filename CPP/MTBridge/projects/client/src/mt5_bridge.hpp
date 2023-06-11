#pragma once

#include <sys.hpp>
#include <result.hpp>
#include <messaging.hpp>
#include <vo.hpp>

using namespace commons;

class mt5_bridge
{
public:
    mt5_bridge() noexcept;

public:
    bool init(const wstring& service_uri, const wstring& symbol, const data_params_t& params, int32_t utc_shift);
    bool query_data(bool& have_data, market_data_t& data);
    bool shutdown();
    void get_last_message(int& err_no, std::wstring& msg);

private:
    class local_state
    {
    public:
        ret<> status;
        messaging::context_t ctx;
        messaging::socket_t socket;
        messaging::io_poll polling;
        messaging::io_poll::item_t pi;
        int32_t utc_shift;

    public:
        local_state(int32_t p_utc_shift);
    };

private:
    std::wstring_convert<std::codecvt_utf8<wchar_t>, wchar_t> m_utf8_cvt;

private:
    static thread_local local_state * m_local_ptr;

private:
    static constexpr int sock_connect_timeout_ms = 2000;
    static constexpr int sock_reconnect_ivl_ms = 500;
    static constexpr int sock_reconnect_ivl_max_ms = 2000;
};
