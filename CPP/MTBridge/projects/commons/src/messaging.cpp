#include "messaging.hpp"
#include <zmq.h>

namespace commons::messaging
{
    socket_t::socket_t(context_t& ctx, type_t type)
    : m_type(type), m_dont_wait(false), m_zmq_socket(zmq_socket(ctx.m_zmq_ctx, m_type_map[(size_t) type]))
    { }

    socket_t::socket_t(socket_t&& socket) noexcept
    : m_type(socket.m_type),
      m_dont_wait(std::exchange(socket.m_dont_wait,false)),
      m_zmq_socket(std::exchange(socket.m_zmq_socket, nullptr))
    { }

    socket_t::~socket_t()
    {
        if (m_zmq_socket != nullptr)
        {
            zmq_close(m_zmq_socket);
            m_zmq_socket = nullptr;
        }
    }

    socket_t::type_t socket_t::type()
    {
        return m_type;
    }

    bool socket_t::dont_wait() const
    {
        return m_dont_wait;
    }

    void socket_t::dont_wait(bool value)
    {
        m_dont_wait = value;
    }

    ret<> socket_t::bind(const string& endpoint)
    {
        return zmq_bind(m_zmq_socket, endpoint.c_str()) == 0 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<> socket_t::connect(const string& endpoint)
    {
        return zmq_connect(m_zmq_socket, endpoint.c_str()) == 0 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<> socket_t::disconnect(const string& endpoint)
    {
        return zmq_disconnect(m_zmq_socket, endpoint.c_str()) == 0 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<size_t> socket_t::recv(void * buffer, size_t size)
    {
        size_t count = zmq_recv(m_zmq_socket, buffer, size, m_dont_wait ? ZMQ_DONTWAIT : 0);
        return count != -1 ? ret<size_t>::done(count) : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<size_t> socket_t::recv(void * buffer, size_t size, bool& have_more)
    {
        size_t count = zmq_recv(m_zmq_socket, buffer, size, m_dont_wait ? ZMQ_DONTWAIT : 0);
        int more = 0; size_t more_size = sizeof(more);
        int gso_ret = zmq_getsockopt(m_zmq_socket, ZMQ_RCVMORE, &more, &more_size);
        return count != -1 && gso_ret != -1 ? (have_more = more != 0, ret<size_t>::done(count)) : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<size_t> socket_t::send(const void * data, size_t size, bool send_more)
    {
        size_t count = zmq_send(m_zmq_socket, (void *) data, size, (m_dont_wait ? ZMQ_DONTWAIT : 0) | (send_more ? ZMQ_SNDMORE : 0));
        return count != -1 ? ret<size_t>::done(count) : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<> socket_t::subscribe(const void * data, size_t size)
    {
        int sso_ret = zmq_setsockopt(m_zmq_socket, ZMQ_SUBSCRIBE, data, size);
        return sso_ret != -1 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<> socket_t::unsubscribe(const void * data, size_t size)
    {
        int sso_ret = zmq_setsockopt(m_zmq_socket, ZMQ_UNSUBSCRIBE, data, size);
        return sso_ret != -1 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<int> socket_t::get_linger()
    {
        int value = 0; size_t value_size = sizeof(value);
        int gso_ret = zmq_getsockopt(m_zmq_socket, ZMQ_LINGER, &value, &value_size);
        return gso_ret != -1 ? ret<int>::done(value) : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<> socket_t::set_linger(int value)
    {
        int sso_ret = zmq_setsockopt(m_zmq_socket, ZMQ_LINGER, &value, sizeof(value));
        return sso_ret != -1 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<bool> socket_t::get_immediate()
    {
        int value = 0; size_t value_size = sizeof(value);
        int gso_ret = zmq_getsockopt(m_zmq_socket, ZMQ_IMMEDIATE, &value, &value_size);
        return gso_ret != -1 ? ret<bool>::done(bool(value)) : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<> socket_t::set_immediate(bool value)
    {
        int val = int(value);
        int sso_ret = zmq_setsockopt(m_zmq_socket, ZMQ_IMMEDIATE, &val, sizeof(val));
        return sso_ret != -1 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<int> socket_t::get_connect_timeout()
    {
        int value = 0; size_t value_size = sizeof(value);
        int gso_ret = zmq_getsockopt(m_zmq_socket, ZMQ_CONNECT_TIMEOUT, &value, &value_size);
        return gso_ret != -1 ? ret<int>::done(value) : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<> socket_t::set_connect_timeout(int value)
    {
        int sso_ret = zmq_setsockopt(m_zmq_socket, ZMQ_CONNECT_TIMEOUT, &value, sizeof(value));
        return sso_ret != -1 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<int> socket_t::get_reconnect_ivl()
    {
        int value = 0; size_t value_size = sizeof(value);
        int gso_ret = zmq_getsockopt(m_zmq_socket, ZMQ_RECONNECT_IVL, &value, &value_size);
        return gso_ret != -1 ? ret<int>::done(value) : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<> socket_t::set_reconnect_ivl(int value)
    {
        int sso_ret = zmq_setsockopt(m_zmq_socket, ZMQ_RECONNECT_IVL, &value, sizeof(value));
        return sso_ret != -1 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<int> socket_t::get_reconnect_ivl_max()
    {
        int value = 0; size_t value_size = sizeof(value);
        int gso_ret = zmq_getsockopt(m_zmq_socket, ZMQ_RECONNECT_IVL_MAX, &value, &value_size);
        return gso_ret != -1 ? ret<int>::done(value) : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<> socket_t::set_reconnect_ivl_max(int value)
    {
        int sso_ret = zmq_setsockopt(m_zmq_socket, ZMQ_RECONNECT_IVL_MAX, &value, sizeof(value));
        return sso_ret != -1 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<> socket_t::set_router_mandatory(bool value)
    {
        int val = int(value);
        int sso_ret = zmq_setsockopt(m_zmq_socket, ZMQ_ROUTER_MANDATORY, &val, sizeof(val));
        return sso_ret != -1 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    int socket_t::m_type_map[] =
    {
        ZMQ_PUB,
        ZMQ_SUB,
        ZMQ_XPUB,
        ZMQ_XSUB,
        ZMQ_PUSH,
        ZMQ_PULL,
        ZMQ_PAIR,
        ZMQ_REQ,
        ZMQ_REP,
        ZMQ_DEALER,
        ZMQ_ROUTER
    };

    context_t::context_t() : m_zmq_ctx(zmq_ctx_new()), m_call_term(true) { }

    context_t::context_t(context_t&& ctx) noexcept
    : m_zmq_ctx(std::exchange(ctx.m_zmq_ctx, nullptr)),
      m_call_term(std::exchange(ctx.m_call_term, false))
    { }

    bool context_t::is_void() { return m_zmq_ctx == nullptr; }

    ret<> context_t::set_block(bool value)
    {
        int s_ret = zmq_ctx_set(m_zmq_ctx, ZMQ_BLOCKY, int(value));
        return s_ret != -1 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    ret<> context_t::set_io_threads(int value)
    {
        int s_ret = zmq_ctx_set(m_zmq_ctx, ZMQ_IO_THREADS, value);
        return s_ret != -1 ? ret<>::done() : exc(zmq_errno(), zmq_strerror(zmq_errno()));
    }

    void context_t::set_call_term(bool value)
    {
        m_call_term = value;
    }

    socket_t context_t::open_socket(socket_t::type_t type)
    {
        return socket_t(*this, type);
    }

    context_t::~context_t()
    {
        if (m_zmq_ctx != nullptr && m_call_term) zmq_ctx_term(m_zmq_ctx);
    }

    io_poll::item_t::item_t(socket_t& socket, bool query_recv, bool query_send)
    : m_socket(socket), m_query_recv(query_recv), m_query_send(query_send), m_can_recv(false), m_can_send(false) { }

    bool io_poll::item_t::can_recv() const
    {
        return m_can_recv;
    }

    bool io_poll::item_t::can_send() const
    {
        return m_can_send;
    }

    io_poll::io_poll() : m_impl_items(nullptr) { }

    io_poll::~io_poll()
    {
        if (m_impl_items) delete [] (zmq_pollitem_t *) m_impl_items;
    }

    io_poll& io_poll::add(io_poll::item_t& item)
    {
        m_item_ptrs.push_back(&item);
        update_impl_items();
        return *this;
    }

    io_poll& io_poll::remove(io_poll::item_t& item)
    {
        for (auto it = m_item_ptrs.begin() ; it != m_item_ptrs.end() ; )
            *it == &item ? m_item_ptrs.erase(it) : ++ it;
        update_impl_items();
        return *this;
    }

    void io_poll::poll(int64_t timeout_ms)
    {
        if (!m_impl_items) return;
        zmq_poll((zmq_pollitem_t *) m_impl_items, (int) m_item_ptrs.size(), (long) timeout_ms);
        update_items();
    }

    void io_poll::update_impl_items()
    {
        size_t size = m_item_ptrs.size();
        auto * impl_items = new zmq_pollitem_t[size];
        for (size_t i = 0 ; i < size ; i ++)
        {
            item_t* item_ptr = m_item_ptrs[i];
            auto& impl_item = impl_items[i];
            impl_item.socket = item_ptr->m_socket.m_zmq_socket;
            impl_item.events = short((item_ptr->m_query_recv ? ZMQ_POLLIN : 0) | (item_ptr->m_query_send ? ZMQ_POLLOUT : 0));
        }
        if (m_impl_items) delete [] (zmq_pollitem_t *) m_impl_items;
        m_impl_items = impl_items;
    }

    void io_poll::update_items()
    {
        size_t size = m_item_ptrs.size();
        auto * impl_items = (zmq_pollitem_t *) m_impl_items;
        for (size_t i = 0 ; i < size ; i ++)
        {
            item_t* item_ptr = m_item_ptrs[i];
            auto& impl_item = impl_items[i];
            item_ptr->m_can_recv = (impl_item.revents & ZMQ_POLLIN) != 0;
            item_ptr->m_can_send = (impl_item.revents & ZMQ_POLLOUT) != 0;
        }
    }
}
