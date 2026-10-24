/* Win98 compat: define the NT-only APIs under their real Win32 names so that
 * mono's object files bind to these definitions instead of kernel32 imports.
 * Linked before kernel32 with --allow-multiple-definition. */
#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0400
#endif

typedef struct _mono98_PROCESSOR_NUMBER { WORD Group; BYTE Number; BYTE Reserved; } mono98_PROCESSOR_NUMBER;

static ULONGLONG (WINAPI *real_GetTickCount64)(void);

ULONGLONG WINAPI GetTickCount64(void)
{
    static DWORD last;
    static ULONGLONG high;
    DWORD now;
    if (real_GetTickCount64)
        return real_GetTickCount64 ();
    now = GetTickCount ();
    if (now < last)
        high += 0x100000000ULL;
    last = now;
    return high + now;
}

void WINAPI InitializeSRWLock(PSRWLOCK lock)
{
    CRITICAL_SECTION *cs = (CRITICAL_SECTION *) HeapAlloc (GetProcessHeap (), 0, sizeof (CRITICAL_SECTION));
    InitializeCriticalSection (cs);
    lock->Ptr = cs;
}
void WINAPI AcquireSRWLockExclusive(PSRWLOCK lock) { EnterCriticalSection ((CRITICAL_SECTION *) lock->Ptr); }
void WINAPI ReleaseSRWLockExclusive(PSRWLOCK lock) { LeaveCriticalSection ((CRITICAL_SECTION *) lock->Ptr); }
void WINAPI AcquireSRWLockShared(PSRWLOCK lock)    { EnterCriticalSection ((CRITICAL_SECTION *) lock->Ptr); }
void WINAPI ReleaseSRWLockShared(PSRWLOCK lock)    { LeaveCriticalSection ((CRITICAL_SECTION *) lock->Ptr); }
BOOLEAN WINAPI TryAcquireSRWLockExclusive(PSRWLOCK lock) { return TryEnterCriticalSection ((CRITICAL_SECTION *) lock->Ptr); }

typedef struct { HANDLE event; CRITICAL_SECTION gen_lock; ULONG generation; } mono98_cond;

void WINAPI InitializeConditionVariable(PCONDITION_VARIABLE cv)
{
    mono98_cond *c = (mono98_cond *) HeapAlloc (GetProcessHeap (), HEAP_ZERO_MEMORY, sizeof (mono98_cond));
    c->event = CreateEvent (NULL, TRUE, FALSE, NULL);
    InitializeCriticalSection (&c->gen_lock);
    cv->Ptr = c;
}

static void cond_wait (PCONDITION_VARIABLE cv, DWORD timeout, void (*enter)(void *), void (*leave)(void *), void *lock)
{
    mono98_cond *c = (mono98_cond *) cv->Ptr;
    ULONG gen;
    EnterCriticalSection (&c->gen_lock);
    gen = c->generation;
    LeaveCriticalSection (&c->gen_lock);
    if (leave) leave (lock);
    WaitForSingleObject (c->event, timeout);
    if (enter) enter (lock);
    EnterCriticalSection (&c->gen_lock);
    if (c->generation == gen)
        ResetEvent (c->event);
    LeaveCriticalSection (&c->gen_lock);
}

static void cs_enter (void *cs) { EnterCriticalSection ((CRITICAL_SECTION *) cs); }
static void cs_leave (void *cs) { LeaveCriticalSection ((CRITICAL_SECTION *) cs); }
static void srw_enter (void *l) { EnterCriticalSection ((CRITICAL_SECTION *) ((PSRWLOCK) l)->Ptr); }
static void srw_leave (void *l) { LeaveCriticalSection ((CRITICAL_SECTION *) ((PSRWLOCK) l)->Ptr); }

BOOL WINAPI SleepConditionVariableCS(PCONDITION_VARIABLE cv, PCRITICAL_SECTION cs, DWORD ms)
{ cond_wait (cv, ms, cs_enter, cs_leave, cs); return TRUE; }
BOOL WINAPI SleepConditionVariableSRW(PCONDITION_VARIABLE cv, PSRWLOCK lock, DWORD ms, ULONG flags)
{ (void) flags; cond_wait (cv, ms, srw_enter, srw_leave, lock); return TRUE; }

void WINAPI WakeConditionVariable(PCONDITION_VARIABLE cv)
{
    mono98_cond *c = (mono98_cond *) cv->Ptr;
    EnterCriticalSection (&c->gen_lock);
    c->generation++;
    LeaveCriticalSection (&c->gen_lock);
    SetEvent (c->event);
}
void WINAPI WakeAllConditionVariable(PCONDITION_VARIABLE cv) { WakeConditionVariable (cv); }

DWORD WINAPI GetCurrentProcessorNumberEx(mono98_PROCESSOR_NUMBER *out)
{ out->Group = 0; out->Number = 0; out->Reserved = 0; return 0; }

BOOL WINAPI GetFileInformationByHandleEx(HANDLE h, int cls, LPVOID buf, DWORD size)
{ (void) h; (void) cls; (void) buf; (void) size; SetLastError (ERROR_INVALID_PARAMETER); return FALSE; }

BOOL WINAPI GetModuleHandleExW(DWORD flags, LPCWSTR name, HMODULE *mod)
{
    HMODULE h = name ? GetModuleHandleW (name) : GetModuleHandleW (NULL);
    (void) flags;
    if (!h) { SetLastError (ERROR_MOD_NOT_FOUND); return FALSE; }
    *mod = h;
    return TRUE;
}

/* Win2000+ API.  Win9x has no per-CPU counters; derive an idle-only clock
 * from GetTickCount so mono_cpu_usage() doesn't hit g_error() and abort. */
BOOL WINAPI GetSystemTimes(LPFILETIME idle, LPFILETIME kernel, LPFILETIME user)
{
    ULARGE_INTEGER t;
    t.QuadPart = (ULONGLONG) GetTickCount () * 10000ULL;
    if (idle) { idle->dwLowDateTime = t.LowPart; idle->dwHighDateTime = t.HighPart; }
    if (kernel) { kernel->dwLowDateTime = 0; kernel->dwHighDateTime = 0; }
    if (user) { user->dwLowDateTime = 0; user->dwHighDateTime = 0; }
    return TRUE;
}

BOOL WINAPI IsWow64Process(HANDLE proc, PBOOL wow64)
{ (void) proc; *wow64 = FALSE; return TRUE; }

BOOL WINAPI SetThreadStackGuarantee(PULONG size) { (void) size; return TRUE; }

PVOID WINAPI AddVectoredExceptionHandler(ULONG first, PVECTORED_EXCEPTION_HANDLER handler)
{ (void) first; (void) handler; return NULL; }

ULONG WINAPI RemoveVectoredExceptionHandler(PVOID handle) { (void) handle; return 0; }

/* InitializeCriticalSectionEx: Win98 only has InitializeCriticalSection */
void WINAPI InitializeCriticalSectionEx(LPCRITICAL_SECTION cs, DWORD spin, DWORD flags)
{ (void) spin; (void) flags; InitializeCriticalSection (cs); }
