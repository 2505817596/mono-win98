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
            Process p = Process.GetCurrentProcess ();
            Console.WriteLine ("pid=" + p.Id);
            Console.WriteLine ("name=" + p.ProcessName);
        } catch (Exception e) { Console.WriteLine ("PROC EX " + e); }

        try {
            string d = "C:/WINDOWS/TEMP/p3_dir";
            Console.WriteLine ("exists-before=" + Directory.Exists (d));
            Directory.CreateDirectory (d);
            Console.WriteLine ("created=" + Directory.Exists (d));
            File.WriteAllText (d + "/a.txt", "x");
            Console.WriteLine ("files=" + Directory.GetFiles (d).Length);
            Directory.Delete (d, true);
            Console.WriteLine ("deleted");
        } catch (Exception e) { Console.WriteLine ("DIR EX " + e); }

        try {
            Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Bind (new IPEndPoint (IPAddress.Loopback, 0));
            s.Listen (1);
            int port = ((IPEndPoint) s.LocalEndPoint).Port;
            Console.WriteLine ("port=" + port);
            Socket c = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            c.Connect (new IPEndPoint (IPAddress.Loopback, port));
            Console.WriteLine ("connected");
            Socket a = s.Accept ();
            Console.WriteLine ("accepted");
            a.Close (); c.Close (); s.Close ();
        } catch (Exception e) { Console.WriteLine ("NET EX " + e); }

        Console.WriteLine ("done");
    }
}
