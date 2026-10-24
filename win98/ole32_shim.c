/* ole32 shim: CoInitializeEx / CoCreateFreeThreadedMarshaler for Win98.
 * Win98 OLE32 does export CoInitialize but not CoInitializeEx. */
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <objbase.h>

BOOL WINAPI DllMain(HINSTANCE h, DWORD r, LPVOID p) { (void)h;(void)r;(void)p; return TRUE; }

__declspec(dllexport) HRESULT WINAPI CoInitializeEx(LPVOID reserved, DWORD coinit)
{
    (void) reserved;
    /* Win98 has no apartment threading; COINIT_APARTMENTTHREADED maps to CoInitialize */
    if (coinit & COINIT_MULTITHREADED) {
        /* no MTA support: report success so callers proceed on STA */
        return CoInitialize (NULL);
    }
    return CoInitialize (NULL);
}

__declspec(dllexport) HRESULT WINAPI CoCreateFreeThreadedMarshaler(LPUNKNOWN outer, LPUNKNOWN *marshaller)
{
    (void) outer;
    if (!marshaller) return E_INVALIDARG;
    *marshaller = NULL;
    return E_NOTIMPL;
}
