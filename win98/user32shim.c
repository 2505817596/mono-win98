#include <windows.h>
#include <stdio.h>
#include <stdarg.h>

BOOL WINAPI DllMain(HINSTANCE h, DWORD r, LPVOID p) { (void)h; (void)r; (void)p; return TRUE; }

/* Win9x: Mono passes an empty menu name (""), which MSLU converts to an empty
 * ANSI name; RegisterClassA then tries to load a menu called "" and fails.
 * Convert explicitly and map empty strings to NULL. */
static void shim_log (const char *fmt, ...);

ATOM WINAPI RegisterClassW (const WNDCLASSW *wc)
{
    WNDCLASSA a;
    char menu[256];
    char cls[256];
    ATOM r;
    DWORD e;
    if (!wc) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    ZeroMemory (&a, sizeof (a));
    /* Win98 rejects unknown class styles (e.g. XP+'s CS_DROPSHADOW, 0x20000)
     * by failing RegisterClass with no error code.  Strip it. */
    a.style = wc->style & ~0x00020000;
    a.lpfnWndProc = wc->lpfnWndProc;
    a.cbClsExtra = wc->cbClsExtra;
    a.cbWndExtra = wc->cbWndExtra;
    a.hInstance = wc->hInstance;
    a.hIcon = wc->hIcon;
    a.hCursor = wc->hCursor;
    a.hbrBackground = wc->hbrBackground;
    if (wc->lpszMenuName && wc->lpszMenuName[0]) {
        WideCharToMultiByte (CP_ACP, 0, wc->lpszMenuName, -1, menu, sizeof (menu), NULL, NULL);
        a.lpszMenuName = menu;
    } else {
        a.lpszMenuName = NULL;
    }
    if (wc->lpszClassName) {
        WideCharToMultiByte (CP_ACP, 0, wc->lpszClassName, -1, cls, sizeof (cls), NULL, NULL);
        a.lpszClassName = cls;
    } else {
        SetLastError (ERROR_INVALID_PARAMETER);
        return FALSE;
    }
    shim_log ("RCW cls='%s' style=%08lx proc=%08lx inst=%08lx icon=%08lx cur=%08lx bg=%08lx menu=%08lx cx=%ld cy=%ld",
        cls, (unsigned long)wc->style, (unsigned long)wc->lpfnWndProc,
        (unsigned long)wc->hInstance, (unsigned long)wc->hIcon,
        (unsigned long)wc->hCursor, (unsigned long)wc->hbrBackground,
        (unsigned long)wc->lpszMenuName, (long)wc->cbClsExtra, (long)wc->cbWndExtra);
    SetLastError (0);
    r = RegisterClassA (&a);
    e = GetLastError ();
    shim_log ("  -> atom=%u err=%lu", (unsigned)r, (unsigned long)e);
    SetLastError (e);
    return r;
}

static void shim_log (const char *fmt, ...)
{
    char buf[1024];
    va_list ap;
    int n;
    FILE *f;

    va_start (ap, fmt);
    n = vsprintf (buf, fmt, ap);
    va_end (ap);
    if (n < 0 || n >= (int)sizeof (buf) - 2) n = sizeof (buf) - 3;
    buf[n] = '\r';
    buf[n + 1] = '\n';
    buf[n + 2] = 0;

    f = fopen ("C:\\SHIMLOG.TXT", "a");
    if (!f) return;
    fwrite (buf, 1, n + 2, f);
    fclose (f);
}



BOOL WINAPI PeekMessageW (LPMSG msg, HWND hWnd, UINT min, UINT max, UINT remove)
{
    return PeekMessageA (msg, hWnd, min, max, remove);
}
