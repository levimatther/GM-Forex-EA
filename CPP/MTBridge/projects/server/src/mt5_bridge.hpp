#pragma once

#include <sys.hpp>
#include <result.hpp>
#include <messaging.hpp>
#include <vo.hpp>

using namespace commons;

class mt5_bridge
{
public:
    mt5_bridge() noexcept = default;

public:
    bool init(const string& service_uri);
    bool poll_request(bool& have_request, std::basic_string<byte_t>& client_id, std::basic_string<byte_t>& symbol, data_params_t& data_params);
    bool send_data(const std::basic_string<byte_t>& client_id, const market_data_t& data);
    bool shutdown();
    void get_last_message(int& err_no, string& msg);

private:
    static constexpr size_t serv_buffer_capacity = 256;

private:
    class local_state
    {
    public:
        ret<> status;
        messaging::context_t ctx;
        messaging::socket_t socket;
        messaging::io_poll polling;
        messaging::io_poll::item_t pi;
        std::array<byte_t, serv_buffer_capacity> serv_buffer;

    public:
        local_state();
    };

private:
    local_state * m_local_ptr;
};
