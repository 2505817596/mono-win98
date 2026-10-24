/*
 * mono98k32.dll - Win98 compatibility shim for kernel32.
 * Implements the 19 NT-only APIs Mono imports; every other kernel32 export
 * used by Mono is a forwarder back to KERNEL32 (see mono98k32.def).
 */
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <tlhelp32.h>
#include <stdio.h>
static const WCHAR *mono98_strip_prefix (const WCHAR *p);


#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0400
#endif




/* ANSI helpers: Win98 W APIs are non-functional stubs without MSLU */
static void w2a(const WCHAR *w, char *a, int n) { WideCharToMultiByte(CP_ACP,0,w,-1,a,n,NULL,NULL); }
static void a2w(const char *a, WCHAR *w, int n) { MultiByteToWideChar(CP_ACP,0,a,-1,w,n); }

__declspec(dllimport) BOOL WINAPI AddJobW(HANDLE, DWORD, LPBYTE, DWORD, LPDWORD);
static void *volatile unicows_dep;
BOOL WINAPI DllMain(HINSTANCE h, DWORD reason, LPVOID r)
{
    unicows_dep = (void *) AddJobW;   /* force unicows.dll dependency */
    (void)h; (void)reason; (void)r;
    return TRUE;
}

/* --- time --- */
ULONGLONG WINAPI GetTickCount64(void)
{
    static DWORD last;
    static ULONGLONG high;
    DWORD now = GetTickCount ();
    if (now < last)
        high += 0x100000000ULL;
    last = now;
    return high + now;
}

/* --- SRW locks: critical-section backed --- */
void WINAPI InitializeSRWLock(PSRWLOCK lock)
{
    CRITICAL_SECTION *cs = (CRITICAL_SECTION *) HeapAlloc (GetProcessHeap (), 0, sizeof (CRITICAL_SECTION));
    InitializeCriticalSection (cs);
    lock->Ptr = cs;
}
void WINAPI AcquireSRWLockExclusive(PSRWLOCK l) { EnterCriticalSection ((CRITICAL_SECTION *) l->Ptr); }
void WINAPI ReleaseSRWLockExclusive(PSRWLOCK l) { LeaveCriticalSection ((CRITICAL_SECTION *) l->Ptr); }
void WINAPI AcquireSRWLockShared(PSRWLOCK l)    { EnterCriticalSection ((CRITICAL_SECTION *) l->Ptr); }
void WINAPI ReleaseSRWLockShared(PSRWLOCK l)    { LeaveCriticalSection ((CRITICAL_SECTION *) l->Ptr); }
BOOLEAN WINAPI TryAcquireSRWLockExclusive(PSRWLOCK l) { return TryEnterCriticalSection ((CRITICAL_SECTION *) l->Ptr); }

/* --- condition variables: event + generation counter --- */
typedef struct { HANDLE event; CRITICAL_SECTION lock; ULONG generation; LONG waiters; } mono98_cond;

void WINAPI InitializeConditionVariable(PCONDITION_VARIABLE cv)
{
    mono98_cond *c = (mono98_cond *) HeapAlloc (GetProcessHeap (), HEAP_ZERO_MEMORY, sizeof (mono98_cond));
    c->event = CreateEvent (NULL, TRUE, FALSE, NULL);
    InitializeCriticalSection (&c->lock);
    cv->Ptr = c;
}

static void cond_wait (PCONDITION_VARIABLE cv, DWORD ms, void (*enter)(void *), void (*leave)(void *), void *l)
{
    mono98_cond *c = (mono98_cond *) cv->Ptr;
    ULONG gen;
    EnterCriticalSection (&c->lock);
    gen = c->generation;
    ResetEvent (c->event);       /* clear before sleeping: prevents permanent-signal spin */
    c->waiters++;
    LeaveCriticalSection (&c->lock);
    if (leave) leave (l);
    WaitForSingleObject (c->event, ms);
    if (enter) enter (l);
    EnterCriticalSection (&c->lock);
    c->waiters--;
    if (c->generation == gen)
        ResetEvent (c->event);
    LeaveCriticalSection (&c->lock);
}

