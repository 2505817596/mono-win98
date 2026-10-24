#!/usr/bin/env python3
"""Report imports in a PE that are not available on Windows 98."""
import struct, sys

# APIs introduced after Windows 98 (NT-only / 2000+ / XP+ / Vista+ / 7+)
NOT_ON_98 = {
 'kernel32.dll': set("""AcquireSRWLockExclusive AcquireSRWLockShared AddVectoredExceptionHandler
CancelIoEx CancelSynchronousIo CreateActCtxW ActivateActCtx DeactivateActCtx ReleaseActCtx
CreateEventExW CreateMutexExW CreateSemaphoreExW CreateWaitableTimerExW
FlsAlloc FlsFree FlsGetValue FlsSetValue GetActiveProcessorCount GetCurrentProcessorNumber
GetCurrentProcessorNumberEx GetCurrentThreadStackLimits GetFileInformationByHandleEx
GetFinalPathNameByHandleW GetLogicalProcessorInformationEx GetModuleHandleExA GetModuleHandleExW
GetNativeSystemInfo GetProcessInformation GetSystemTimePreciseAsFileTime GetSystemTimes
GetThreadDescription GetThreadErrorMode GetTickCount64 InitOnceExecuteOnce InitializeConditionVariable
InitializeCriticalSectionEx InitializeSListHead InitializeSRWLock IsWow64Process
QueryFullProcessImageNameW ReleaseSRWLockExclusive ReleaseSRWLockShared RemoveVectoredExceptionHandler
SetProcessInformation SetThreadDescription SetThreadErrorMode SetThreadStackGuarantee
SleepConditionVariableCS SleepConditionVariableSRW TryAcquireSRWLockExclusive TryAcquireSRWLockShared
WakeAllConditionVariable WakeConditionVariable GetLargePageMinimum VirtualAlloc2
InitializeProcThreadAttributeList UpdateProcThreadAttribute DeleteProcThreadAttributeList
CreateSymbolicLinkW GetVolumeInformationByHandleW SetFileInformationByHandle
GetFileInformationByHandle GetSystemCpuSetInformation SetThreadIdealProcessorEx
GetThreadIdealProcessorEx GetMaximumProcessorCount GetSystemFirmwareTable
GetFileAttributesExW GetLongPathNameW GetFullPathNameW""".split()),
 'shell32.dll': set("""CommandLineToArgvW SHGetFolderPathW SHGetKnownFolderPath SHCreateDirectoryExW
SHGetFolderPathAndSubDirW SHGetNameFromIDList SHGetIDListFromObject SHBindToParent""".split()),
 'user32.dll': set("""ChangeWindowMessageFilterEx GetDpiForWindow SetProcessDpiAwarenessContext
EnableNonClientDpiScaling GetPointerType GetPointerInfo AddClipboardFormatListener
RemoveClipboardFormatListener SetWindowDisplayAffinity GetWindowDisplayAffinity
RegisterTouchWindow GetTouchInputInfo CloseTouchInputHandle GetGestureInfo SetGestureConfig
GetWindowLongPtrW SetWindowLongPtrW GetRawInputData RegisterRawInputDevices
IsProcessDPIAware SetProcessDPIAware GetSystemMetricsForDpi""".split()),
 'advapi32.dll': set("""GetEffectiveRightsFromAclW GetNamedSecurityInfoW SetNamedSecurityInfoW
GetSecurityInfo SetSecurityInfo CreateProcessWithLogonW CreateProcessAsUserW
GetExplicitEntriesFromAclW SetEntriesInAclW GetInheritanceSourceW TreeResetNamedSecurityInfoW
ConvertStringSidToSidW ConvertSidToStringSidW GetTokenInformationEx
CheckTokenMembershipEx DuplicateTokenEx CreateRestrictedToken
LsaOpenPolicy LsaQueryInformationPolicy LsaClose LsaFreeMemory
GetUserNameExW GetComputerNameExW LookupAccountNameW""".split()),
 'ole32.dll': set("""CoInitializeEx CoCreateFreeThreadedMarshaler CoGetApartmentType
RoInitialize RoUninitialize RoGetActivationFactory WindowsCreateString
WindowsDeleteString WindowsGetStringRawBuffer""".split()),
 'bcrypt.dll': set("*",),
 'ws2_32.dll': set("""WSASocketW WSAPoll getaddrinfo freeaddrinfo getnameinfo inet_pton inet_ntop
WSARecvMsg WSASendMsg WSAIoctl GetAddrInfoExW SetAddrInfoExW""".split()),
 'version.dll': set(),
 'winmm.dll': set(),
 'msvcrt.dll': set("""_ecvt_s _fcvt_s _gcvt_s _itoa_s _ltoa_s _ultoa_s _ui64toa_s _i64toa_s
strcpy_s strcat_s sprintf_s vsprintf_s strncpy_s strncat_s memcpy_s memmove_s
_wfopen_s fopen_s freopen_s gets_s _strnicmp _stricoll _strncoll
_wcsdup _wcsicmp _wcsnicmp _wcsicoll _wcsncoll _wcsdup _wtoi _wtol _wtoi64 _wcsicmp
_set_invalid_parameter_handler _get_errno _set_errno _dupenv_s _wdupenv_s
_getcwd_s _wgetcwd _wfullpath _fullpath _mktemp_s _wmktemp_s _snprintf_s _vsnprintf_s
_swprintf_s _vswprintf_s _snwprintf_s _vsnwprintf_s _wfreopen_s
_initterm_e _configthreadlocale _ftime_s _localtime64_s _gmtime64_s
fseeko64 ftello64 _fseeki64 _ftelli64 _wstati64 _wfstati64
_qsort_s _wqsort_s _setmode
__stdio_common_vfprintf __stdio_common_vsprintf""".split()),
}

