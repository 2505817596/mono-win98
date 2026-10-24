using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

class P
{
    static void Main ()
    {
        try {
            string t = Path.GetTempPath ();
            Console.WriteLine ("temp=" + t);
            string d = Path.Combine (t, "probe_dir");
            Directory.CreateDirectory (d);
            Console.WriteLine ("created=" + Directory.Exists (d));
            Directory.Delete (d);
            Console.WriteLine ("deleted");
        } catch (Exception e) { Console.WriteLine ("DIR EX: " + e); }

        try {
            Process p = Process.GetCurrentProcess ();
            Console.WriteLine ("pid=" + p.Id + " name=" + p.ProcessName);
        } catch (Exception e) { Console.WriteLine ("PROC EX: " + e); }

        try {
            TcpListener l = new TcpListener (IPAddress.Loopback, 0);
            l.Start ();
            int port = ((IPEndPoint) l.LocalEndpoint).Port;
            Console.WriteLine ("listen port=" + port);
            using (TcpClient c = new TcpClient ()) {
                c.Connect (IPAddress.Loopback, port);
                Console.WriteLine ("connected");
            }
            l.Stop ();
        } catch (Exception e) { Console.WriteLine ("NET EX: " + e); }

        Console.WriteLine ("done");
    }
}