static void cs_enter (void *p) { EnterCriticalSection ((CRITICAL_SECTION *) p); }
static void cs_leave (void *p) { LeaveCriticalSection ((CRITICAL_SECTION *) p); }
static void srw_enter (void *p) { EnterCriticalSection ((CRITICAL_SECTION *) ((PSRWLOCK) p)->Ptr); }
static void srw_leave (void *p) { LeaveCriticalSection ((CRITICAL_SECTION *) ((PSRWLOCK) p)->Ptr); }

BOOL WINAPI SleepConditionVariableCS(PCONDITION_VARIABLE cv, PCRITICAL_SECTION cs, DWORD ms)
{ cond_wait (cv, ms, cs_enter, cs_leave, cs); return TRUE; }

BOOL WINAPI SleepConditionVariableSRW(PCONDITION_VARIABLE cv, PSRWLOCK l, DWORD ms, ULONG flags)
{ (void) flags; cond_wait (cv, ms, srw_enter, srw_leave, l); return TRUE; }

void WINAPI WakeConditionVariable(PCONDITION_VARIABLE cv)
{
    mono98_cond *c = (mono98_cond *) cv->Ptr;
    EnterCriticalSection (&c->lock);
    c->generation++;
    if (c->waiters > 0)
        SetEvent (c->event);
    LeaveCriticalSection (&c->lock);
}
void WINAPI WakeAllConditionVariable(PCONDITION_VARIABLE cv) { WakeConditionVariable (cv); }

/* --- misc --- */
void WINAPI GetCurrentProcessorNumberEx(PPROCESSOR_NUMBER out)
{ out->Group = 0; out->Number = 0; out->Reserved = 0; }

BOOL WINAPI GetFileInformationByHandle(HANDLE h, LPBY_HANDLE_FILE_INFORMATION i)
{
    DWORD hi, lo;
    i->dwFileAttributes = GetFileAttributes (NULL); /* filled by caller path; not supported */
    (void) h; (void) hi; (void) lo;
    SetLastError (ERROR_CALL_NOT_IMPLEMENTED);
    return FALSE;
}

BOOL WINAPI GetFileInformationByHandleEx(HANDLE h, FILE_INFO_BY_HANDLE_CLASS cls, LPVOID buf, DWORD size)
{ (void) h; (void) cls; (void) buf; (void) size; SetLastError (ERROR_CALL_NOT_IMPLEMENTED); return FALSE; }

BOOL WINAPI GetModuleHandleExW(DWORD flags, LPCWSTR name, HMODULE *mod)
{
    char an[260];
    HMODULE h;
    if (name) { w2a (name, an, sizeof (an)); h = GetModuleHandleA (an); }
    else h = GetModuleHandleA (NULL);
    (void) flags;
    if (!h) { SetLastError (ERROR_MOD_NOT_FOUND); return FALSE; }
    *mod = h;
    return TRUE;
}

BOOL WINAPI SetThreadStackGuarantee(PULONG size) { (void) size; return TRUE; }

PVOID WINAPI AddVectoredExceptionHandler(ULONG first, PVECTORED_EXCEPTION_HANDLER handler)
{ (void) first; (void) handler; return NULL; }

ULONG WINAPI RemoveVectoredExceptionHandler(PVOID handle) { (void) handle; return 0; }

BOOL WINAPI InitializeCriticalSectionEx(LPCRITICAL_SECTION cs, DWORD spin, DWORD flags)
{ (void) spin; (void) flags; InitializeCriticalSection (cs); return TRUE; }

/* CancelIoEx / CancelSynchronousIo: no equivalent on Win9x */
BOOL WINAPI CancelIoEx(HANDLE h, LPOVERLAPPED o) { (void) h; (void) o; SetLastError (ERROR_NOT_SUPPORTED); return FALSE; }
BOOL WINAPI CancelSynchronousIo(HANDLE t) { (void) t; SetLastError (ERROR_NOT_SUPPORTED); return FALSE; }

