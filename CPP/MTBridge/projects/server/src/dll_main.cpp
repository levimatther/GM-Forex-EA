#include <sys.hpp>
#include "mt5_bridge.hpp"

mt5_bridge mt5;

BOOL WINAPI DllMain([[maybe_unused]] _In_ HINSTANCE hinstDLL, [[maybe_unused]] _In_ DWORD fdwReason, [[maybe_unused]] _In_ LPVOID lpvReserved)
{
    return TRUE;
}


#pragma pack(push, 1)

struct raw_buffer
{
    void * const data_ptr;
    const uint32_t buffer_size;
    uint32_t data_size;
};

#pragma pack(pop)


extern "C"
{
    __declspec(dllexport) bool __stdcall MT5S_Init(const char * service_uri)
    {
        return mt5.init(service_uri);
    }

    __declspec(dllexport) bool __stdcall MT5S_PollRequest
        (
            bool * have_request_ptr,
            raw_buffer * client_id_ptr,
            raw_buffer * symbol_ptr,
            data_params_t * data_params_ptr
        )
    {
        std::basic_string<byte_t> client_id;
        std::basic_string<byte_t> symbol;
        bool success = mt5.poll_request(*have_request_ptr, client_id, symbol, *data_params_ptr);
        if (success && *have_request_ptr)
        {
            client_id_ptr->data_size = uint32_t(client_id.size());
            std::memcpy(client_id_ptr->data_ptr, client_id.data(), std::min(client_id_ptr->buffer_size, client_id_ptr->data_size));
            symbol_ptr->data_size = uint32_t(symbol.size());
            std::memcpy(symbol_ptr->data_ptr, symbol.data(), std::min(symbol_ptr->buffer_size, symbol_ptr->data_size));
        }
        return success;
    }

    __declspec(dllexport) bool __stdcall MT5S_SendData(const raw_buffer * client_id_ptr, const market_data_t * data)
    {
        std::basic_string<byte_t> client_id((const byte_t *) client_id_ptr->data_ptr, client_id_ptr->data_size);
        return mt5.send_data(client_id, *data);
    }

    __declspec(dllexport) bool __stdcall MT5S_Shutdown()
    {
        return mt5.shutdown();
    }

    __declspec(dllexport) void __stdcall MT5S_GetLastMessage(int * err_no_ptr, raw_buffer * msg_ptr)
    {
        string msg;
        mt5.get_last_message(*err_no_ptr, msg);
        msg_ptr->data_size = uint32_t(msg.size());
        std::memcpy(msg_ptr->data_ptr, msg.data(), std::min(msg_ptr->buffer_size, msg_ptr->data_size));
    }
}
