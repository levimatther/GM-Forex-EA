#include "mt5_bridge.hpp"

mt5_bridge::mt5_bridge() noexcept
{
    setlocale(LC_ALL, ".utf8");
}

bool mt5_bridge::init(const wstring& service_uri, const wstring& symbol, const data_params_t& params, int32_t utc_shift)
{
    if (m_local_ptr == nullptr) m_local_ptr = new local_state(utc_shift);
    if (auto connect_r = m_local_ptr->socket.connect(m_utf8_cvt.to_bytes(service_uri)))
    {
        string symbol_bytes(m_utf8_cvt.to_bytes(symbol));
        if (auto send_r = m_local_ptr->socket.send(symbol_bytes.data(), symbol_bytes.size(), true))
        {
            if ((send_r = m_local_ptr->socket.send(&params, sizeof(params))))
                return true;
            else
                m_local_ptr->status = ret<>::fail(send_r);
        }
        else
            m_local_ptr->status = ret<>::fail(send_r);
    }
    else
        m_local_ptr->status = ret<>::fail(connect_r);
    return false;
}

bool mt5_bridge::query_data(bool& have_data, market_data_t& data)
{
    m_local_ptr->polling.poll(0);
    have_data = m_local_ptr->pi.can_recv();
    if (have_data)
    {
        if (auto recv_r = m_local_ptr->socket.recv(&data, sizeof(data)))
        {
            if ((uint32_t(data.what) & uint32_t(market_data_t::what_t::tick)) != 0)
                data.tick.time += m_local_ptr->utc_shift;
            if ((uint32_t(data.what) & uint32_t(market_data_t::what_t::signal)) != 0)
                data.signal.time += m_local_ptr->utc_shift;
            return true;
        }
        else
            m_local_ptr->status = ret<>::fail(recv_r);
        return false;
    }
    return true;
}

bool mt5_bridge::shutdown()
{
    delete m_local_ptr; m_local_ptr = nullptr;
    return true;
}

void mt5_bridge::get_last_message(int& err_no, std::wstring& msg)
{
    err_no = 0; msg = L"OK";
    auto fetch_err = [this, &err_no, &msg](const auto& e) -> void
    {
        err_no = ((const messaging::exc&) e).err_code();
        msg = m_utf8_cvt.from_bytes(e.what());
    };
    if (m_local_ptr != nullptr && m_local_ptr->status.has_error())
    {
        m_local_ptr->status.error(fetch_err);
        m_local_ptr->status = ret<>::done();
    }
}

mt5_bridge::local_state::local_state(int32_t p_utc_shift)
: status(ret<>::done()),
  socket(ctx.open_socket(messaging::socket_t::type_t::dealer)),
  pi(socket, true, false),
  utc_shift(p_utc_shift)
{
    socket.set_linger(0);
    socket.set_connect_timeout(sock_connect_timeout_ms);
    socket.set_reconnect_ivl(sock_reconnect_ivl_ms);
    socket.set_reconnect_ivl_max(sock_reconnect_ivl_max_ms);
    polling.add(pi);
}

thread_local mt5_bridge::local_state * mt5_bridge::m_local_ptr;