/* Win98 is never WOW64 */
BOOL WINAPI IsWow64Process(HANDLE proc, PBOOL wow64)
{ (void) proc; *wow64 = FALSE; return TRUE; }

/* ---------- additional Win98 gaps found by GetProcAddress probe ---------- */
void WINAPI FlushProcessWriteBuffers(void) { MemoryBarrier(); }

BOOL WINAPI GetFileSizeEx(HANDLE h, PLARGE_INTEGER size)
{
    DWORD hi = 0, lo = GetFileSize (h, &hi);
    if (lo == INVALID_FILE_SIZE && GetLastError () != NO_ERROR) return FALSE;
    size->LowPart = lo; size->HighPart = (LONG) hi;
    return TRUE;
}

int WINAPI GetLocaleInfoEx(LPCWSTR name, LCTYPE type, LPWSTR buf, int cch)
{
    char a[512];
    int r;
    (void) name;
    r = GetLocaleInfoA (LOCALE_USER_DEFAULT, type, a, sizeof (a));
    if (r <= 0) return r;
    if (!buf) return r;
    a2w (a, buf, cch);
    return r;
}

DWORD WINAPI GetProcessId(HANDLE proc)
{
    if (proc == GetCurrentProcess ()) return GetCurrentProcessId ();
    SetLastError (ERROR_CALL_NOT_IMPLEMENTED);
    return 0;
}

BOOL WINAPI GlobalMemoryStatusEx(LPMEMORYSTATUSEX ms)
{
    MEMORYSTATUS m; ZeroMemory (&m, sizeof (m)); m.dwLength = sizeof (m);
    GlobalMemoryStatus (&m);
    ms->dwLength = sizeof (*ms);
    ms->dwMemoryLoad = m.dwMemoryLoad;
    ms->ullTotalPhys = m.dwTotalPhys;   ms->ullAvailPhys = m.dwAvailPhys;
    ms->ullTotalPageFile = m.dwTotalPageFile; ms->ullAvailPageFile = m.dwAvailPageFile;
    ms->ullTotalVirtual = m.dwTotalVirtual;   ms->ullAvailVirtual = m.dwAvailVirtual;
    ms->ullAvailExtendedVirtual = 0;
    return TRUE;
}

HANDLE WINAPI OpenThread(DWORD access, BOOL inherit, DWORD tid)
{
    (void) access; (void) inherit; (void) tid;
    SetLastError (ERROR_CALL_NOT_IMPLEMENTED);
    return NULL;
}

BOOL WINAPI ReplaceFileW(LPCWSTR replaced, LPCWSTR replacement, LPCWSTR backup,
                         DWORD flags, LPVOID excl, LPVOID reserved)
{
    char a1[260], a2[260], a3[260];
    (void) flags; (void) excl; (void) reserved;
    w2a (replaced, a1, sizeof (a1));
    w2a (replacement, a2, sizeof (a2));
    if (backup) { w2a (backup, a3, sizeof (a3)); if (!CopyFileA (a1, a3, FALSE)) return FALSE; }
    if (!DeleteFileA (a1)) return FALSE;
    return MoveFileA (a2, a1);
}

