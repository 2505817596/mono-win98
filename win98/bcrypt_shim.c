/* bcrypt.dll shim for Win98: BCryptGenRandom via CryptoAPI (advapi32) */
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <wincrypt.h>

typedef long NTSTATUS;
#define BCRYPT_SUCCESS(s) ((s) >= 0)

BOOL WINAPI DllMain(HINSTANCE h, DWORD r, LPVOID p) { (void)h;(void)r;(void)p; return TRUE; }

__declspec(dllexport) NTSTATUS WINAPI BCryptGenRandom(void *alg, unsigned char *buf, unsigned long len, unsigned long flags)
{
    HCRYPTPROV prov;
    (void) alg; (void) flags;
    if (!CryptAcquireContext (&prov, NULL, NULL, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT))
        return (NTSTATUS) 0xC0000001L;
    if (!CryptGenRandom (prov, len, buf)) {
        CryptReleaseContext (prov, 0);
        return (NTSTATUS) 0xC0000001L;
    }
    CryptReleaseContext (prov, 0);
    return 0;
}
