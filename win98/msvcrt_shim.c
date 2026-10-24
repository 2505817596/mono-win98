/* msvcrt shim: _ecvt_s (secure CRT, not present in Win98 msvcrt 6.10). */
#include <windows.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>

BOOL WINAPI DllMain(HINSTANCE h, DWORD r, LPVOID p) { (void)h;(void)r;(void)p; return TRUE; }

/* _ecvt_s(buffer, size, value, ndigits, decpt, sign) */
__declspec(dllexport) int _ecvt_s(char *buf, size_t size, double value, int ndigits, int *decpt, int *sign)
{
    char tmp[64];
    char *r = _ecvt (value, ndigits, decpt, sign);
    if (!r) return 22;              /* EINVAL */
    strncpy (tmp, r, sizeof (tmp) - 1);
    tmp[sizeof (tmp) - 1] = 0;
    if (strlen (tmp) + 1 > size) return 34;   /* ERANGE */
    strcpy (buf, tmp);
    return 0;
}
