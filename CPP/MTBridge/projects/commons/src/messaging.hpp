#pragma once

#include "sys.hpp"
#include "result.hpp"

namespace commons::messaging
{
    class context_t;
    class socket_t;
    class io_poll;

    class socket_t final
    {
        friend class context_t;
        friend class io_poll;

    public:
        enum class type_t : uint32_t { pub, sub, xpub, xsub, push, pull, pair, req, rep, dealer, router };

    public:
        ~socket_t();
        socket_t(const socket_t&) = delete;
        socket_t(socket_t&& socket) noexcept;
        socket_t& operator = (const socket_t&) = delete;
        type_t type();
        bool dont_wait() const;
        void dont_wait(bool value);
        ret<> bind(const string& endpoint);
        ret<> connect(const string& endpoint);
        ret<> disconnect(const string& endpoint);
        ret<size_t> recv(void * buffer, size_t size);
        ret<size_t> recv(void * buffer, size_t size, bool& have_more);
        ret<size_t> send(const void * data, size_t size, bool send_more = false);
        ret<> subscribe(const void * data, size_t size);
        ret<> unsubscribe(const void * data, size_t size);
        ret<int> get_linger();
        ret<> set_linger(int value);
        ret<bool> get_immediate();
        ret<> set_immediate(bool value);
        ret<int> get_connect_timeout();
        ret<> set_connect_timeout(int value);
        ret<int> get_reconnect_ivl();
        ret<> set_reconnect_ivl(int value);
        ret<int> get_reconnect_ivl_max();
        ret<> set_reconnect_ivl_max(int value);
        ret<> set_router_mandatory(bool value);
    private:
        socket_t(context_t& ctx, type_t type);

    private:
        type_t m_type;
        bool m_dont_wait;
        void * m_zmq_socket;

    private:
        static constexpr size_t types_count = 11;
        static int m_type_map[types_count];
    };

    class context_t final
    {
        friend class socket_t;

    public:
        context_t();
        context_t(const context_t&) = delete;
        context_t(context_t&& ctx) noexcept;
        ~context_t();
        context_t& operator = (const context_t&) = delete;
        bool is_void();
        ret<> set_block(bool value);
        ret<> set_io_threads(int value);
        void set_call_term(bool value);
        socket_t open_socket(socket_t::type_t type);

    private:
        void * m_zmq_ctx;
        bool m_call_term;
    };

    class io_poll final
    {
    public:
        class item_t final
        {
            friend class io_poll;

        public:
            explicit item_t(socket_t& socket, bool query_recv, bool query_send);
            bool can_recv() const;
            bool can_send() const;

        private:
            socket_t& m_socket;
            const bool m_query_recv;
            const bool m_query_send;
            bool m_can_recv;
            bool m_can_send;
        };

    public:
        io_poll();
        ~io_poll();
        io_poll& add(item_t& item);
        io_poll& remove(item_t& item);
        void poll(int64_t timeout_ms);

    private:
        void update_impl_items();
        void update_items();

    private:
        std::vector<item_t*> m_item_ptrs;
        void * m_impl_items;
    };

    class exc : public std::runtime_error
    {
    public:
        exc() : std::runtime_error(""), m_err_code(0) { }
        exc(int p_err_code, const string& msg) : std::runtime_error(msg), m_err_code(p_err_code) { }
        int err_code() const { return m_err_code; }

    private:
        int m_err_code;
    };
}
