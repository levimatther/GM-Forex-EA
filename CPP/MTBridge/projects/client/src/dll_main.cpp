#include <sys.hpp>
#include "mt5_bridge.hpp"

mt5_bridge mt5;

BOOL WINAPI DllMain([[maybe_unused]] _In_ HINSTANCE hinstDLL, [[maybe_unused]] _In_ DWORD fdwReason, [[maybe_unused]] _In_ LPVOID lpvReserved)
{
    return TRUE;
}

extern "C"
{
    __declspec(dllexport) bool __stdcall MT5C_Init(const wchar_t * service_uri, const wchar_t * symbol, const data_params_t * params, int32_t utc_shift)
    {
        return mt5.init(wstring(service_uri), wstring(symbol), *params, utc_shift);
    }

    __declspec(dllexport) bool __stdcall MT5C_QueryData(bool * have_data, market_data_t * data)
    {
        return mt5.query_data(*have_data, *data);
    }

    __declspec(dllexport) bool __stdcall MT5C_Shutdown()
    {
        return mt5.shutdown();
    }

    __declspec(dllexport) bool __stdcall MT5C_GetLastMessage(int * err_no, wchar_t * buffer, uint32_t size)
    {
        wstring msg;
        mt5.get_last_message(*err_no, msg);
        return msg.size() < size && wcscpy_s(buffer, size, msg.c_str()) == 0;
    }
}
