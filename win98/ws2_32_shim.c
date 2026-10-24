/* ws2_32 shim: WSASocketW / WSAIoctl / getaddrinfo family for Win98.
 * Win98 ships Winsock 2.0 (no WSAIoctl, no getaddrinfo). */
#define WIN32_LEAN_AND_MEAN
#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>

BOOL WINAPI DllMain(HINSTANCE h, DWORD r, LPVOID p) { (void)h;(void)r;(void)p; return TRUE; }

__declspec(dllexport) SOCKET WINAPI WSASocketW(int af, int type, int proto, LPWSAPROTOCOL_INFOW info, GROUP g, DWORD flags)
{
    (void) info; (void) g; (void) flags;
    return socket (af, type, proto);
}

__declspec(dllexport) int WINAPI WSAIoctl(SOCKET s, DWORD code, LPVOID in, DWORD inlen,
                                          LPVOID out, DWORD outlen, LPDWORD ret, LPWSAOVERLAPPED ov, LPWSAOVERLAPPED_COMPLETION_ROUTINE cr)
{
    (void) s; (void) code; (void) in; (void) inlen; (void) out; (void) outlen;
    (void) ret; (void) ov; (void) cr;
    WSASetLastError (WSAEOPNOTSUPP);
    return SOCKET_ERROR;
}

/* Minimal getaddrinfo: resolve via gethostbyname, return AF_INET entries. */
__declspec(dllexport) int WINAPI getaddrinfo(const char *node, const char *service,
                                             const struct addrinfo *hints, struct addrinfo **res)
{
    struct hostent *he;
    struct addrinfo *ai;
    struct sockaddr_in *sa;
    unsigned short port = 0;
    int i, n = 0;

    if (res) *res = NULL;
    if (!node) return WSAHOST_NOT_FOUND;
    if (service)
        port = htons ((unsigned short) atoi (service));

    he = gethostbyname (node);
    if (!he || he->h_addrtype != AF_INET)
        return WSAHOST_NOT_FOUND;

    for (i = 0; he->h_addr_list[i]; i++) n++;
    ai = (struct addrinfo *) calloc (n, sizeof (struct addrinfo));
    if (!ai) return WSA_NOT_ENOUGH_MEMORY;
    for (i = 0; i < n; i++) {
        ai[i].ai_family = AF_INET;
        ai[i].ai_socktype = hints && hints->ai_socktype ? hints->ai_socktype : SOCK_STREAM;
        ai[i].ai_protocol = hints && hints->ai_protocol ? hints->ai_protocol : 0;
        ai[i].ai_addrlen = sizeof (struct sockaddr_in);
        sa = (struct sockaddr_in *) calloc (1, sizeof (struct sockaddr_in));
        sa->sin_family = AF_INET;
        sa->sin_port = port;
        memcpy (&sa->sin_addr, he->h_addr_list[i], 4);
        ai[i].ai_addr = (struct sockaddr *) sa;
        ai[i].ai_next = (i + 1 < n) ? &ai[i + 1] : NULL;
    }
    if (res) *res = ai;
    return 0;
}

__declspec(dllexport) void WINAPI freeaddrinfo(struct addrinfo *ai)
{
    while (ai) {
        struct addrinfo *next = ai->ai_next;
        if (ai->ai_addr) free (ai->ai_addr);
        free (ai);
        ai = next;
    }
}

__declspec(dllexport) int WINAPI getnameinfo(const struct sockaddr *sa, socklen_t salen,
                                             char *host, DWORD hostlen, char *serv, DWORD servlen, int flags)
{
    (void) salen; (void) flags;
    if (host && hostlen && sa->sa_family == AF_INET) {
        struct sockaddr_in *in = (struct sockaddr_in *) sa;
        struct hostent *he = gethostbyaddr ((const char *) &in->sin_addr, 4, AF_INET);
        if (he) lstrcpynA (host, he->h_name, hostlen);
        else    lstrcpynA (host, inet_ntoa (in->sin_addr), hostlen);
    }
    if (serv && servlen && sa->sa_family == AF_INET)
        wsprintfA (serv, "%d", ntohs (((struct sockaddr_in *) sa)->sin_port));
    return 0;
}

/* inet_pton: missing on Win98 */
__declspec(dllexport) int WINAPI inet_pton(int af, const char *src, void *dst)
{
    struct in_addr a;
    unsigned long v;
    if (af != AF_INET) return -1;
    v = inet_addr (src);
    if (v == INADDR_NONE) return 0;
    a.s_addr = v;
    memcpy (dst, &a, 4);
    return 1;
}