/* psapi.dll shims (K32* aliases) */
static FARPROC psapi_fn(const char *name)
{
    static HMODULE ps;
    if (!ps) ps = LoadLibraryA ("psapi.dll");
    return ps ? GetProcAddress (ps, name) : NULL;
}
BOOL WINAPI K32EnumProcesses(DWORD *ids, DWORD cb, DWORD *needed)
{
    typedef BOOL (WINAPI *F)(DWORD *, DWORD, DWORD *);
    F f = (F) psapi_fn ("EnumProcesses");
    if (f) return f (ids, cb, needed);
    SetLastError (ERROR_CALL_NOT_IMPLEMENTED); return FALSE;
}
BOOL WINAPI K32EnumProcessModules(HANDLE h, HMODULE *mods, DWORD cb, DWORD *needed)
{
    typedef BOOL (WINAPI *F)(HANDLE, HMODULE *, DWORD, DWORD *);
    F f = (F) psapi_fn ("EnumProcessModules");
    if (f) return f (h, mods, cb, needed);
    SetLastError (ERROR_CALL_NOT_IMPLEMENTED); return FALSE;
}
DWORD WINAPI K32GetModuleBaseNameW(HANDLE h, HMODULE m, LPWSTR buf, DWORD cb)
{
    typedef DWORD (WINAPI *F)(HANDLE, HMODULE, LPSTR, DWORD);
    char a[260]; DWORD r;
    F f = (F) psapi_fn ("GetModuleBaseNameA");
    if (!f) { SetLastError (ERROR_CALL_NOT_IMPLEMENTED); return 0; }
    r = f (h, m, a, sizeof (a));
    if (r && buf) a2w (a, buf, cb / 2);
    return r;
}
DWORD WINAPI K32GetModuleFileNameExW(HANDLE h, HMODULE m, LPWSTR buf, DWORD cb)
{
    typedef DWORD (WINAPI *F)(HANDLE, HMODULE, LPSTR, DWORD);
    char a[260]; DWORD r;
    F f = (F) psapi_fn ("GetModuleFileNameExA");
    if (!f) { SetLastError (ERROR_CALL_NOT_IMPLEMENTED); return 0; }
    r = f (h, m, a, sizeof (a));
    if (r && buf) a2w (a, buf, cb / 2);
    return r;
}
BOOL WINAPI K32GetModuleInformation(HANDLE h, HMODULE m, LPVOID info, DWORD cb)
{
    typedef BOOL (WINAPI *F)(HANDLE, HMODULE, LPVOID, DWORD);
    F f = (F) psapi_fn ("GetModuleInformation");
    if (f) return f (h, m, info, cb);
    SetLastError (ERROR_CALL_NOT_IMPLEMENTED); return FALSE;
}

/* Windows 2000+ API; Win9x has no per-CPU counters.  Report an idle-only
 * clock derived from GetTickCount so callers (mono_cpu_usage) don't abort. */
BOOL WINAPI GetSystemTimes(LPFILETIME idle, LPFILETIME kernel, LPFILETIME user)
{
    ULARGE_INTEGER t;
    t.QuadPart = (ULONGLONG) GetTickCount () * 10000ULL;
    if (idle) { idle->dwLowDateTime = t.LowPart; idle->dwHighDateTime = t.HighPart; }
    if (kernel) { kernel->dwLowDateTime = 0; kernel->dwHighDateTime = 0; }
    if (user) { user->dwLowDateTime = 0; user->dwHighDateTime = 0; }
    return TRUE;
}

/* In-process crash logger: capture the faulting address on Win9x. */
typedef LPTOP_LEVEL_EXCEPTION_FILTER (WINAPI *SET_FILTER_FN)(LPTOP_LEVEL_EXCEPTION_FILTER);
static LPTOP_LEVEL_EXCEPTION_FILTER mono98_prev_filter;
static SET_FILTER_FN mono98_real_set_filter;

static void mono98_addr (FILE *f, const char *tag, DWORD a)
{
    MEMORY_BASIC_INFORMATION mbi;
    char mod[64];
    DWORD off = 0;
    HANDLE snap;
    MODULEENTRY32 me;
    lstrcpynA (mod, "?", sizeof (mod));
    if (!VirtualQuery ((LPCVOID)(DWORD_PTR) a, &mbi, sizeof (mbi))) {
        fprintf (f, " %s=%08lx [unmapped]", tag, a);
        return;
    }
    snap = CreateToolhelp32Snapshot (TH32CS_SNAPMODULE, GetCurrentProcessId ());
    if (snap != INVALID_HANDLE_VALUE) {
        me.dwSize = sizeof (me);
        if (Module32First (snap, &me)) {
            do {
                DWORD b = (DWORD)(DWORD_PTR) me.modBaseAddr;
                if (a >= b && a < b + me.modBaseSize) {
                    lstrcpynA (mod, me.szModule, sizeof (mod));
                    off = a - b;
                    break;
                }
            } while (Module32Next (snap, &me));
        }
        CloseHandle (snap);
    }
    fprintf (f, " %s=%08lx [%s+%lx] st=%lx ty=%lx pr=%lx alloc=%p sz=%lx",
             tag, a, mod, off, mbi.State, mbi.Type, mbi.Protect,
             mbi.AllocationBase, mbi.RegionSize);
}

