#pragma once

#include "sys.hpp"

namespace commons
{
    template<typename value_tp = void, typename _enable_ = void>
    class ret { };

    template<typename value_tp> using _should_copy_value_ =
        typename std::enable_if<std::is_copy_constructible<value_tp>::value>::type;
    template<typename value_tp> using _should_move_value_ =
        typename std::enable_if<!std::is_copy_constructible<value_tp>::value&& std::is_move_constructible<value_tp>::value>::type;

    template<typename value_tp>
    class ret<value_tp, _should_copy_value_<value_tp>>
    {
    public:
        using value_t = value_tp;
        using error_t = std::exception_ptr;

    public:
        ret() noexcept :
            m_status(false),
            m_value(),
            m_error()
        { }

        ret(const ret& r) noexcept :
            m_status(r.m_status),
            m_value(r.m_value),
            m_error(r.m_error)
        { }

        ret(const value_tp& p_value) noexcept :
            m_status(true),
            m_value(p_value),
            m_error()
        { }

        ret(error_t p_error) noexcept :
            m_status(false),
            m_value(),
            m_error(p_error)
        { }

        template<typename exc_tp, std::enable_if_t<std::is_base_of<std::exception, exc_tp>::value, bool> = true>
        ret(const exc_tp& p_exc) noexcept :
            m_status(false),
            m_value(),
            m_error(std::make_exception_ptr(p_exc))
        { }

    public:
        static ret done(const value_tp& p_value) { return ret(p_value); }

        static ret fail(bool capture = false) { return capture ? ret(std::current_exception()) : ret(); }

        static ret fail(const error_t& p_error) { return ret(p_error); }

        template<typename exc_tp, std::enable_if_t<std::is_base_of<std::exception, exc_tp>::value, bool> = true>
        static ret fail(const exc_tp& p_exc) { return ret(p_exc); }

        template<typename any_value_tp>
        static ret fail(const ret<any_value_tp>& p_ret) { return ret(p_ret.error()); }

        operator bool() const { return m_status; }

        value_tp& value() { return m_value; }

        value_tp& operator * () { return m_value; }

        bool has_error() const { return bool(m_error); }

        error_t error() const { return m_error; }

        template<typename func_tp>
        typename std::invoke_result<func_tp, const std::exception&>::type error(func_tp&& func)
        {
            try { std::rethrow_exception(m_error); }
            catch (const std::exception& e) { return func(e); }
        }

    private:
        bool m_status;
        value_tp m_value;
        error_t m_error;
    };

    template<typename value_tp>
    class ret<value_tp, _should_move_value_<value_tp>>
    {
    public:
        using value_t = value_tp;
        using error_t = std::exception_ptr;

    public:
        ret() noexcept :
            m_status(false),
            m_value(),
            m_error()
        { }

        ret(ret&& r) noexcept :
            m_status(std::exchange(r.m_status, false)),
            m_value(std::move(r.m_value)),
            m_error(std::move(r.m_error))
        { }

        ret(value_tp&& p_value) noexcept :
            m_status(true),
            m_value(std::move(p_value)),
            m_error()
        { }

        ret(const error_t& p_error) noexcept :
            m_status(false),
            m_value(),
            m_error(p_error)
        { }

        template<typename exc_tp, std::enable_if_t<std::is_base_of<std::exception, exc_tp>::value, bool> = true>
        ret(const exc_tp& p_exc) noexcept :
            m_status(false),
            m_value(),
            m_error(std::make_exception_ptr(p_exc))
        { }

    public:
        static ret done(value_tp&& p_value) { return ret(std::move(p_value)); }

        static ret fail(bool capture = false) { return capture ? ret(std::current_exception()) : ret(); }

        static ret fail(const error_t& p_error) { return ret(p_error); }

        template<typename exc_tp, std::enable_if_t<std::is_base_of<std::exception, exc_tp>::value, bool> = true>
        static ret fail(const exc_tp& p_exc) { return ret(p_exc); }

        template<typename any_value_tp>
        static ret fail(const ret<any_value_tp>& p_ret) { return ret(p_ret.error()); }

        operator bool() const { return m_status; }

        value_tp& value() { return m_value; }

        value_tp& operator * () { return m_value; }

        bool has_error() const { return bool(m_error); }

        error_t error() const { return m_error; }

        template<typename func_tp>
        typename std::invoke_result<func_tp, const std::exception&>::type error(func_tp&& func)
        {
            try { std::rethrow_exception(m_error); }
            catch (const std::exception& e) { return func(e); }
        }

    private:
        ret pass() { return std::move(*this); }

    private:
        bool m_status;
        value_tp m_value;
        error_t m_error;
    };

    template<>
    class ret<void>
    {
    public:
        using error_t = std::exception_ptr;

    public:
        ret() noexcept :
            m_status(false),
            m_error()
        { }

        ret(const ret& r) noexcept :
            m_status(r.m_status),
            m_error(r.m_error)
        { }

        ret(bool p_status) noexcept :
            m_status(p_status),
            m_error()
        { }

        ret(const error_t& p_error) noexcept :
            m_status(false),
            m_error(p_error)
        { }

        template<typename exc_tp, std::enable_if_t<std::is_base_of<std::exception, exc_tp>::value, bool> = true>
        ret(const exc_tp& p_exc) noexcept :
            m_status(false),
            m_error(std::make_exception_ptr(p_exc))
        { }

    public:
        static ret done() { return ret(true); }

        static ret fail(bool capture = false) { return capture ? ret(std::current_exception()) : ret(); }

        static ret fail(const error_t& p_error) { return ret(p_error); }

        template<typename exc_tp, std::enable_if_t<std::is_base_of<std::exception, exc_tp>::value, bool> = true>
        static ret fail(const exc_tp& p_exc) { return ret(p_exc); }

        template<typename any_value_tp>
        static ret fail(const ret<any_value_tp>& p_ret) { return ret(p_ret.error()); }

        operator bool() const { return m_status; }

        bool has_error() const { return bool(m_error); }

        error_t error() const { return m_error; }

        template<typename func_tp>
        typename std::invoke_result<func_tp, const std::exception&>::type error(func_tp&& func)
        {
            try { std::rethrow_exception(m_error); }
            catch (const std::exception& e) { return func(e); }
        }

    private:
        bool m_status;
        error_t m_error;
    };
}
