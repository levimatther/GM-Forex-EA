#include "mt5_bridge.hpp"

bool mt5_bridge::init(const string& service_uri)
{
    if (m_local_ptr == nullptr) m_local_ptr = new local_state();
    if (auto bind_r = m_local_ptr->socket.bind(service_uri))
        return true;
    else
        m_local_ptr->status = ret<>::fail(bind_r);
    return false;
}

bool mt5_bridge::poll_request(bool& have_request, std::basic_string<byte_t>& client_id, std::basic_string<byte_t>& symbol, data_params_t& data_params)
{
    m_local_ptr->polling.poll(0);
    have_request = m_local_ptr->pi.can_recv();
    if (have_request)
    {
        bool have_more;
        auto buffer = m_local_ptr->serv_buffer;
        if (auto recv_r = m_local_ptr->socket.recv(m_local_ptr->serv_buffer.data(), buffer.size(), have_more))
        {
            client_id = std::basic_string<byte_t>(m_local_ptr->serv_buffer.data(), std::min(buffer.size(), *recv_r));
            if (have_more)
            {
                if ((recv_r = m_local_ptr->socket.recv(m_local_ptr->serv_buffer.data(), buffer.size(), have_more)))
                {
                    symbol = std::basic_string<byte_t>(m_local_ptr->serv_buffer.data(), std::min(buffer.size(), *recv_r));
                    if (have_more)
                    {
                        if ((recv_r = m_local_ptr->socket.recv(&data_params, sizeof(data_params))))
                            return true;
                        else
                            m_local_ptr->status = ret<>::fail(recv_r);
                    }
                    else
                        m_local_ptr->status = ret<>::fail();
                }
                else
                    m_local_ptr->status = ret<>::fail(recv_r);
            }
            else
                m_local_ptr->status = ret<>::fail();
        }
        else
            m_local_ptr->status = ret<>::fail(recv_r);
        return false;
    }
    return true;
}

bool mt5_bridge::send_data(const std::basic_string<byte_t>& client_id, const market_data_t& data)
{
    if (auto send_r = m_local_ptr->socket.send(client_id.data(), client_id.size(), true))
    {
        if ((send_r = m_local_ptr->socket.send(&data, sizeof(data))))
            return true;
        else
            m_local_ptr->status = ret<>::fail(send_r);
    }
    else
        m_local_ptr->status = ret<>::fail(send_r);
    return false;
}

bool mt5_bridge::shutdown()
{
    delete m_local_ptr; m_local_ptr = nullptr;
    return true;
}

void mt5_bridge::get_last_message(int& err_no, string& msg)
{
    err_no = 0; msg = "OK"s;
    auto fetch_err = [this, &err_no, &msg](const auto& e) -> void
    {
        err_no = ((const messaging::exc&) e).err_code();
        msg = e.what();
    };
    if (m_local_ptr != nullptr && m_local_ptr->status.has_error())
    {
        m_local_ptr->status.error(fetch_err);
        m_local_ptr->status = ret<>::done();
    }
}

mt5_bridge::local_state::local_state()
: status(ret<>::done()),
  socket(ctx.open_socket(messaging::socket_t::type_t::router)),
  pi(socket, true, false),
  serv_buffer()
{
    socket.set_linger(0);
    polling.add(pi);
}
