#pragma once

#include "sys.hpp"

namespace commons
{
    template<typename result_tp = void, typename ...args_tps>
    class worker
    {
    public:
        worker() : m_stop_request(false) { };

        bool stop()
        { 
            auto signal_stop = [this]() -> bool
            {
                bool proceed;
                {
                    std::lock_guard<std::mutex> guard(m_mutex); 
                    m_stop_request.store(proceed = on_stop());
                }
                m_cv.notify_one();
                return proceed;
            };
            return signal_stop();
        }

        result_tp wait()
        { 
            if (m_thread.joinable()) m_thread.join();
            auto result_f = m_promise.get_future();
            return result_f.get();
        };

    protected:
        void start_new(args_tps&&... args) { m_thread = std::thread(&worker::run_proc, this, std::forward<args_tps>(args)...); }

        void start_in_this(args_tps&&... args) { run_proc(std::forward<args_tps>(args)...); }
        
        virtual result_tp run(args_tps&&... args) = 0;
        
        virtual bool on_stop() { return true; }
        
        bool is_stop_requested() { return m_stop_request.load(); }

        void wait_for_stop()
        {
            std::unique_lock<std::mutex> lock(m_mutex);
            m_cv.wait(lock, [this] { return m_stop_request.load(); });
        }

        template<typename f_tp>
        typename std::invoke_result<f_tp>::type exec_sync(f_tp func)
        {
            std::lock_guard<std::mutex> guard(m_mutex);
            return func();
        }

    private:
        void run_proc(args_tps&&... args)
        {
            try
            {
                result_tp result = run(std::forward<args_tps>(args)...);
                m_promise.set_value(result);
            }
            catch (...)
            {
                m_promise.set_exception(std::current_exception());
            }
        };

    private:
        std::thread m_thread;
        std::mutex m_mutex;
        std::condition_variable m_cv;
        std::atomic_bool m_stop_request;
        std::promise<result_tp> m_promise;
    };

    template<typename ...args_tps>
    class worker<void, args_tps...>
    {
    public:
        worker() : m_stop_request(false) { };

        bool stop()
        { 
            auto signal_stop = [this]() -> bool
            {
                bool proceed;
                {
                    std::lock_guard<std::mutex> guard(m_mutex); 
                    m_stop_request.store(proceed = on_stop());
                }
                m_cv.notify_one();
                return proceed;
            };
            return signal_stop();
        }

        void wait()
        {
            if (m_thread.joinable()) m_thread.join();
            auto result_f = m_promise.get_future();
            return result_f.get();
        };

    protected:
        void start_new(args_tps&&... args) { m_thread = std::thread(&worker::run_proc, this, std::forward<args_tps>(args)...); }

        void start_in_this(args_tps&&... args) { run_proc(std::forward<args_tps>(args)...); }
        
        virtual void run(args_tps&&... args) = 0;
        
        virtual bool on_stop() { return true; }
        
        bool is_stop_requested() { return m_stop_request.load(); }

        void wait_for_stop()
        {
            std::unique_lock<std::mutex> lock(m_mutex);
            m_cv.wait(lock, [this] { return m_stop_request.load(); });
        }

        template<typename f_tp>
        typename std::invoke_result<f_tp>::type exec_sync(f_tp func)
        {
            std::lock_guard<std::mutex> guard(m_mutex);
            return func();
        }

    private:
        void run_proc(args_tps&&... args)
        {
            try
            {
                run(std::forward<args_tps>(args)...);
                m_promise.set_value();
            }
            catch (...)
            {
                m_promise.set_exception(std::current_exception());
            }
        };

    private:
        std::thread m_thread;
        std::mutex m_mutex;
        std::condition_variable m_cv;
        std::atomic_bool m_stop_request;
        std::promise<void> m_promise;
    };
}