def rva2off(rva, secs):
    for va, vsz, raw, rawsz in secs:
        if va <= rva < va + max(vsz, rawsz):
            return raw + (rva - va)
    raise ValueError(hex(rva))

def check(path):
    data = open(path, 'rb').read()
    pe = struct.unpack_from('<I', data, 0x3c)[0]
    nsec = struct.unpack_from('<H', data, pe + 6)[0]
    opt = pe + 24
    plus = struct.unpack_from('<H', data, opt)[0] == 0x20b
    dd = opt + (112 if plus else 96)
    imp_rva, _ = struct.unpack_from('<II', data, dd + 8)
    so = opt + struct.unpack_from('<H', data, pe + 20)[0]
    secs = []
    for i in range(nsec):
        o = so + i * 40
        vsz, va, rawsz, raw = struct.unpack_from('<IIII', data, o + 8)
        secs.append((va, vsz, raw, rawsz))
    off = rva2off(imp_rva, secs)
    problems = []
    while True:
        oft, ts, fwd, nr, ft = struct.unpack_from('<IIIII', data, off)
        if nr == 0:
            break
        no = rva2off(nr, secs)
        dll = data[no:data.index(b'\0', no)].decode('latin1').lower()
        bad = NOT_ON_98.get(dll, set())
        if bad:
            t = oft or ft
            if t:
                to = rva2off(t, secs)
                while True:
                    v = struct.unpack_from('<I', data, to)[0]
                    if v == 0:
                        break
                    if not (v & 0x80000000):
                        vo = rva2off(v, secs)
                        nm = data[vo + 2:data.index(b'\0', vo + 2)].decode('latin1')
                        if bad == "*" or nm in bad:
                            problems.append((dll, nm))
                    to += 4
        off += 20
    return problems

if __name__ == '__main__':
    total = 0
    for p in sys.argv[1:]:
        probs = check(p)
        total += len(probs)
        print("%s: %d Win98-incompatible import(s)" % (p, len(probs)))
        for dll, nm in probs:
            print("   %s!%s" % (dll, nm))
    sys.exit(1 if total else 0)