static const char *mono98_pmip (DWORD a)
{
    static HMODULE lib;
    static char *(*pmip) (void *);
    if (!lib)
        lib = GetModuleHandleA ("libmonosgen-2.0.dll");
    if (!lib)
        return NULL;
    if (!pmip)
        pmip = (char *(*)(void *)) GetProcAddress (lib, "mono_pmip");
    if (!pmip)
        return NULL;
    return pmip ((void *)(DWORD_PTR) a);
}

static int mono98_crash_no;

static void mono98_log_crash (EXCEPTION_POINTERS *ep)
{
    FILE *f;
    int n = ++mono98_crash_no;
    DWORD cand [12];
    int nc = 0, i;
    f = fopen ("C:\\CRASH98.TXT", "a");
    if (!f) return;
    if (!ep || !ep->ExceptionRecord) {
        fprintf (f, "#%d filter called without exception info\n", n);
        fclose (f);
        return;
    }
    fprintf (f, "#%d code=%08lx addr=%p flags=%lx params=%lu",
             n, ep->ExceptionRecord->ExceptionCode,
             ep->ExceptionRecord->ExceptionAddress,
             ep->ExceptionRecord->ExceptionFlags,
             ep->ExceptionRecord->NumberParameters);
    if (ep->ExceptionRecord->NumberParameters > 1)
        fprintf (f, " i0=%lx i1=%lx",
                 ep->ExceptionRecord->ExceptionInformation[0],
                 ep->ExceptionRecord->ExceptionInformation[1]);
    if (ep->ContextRecord)
        fprintf (f, " eip=%08lx esp=%08lx ebp=%08lx",
                 ep->ContextRecord->Eip, ep->ContextRecord->Esp, ep->ContextRecord->Ebp);
    fputc (10, f);

    if (n <= 3) {
        if (ep->ExceptionRecord->NumberParameters > 1)
            mono98_addr (f, "faultdata", (DWORD) ep->ExceptionRecord->ExceptionInformation[1]);
        if (ep->ContextRecord) {
            DWORD esp = ep->ContextRecord->Esp;
            DWORD ebp = ep->ContextRecord->Ebp;
            DWORD *sp = (DWORD *)(DWORD_PTR) esp;
            mono98_addr (f, "eip", ep->ContextRecord->Eip);
            fputc (10, f);
            fprintf (f, "  stack:");
            for (i = 0; i < 24; i++) {
                DWORD v = sp [i];
                fprintf (f, " %08lx", v);
                if (nc < 12 && (v & 0xfff00000) == 0x63300000)
                    cand [nc++] = v;
            }
            fputc (10, f);
            for (i = 0; i < 6 && ebp > esp && ebp < esp + 0x10000 && (ebp & 3) == 0; i++) {
                DWORD *fp = (DWORD *)(DWORD_PTR) ebp;
                DWORD next = fp [0];
                DWORD ret = fp [1];
                mono98_addr (f, "ret", ret);
                fputc (10, f);
                if (nc < 12 && (ret & 0xfff00000) == 0x63300000)
                    cand [nc++] = ret;
                if (next <= ebp)
                    break;
                ebp = next;
            }
        }
    } else {
        if (ep->ExceptionRecord->NumberParameters > 1)
            fprintf (f, "  (repeat) i1=%lx\n", ep->ExceptionRecord->ExceptionInformation[1]);
    }
    fflush (f);
    if (n <= 2 && ep->ExceptionRecord->ExceptionAddress) {
        const char *s = mono98_pmip ((DWORD)(DWORD_PTR) ep->ExceptionRecord->ExceptionAddress);
        if (s)
            fprintf (f, "  pmip(eip)=%s\n", s);
        for (i = 0; i < nc && i < 4; i++) {
            s = mono98_pmip (cand [i]);
            if (s)
                fprintf (f, "  pmip(%08lx)=%s\n", cand [i], s);
        }
    }
    fclose (f);
}

