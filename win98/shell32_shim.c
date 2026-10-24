/* shell32 shim for Win98: implements the shell32 5.0 APIs Mono needs. */
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <shellapi.h>

BOOL WINAPI SHGetSpecialFolderPathA(HWND, LPSTR, int, BOOL);

BOOL WINAPI DllMain(HINSTANCE h, DWORD r, LPVOID p) { (void)h;(void)r;(void)p; return TRUE; }

/* CommandLineToArgvW: split per Windows rules (quotes, backslash escapes). */
__declspec(dllexport) LPWSTR *WINAPI CommandLineToArgvW(LPCWSTR cmd, int *argc)
{
    int cap = 16, n = 0, i = 0;
    LPWSTR *argv = (LPWSTR *) LocalAlloc (LMEM_FIXED, cap * sizeof (LPWSTR));
    if (!argv) return NULL;
    if (!cmd) cmd = L"";
    while (cmd[i]) {
        WCHAR buf[4096];
        int b = 0, inq = FALSE;
        while (cmd[i]) {
            WCHAR c = cmd[i];
            if (c == L'\\') {
                int sl = 0;
                while (cmd[i] == L'\\') { sl++; i++; }
                if (cmd[i] == L'\"') {
                    int k;
                    for (k = 0; k < sl / 2 && b < 4095; k++) buf[b++] = L'\\';
                    if (sl & 1) { if (b < 4095) buf[b++] = L'\"'; i++; }
                } else {
                    int k;
                    for (k = 0; k < sl && b < 4095; k++) buf[b++] = L'\\';
                }
            } else if (c == L'\"') {
                inq = !inq; i++;
            } else if (!inq && (c == L' ' || c == L'\t')) {
                break;
            } else {
                if (b < 4095) buf[b++] = c;
                i++;
            }
        }
        buf[b] = 0;
        if (n == cap) {
            LPWSTR *na = (LPWSTR *) LocalReAlloc ((HLOCAL) argv, (cap *= 2) * sizeof (LPWSTR), LMEM_MOVEABLE);
            if (!na) { LocalFree (argv); return NULL; }
            argv = na;
        }
        argv[n] = (LPWSTR) LocalAlloc (LMEM_FIXED, (b + 1) * sizeof (WCHAR));
        if (!argv[n]) { LocalFree (argv); return NULL; }
        lstrcpyW (argv[n], buf);
        n++;
        while (cmd[i] == L' ' || cmd[i] == L'\t') i++;
    }
    if (argc) *argc = n;
    return argv;
}

/* SHGetFolderPathW: map to SHGetSpecialFolderPathW (available on Win98) */
__declspec(dllexport) HRESULT WINAPI SHGetFolderPathW(HWND hwnd, int csidl, HANDLE token, DWORD flags, LPWSTR path)
{
    (void) token;
    if (flags & 0x8000)   /* CSIDL_FLAG_CREATE */
        CreateDirectoryW (path, NULL);
    {
        char a[MAX_PATH];
        BOOL ok = SHGetSpecialFolderPathA (hwnd, a, csidl & 0xff, (flags & 0x8000) ? TRUE : FALSE);
        if (!ok) return E_FAIL;
        MultiByteToWideChar (CP_ACP, 0, a, -1, path, MAX_PATH);
        return S_OK;
    }
}

/* ShellExecuteExW: forward to ShellExecuteExA via ANSI conversion */
__declspec(dllexport) BOOL WINAPI ShellExecuteExW(SHELLEXECUTEINFOW *info)
{
    SHELLEXECUTEINFOA a;
    char file[MAX_PATH * 2], params[2048], dir[MAX_PATH * 2];
    BOOL r;
    if (!info) return FALSE;
    ZeroMemory (&a, sizeof (a));
    a.cbSize = sizeof (a);
    a.fMask = info->fMask;
    a.hwnd = info->hwnd;
    a.lpVerb = NULL;
    a.lpFile = WideCharToMultiByte (CP_ACP, 0, info->lpFile, -1, file, sizeof (file), NULL, NULL) ? file : NULL;
    a.lpParameters = info->lpParameters ? (WideCharToMultiByte (CP_ACP, 0, info->lpParameters, -1, params, sizeof (params), NULL, NULL) ? params : NULL) : NULL;
    a.lpDirectory = info->lpDirectory ? (WideCharToMultiByte (CP_ACP, 0, info->lpDirectory, -1, dir, sizeof (dir), NULL, NULL) ? dir : NULL) : NULL;
    a.nShow = info->nShow;
    r = ShellExecuteExA (&a);
    info->hInstApp = a.hInstApp;
    info->hProcess = a.hProcess;
    return r;
}
