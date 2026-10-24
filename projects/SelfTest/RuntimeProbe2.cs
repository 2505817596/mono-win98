using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

class P
{
    [DllImport ("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateDirectoryW")]
    static extern bool CreateDirectoryW (string p, IntPtr sa);

    [DllImport ("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, EntryPoint = "CreateDirectoryA")]
    static extern bool CreateDirectoryA (string p, IntPtr sa);

    static void Main ()
    {
        Console.WriteLine ("start");
        bool a = CreateDirectoryA ("C:/WINDOWS/TEMP/p2_a", IntPtr.Zero);
        Console.WriteLine ("CreateDirectoryA=" + a + " err=" + Marshal.GetLastWin32Error ());
        bool w = CreateDirectoryW ("C:/WINDOWS/TEMP/p2_w", IntPtr.Zero);
        Console.WriteLine ("CreateDirectoryW=" + w + " err=" + Marshal.GetLastWin32Error ());

        try {
            Process p = Process.GetCurrentProcess ();
            Console.WriteLine ("proc id=" + p.Id);
            Console.WriteLine ("proc name=" + p.ProcessName);
        } catch (Exception e) { Console.WriteLine ("PROC EX: " + e); }

        try {
            TcpListener l = new TcpListener (IPAddress.Loopback, 0);
            l.Start ();
            Console.WriteLine ("listen ok port=" + ((IPEndPoint) l.LocalEndpoint).Port);
            l.Stop ();
        } catch (Exception e) { Console.WriteLine ("NET EX: " + e); }

        Console.WriteLine ("done");
    }
}