static LONG WINAPI mono98_unhandled_filter (EXCEPTION_POINTERS *ep)
{
    mono98_log_crash (ep);
    if (mono98_prev_filter)
        return mono98_prev_filter (ep);
    return EXCEPTION_CONTINUE_SEARCH;
}

LPTOP_LEVEL_EXCEPTION_FILTER WINAPI SetUnhandledExceptionFilter (LPTOP_LEVEL_EXCEPTION_FILTER f)
{
    LPTOP_LEVEL_EXCEPTION_FILTER old = mono98_prev_filter;
    if (!mono98_real_set_filter)
        mono98_real_set_filter = (SET_FILTER_FN) GetProcAddress (GetModuleHandleA ("kernel32.dll"), "SetUnhandledExceptionFilter");
    mono98_prev_filter = f;
    if (mono98_real_set_filter)
        mono98_real_set_filter (mono98_unhandled_filter);
    return old;
}

/* Win7+ API; Win9x has no per-thread error mode.  Pretend success so
 * managed file-system code doesn't die on a missing entry point. */
BOOL WINAPI SetThreadErrorMode (DWORD dwNewMode, LPDWORD lpOldMode)
{
    if (lpOldMode) *lpOldMode = 0;
    (void) dwNewMode;
    return TRUE;
}

/* Win2000+ API; Win9x only has FindFirstFileA/W. */
HANDLE WINAPI FindFirstFileExW (LPCWSTR lpFileName, FINDEX_INFO_LEVELS fInfoLevelId,
                                LPVOID lpFindFileData, FINDEX_SEARCH_OPS fSearchOp,
                                LPVOID lpSearchFilter, DWORD dwAdditionalFlags)
{
    char name[1024];
    WIN32_FIND_DATAA da;
    WIN32_FIND_DATAW *wd = (WIN32_FIND_DATAW *) lpFindFileData;
    HANDLE h;

    (void) fSearchOp; (void) lpSearchFilter; (void) dwAdditionalFlags;
    if (!lpFileName || !lpFindFileData ||
        (fInfoLevelId != FindExInfoStandard && fInfoLevelId != FindExInfoBasic)) {
        SetLastError (ERROR_INVALID_PARAMETER);
        return INVALID_HANDLE_VALUE;
    }
    if (!WideCharToMultiByte (CP_ACP, 0, mono98_strip_prefix (lpFileName), -1, name, sizeof (name), NULL, NULL)) {
        SetLastError (ERROR_INVALID_PARAMETER);
        return INVALID_HANDLE_VALUE;
    }
    h = FindFirstFileA (name, &da);
    if (h == INVALID_HANDLE_VALUE)
        return h;
    ZeroMemory (wd, sizeof (*wd));
    wd->dwFileAttributes = da.dwFileAttributes;
    wd->ftCreationTime = da.ftCreationTime;
    wd->ftLastAccessTime = da.ftLastAccessTime;
    wd->ftLastWriteTime = da.ftLastWriteTime;
    wd->nFileSizeHigh = da.nFileSizeHigh;
    wd->nFileSizeLow = da.nFileSizeLow;
    wd->dwReserved0 = da.dwReserved0;
    wd->dwReserved1 = da.dwReserved1;
    MultiByteToWideChar (CP_ACP, 0, da.cFileName, -1, wd->cFileName, MAX_PATH);
    MultiByteToWideChar (CP_ACP, 0, da.cAlternateFileName, -1, wd->cAlternateFileName, 14);
    return h;
}

