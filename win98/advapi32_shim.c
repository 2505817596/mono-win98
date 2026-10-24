/* advapi32 shim: Win2K+ security APIs used by Mono's ACL support.
 * On Win98 NT security descriptors do not exist, so these degrade gracefully. */
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <aclapi.h>

BOOL WINAPI DllMain(HINSTANCE h, DWORD r, LPVOID p) { (void)h;(void)r;(void)p; return TRUE; }

__declspec(dllexport) DWORD WINAPI GetNamedSecurityInfoW(
    LPCWSTR name, SE_OBJECT_TYPE type, SECURITY_INFORMATION info,
    PSID *owner, PSID *group, PACL *dacl, PACL *sacl, PSECURITY_DESCRIPTOR *sd)
{
    (void) name; (void) type; (void) info; (void) owner; (void) group;
    (void) dacl; (void) sacl; (void) sd;
    return ERROR_CALL_NOT_IMPLEMENTED;
}

__declspec(dllexport) DWORD WINAPI SetNamedSecurityInfoW(
    LPWSTR name, SE_OBJECT_TYPE type, SECURITY_INFORMATION info,
    PSID owner, PSID group, PACL dacl, PACL sacl)
{
    (void) name; (void) type; (void) info; (void) owner; (void) group; (void) dacl; (void) sacl;
    return ERROR_CALL_NOT_IMPLEMENTED;
}

__declspec(dllexport) DWORD WINAPI GetEffectiveRightsFromAclW(PACL acl, PTRUSTEE_W trustee, PACCESS_MASK rights)
{
    (void) acl; (void) trustee; (void) rights;
    return ERROR_CALL_NOT_IMPLEMENTED;
}

__declspec(dllexport) DWORD WINAPI SetEntriesInAclW(ULONG count, PEXPLICIT_ACCESS_W list, PACL old, PACL *newacl)
{
    (void) count; (void) list; (void) old; (void) newacl;
    return ERROR_CALL_NOT_IMPLEMENTED;
}

__declspec(dllexport) BOOL WINAPI CreateProcessWithLogonW(
    LPCWSTR user, LPCWSTR domain, LPCWSTR password, DWORD flags,
    LPCWSTR app, LPWSTR cmdline, DWORD creation, LPVOID env, LPCWSTR cwd,
    LPSTARTUPINFOW si, LPPROCESS_INFORMATION pi)
{
    (void) user; (void) domain; (void) password; (void) flags; (void) app;
    (void) cmdline; (void) creation; (void) env; (void) cwd; (void) si; (void) pi;
    SetLastError (ERROR_CALL_NOT_IMPLEMENTED);
    return FALSE;
}
