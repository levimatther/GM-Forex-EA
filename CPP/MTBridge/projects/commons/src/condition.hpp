#pragma once

#include "sys.hpp"

namespace commons
{
    class condition
    {
    public:
        condition() : m_status(false) { };

        operator bool () const
        {
            std::unique_lock<std::mutex> lock(m_mutex);
            return m_status;
        }

        void wait() const
        {
            std::unique_lock<std::mutex> lock(m_mutex);
            m_cv.wait(lock, [this] { return m_status; });
        }

        void set(bool p_notify_all = true)
        { 
            {
                std::lock_guard<std::mutex> guard(m_mutex); 
                m_status = true;
            }
            p_notify_all ? m_cv.notify_all() : m_cv.notify_one();
        }

    private:
        mutable std::mutex m_mutex;
        mutable std::condition_variable m_cv;
        bool m_status;
    };
}