HANDLE WINAPI FindFirstFileExA (LPCSTR lpFileName, FINDEX_INFO_LEVELS fInfoLevelId,
                                LPVOID lpFindFileData, FINDEX_SEARCH_OPS fSearchOp,
                                LPVOID lpSearchFilter, DWORD dwAdditionalFlags)
{
    (void) fSearchOp; (void) lpSearchFilter; (void) dwAdditionalFlags;
    if (fInfoLevelId != FindExInfoStandard && fInfoLevelId != FindExInfoBasic) {
        SetLastError (ERROR_INVALID_PARAMETER);
        return INVALID_HANDLE_VALUE;
    }
    return FindFirstFileA (lpFileName, (WIN32_FIND_DATAA *) lpFindFileData);
}

/* Vista+ API; Win9x only has GetTimeZoneInformation.  Fill the extended
 * structure from the classic one so TimeZoneInfo works. */


DWORD WINAPI GetDynamicTimeZoneInformation (DYNAMIC_TIME_ZONE_INFORMATION *p)
{
    TIME_ZONE_INFORMATION tzi;
    DWORD r;
    if (!p) { SetLastError (ERROR_INVALID_PARAMETER); return 0xFFFFFFFF; }
    ZeroMemory (&tzi, sizeof (tzi));
    r = GetTimeZoneInformation (&tzi);
    ZeroMemory (p, sizeof (*p));
    p->Bias = tzi.Bias;
    lstrcpynW (p->StandardName, tzi.StandardName, 32);
    p->StandardDate = tzi.StandardDate;
    p->StandardBias = tzi.StandardBias;
    lstrcpynW (p->DaylightName, tzi.DaylightName, 32);
    p->DaylightDate = tzi.DaylightDate;
    p->DaylightBias = tzi.DaylightBias;
    p->TimeZoneKeyName[0] = 0;
    p->DynamicDaylightTimeDisabled = FALSE;
    return r;
}

BOOL WINAPI SetDynamicTimeZoneInformation (const DYNAMIC_TIME_ZONE_INFORMATION *p)
{
    (void) p;
    SetLastError (ERROR_CALL_NOT_IMPLEMENTED);
    return FALSE;
}

/* Win9x file APIs: corefx passes extended-length (question-mark) paths to
 * CreateDirectoryW / RemoveDirectoryW(recursive) / FindFirstFileW etc.
 * Win9x has no extended path syntax (CreateDirectoryA returns
 * ERROR_INVALID_FUNCTION), so strip the prefix and use the ANSI API. */
static const WCHAR *mono98_strip_prefix (const WCHAR *p)
{
    if (!p) return p;
    if (p[0] == 0x5C && p[1] == 0x5C && p[2] == '?' && p[3] == 0x5C) {
        if (!(p[4] == 'U' && p[5] == 'N' && p[6] == 'C' && p[7] == 0x5C))
            return p + 4;
    }
    if (p[0] == 0x5C && p[1] == '?' && p[2] == '?' && p[3] == 0x5C)
        return p + 4;
    return p;
}

static BOOL mono98_w2a (const WCHAR *w, char *buf, int len)
{
    return WideCharToMultiByte (CP_ACP, 0, w, -1, buf, len, NULL, NULL) > 0;
}

static void mono98_fill_wfd (const WIN32_FIND_DATAA *a, WIN32_FIND_DATAW *w)
{
    ZeroMemory (w, sizeof (*w));
    w->dwFileAttributes = a->dwFileAttributes;
    w->ftCreationTime = a->ftCreationTime;
    w->ftLastAccessTime = a->ftLastAccessTime;
    w->ftLastWriteTime = a->ftLastWriteTime;
    w->nFileSizeHigh = a->nFileSizeHigh;
    w->nFileSizeLow = a->nFileSizeLow;
    w->dwReserved0 = a->dwReserved0;
    w->dwReserved1 = a->dwReserved1;
    MultiByteToWideChar (CP_ACP, 0, a->cFileName, -1, w->cFileName, MAX_PATH);
    MultiByteToWideChar (CP_ACP, 0, a->cAlternateFileName, -1, w->cAlternateFileName, 14);
}

BOOL WINAPI CreateDirectoryW (LPCWSTR path, LPSECURITY_ATTRIBUTES sa)
{
    char b[1024];
    if (!path) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    if (!mono98_w2a (mono98_strip_prefix (path), b, sizeof b)) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    BOOL r = CreateDirectoryA (b, sa);
    return r;
}

BOOL WINAPI RemoveDirectoryW (LPCWSTR path)
{
    char b[1024];
    if (!path) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    if (!mono98_w2a (mono98_strip_prefix (path), b, sizeof b)) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    BOOL r = RemoveDirectoryA (b);
    return r;
}

BOOL WINAPI DeleteFileW (LPCWSTR path)
{
    char b[1024];
    if (!path) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    if (!mono98_w2a (mono98_strip_prefix (path), b, sizeof b)) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    BOOL r = DeleteFileA (b);
    return r;
}

DWORD WINAPI GetFileAttributesW (LPCWSTR path)
{
    char b[1024];
    if (!path) { SetLastError (ERROR_INVALID_PARAMETER); return 0xFFFFFFFF; }
    if (!mono98_w2a (mono98_strip_prefix (path), b, sizeof b)) { SetLastError (ERROR_INVALID_PARAMETER); return 0xFFFFFFFF; }
    DWORD r = GetFileAttributesA (b);
    return r;
}

BOOL WINAPI GetFileAttributesExW (LPCWSTR path, GET_FILEEX_INFO_LEVELS level, LPVOID info)
{
    char b[1024];
    if (!path || !info) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    if (level != GetFileExInfoStandard) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    if (!mono98_w2a (mono98_strip_prefix (path), b, sizeof b)) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    return GetFileAttributesExA (b, level, info);
}

HANDLE WINAPI FindFirstFileW (LPCWSTR path, LPWIN32_FIND_DATAW data)
{
    char b[1024];
    WIN32_FIND_DATAA da;
    HANDLE h;
    if (!path || !data) { SetLastError (ERROR_INVALID_PARAMETER); return INVALID_HANDLE_VALUE; }
    if (!mono98_w2a (mono98_strip_prefix (path), b, sizeof b)) { SetLastError (ERROR_INVALID_PARAMETER); return INVALID_HANDLE_VALUE; }
    h = FindFirstFileA (b, &da);
    if (h == INVALID_HANDLE_VALUE) return h;
    mono98_fill_wfd (&da, data);
    return h;
}

BOOL WINAPI CopyFileW (LPCWSTR from, LPCWSTR to, BOOL fail_if_exists)
{
    char a[1024], bb[1024];
    if (!from || !to) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    if (!mono98_w2a (mono98_strip_prefix (from), a, sizeof a)) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    if (!mono98_w2a (mono98_strip_prefix (to), bb, sizeof bb)) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    return CopyFileA (a, bb, fail_if_exists);
}

BOOL WINAPI MoveFileW (LPCWSTR from, LPCWSTR to)
{
    char a[1024], bb[1024];
    if (!from || !to) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    if (!mono98_w2a (mono98_strip_prefix (from), a, sizeof a)) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    if (!mono98_w2a (mono98_strip_prefix (to), bb, sizeof bb)) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    return MoveFileA (a, bb);
}

HANDLE WINAPI CreateFileW (LPCWSTR name, DWORD access, DWORD share,
                           LPSECURITY_ATTRIBUTES sa, DWORD disposition,
                           DWORD flags, HANDLE template)
{
    char b[1024];
    if (!name) { SetLastError (ERROR_INVALID_PARAMETER); return INVALID_HANDLE_VALUE; }
    if (!mono98_w2a (mono98_strip_prefix (name), b, sizeof b)) {
        SetLastError (ERROR_INVALID_PARAMETER);
        return INVALID_HANDLE_VALUE;
    }
    HANDLE r = CreateFileA (b, access, share, sa, disposition, flags, template);
    return r;
}

BOOL WINAPI FindNextFileW (HANDLE h, LPWIN32_FIND_DATAW data)
{
    WIN32_FIND_DATAA da;
    BOOL r;
    if (!data) { SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }
    r = FindNextFileA (h, &da);
    if (r)
        mono98_fill_wfd (&da, data);
    else {
        DWORD e = GetLastError ();
        SetLastError (e);
    }
    return r;
}
