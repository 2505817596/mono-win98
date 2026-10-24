using System;
using System.Net;
using System.Net.Sockets;

class P
{
    static void Main ()
    {
        Console.WriteLine ("start");
        Console.Out.Flush ();
        Socket s = null, c = null, a = null;
        try {
            s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine ("socket ok"); Console.Out.Flush ();
            s.Bind (new IPEndPoint (IPAddress.Loopback, 0));
            Console.WriteLine ("bind ok"); Console.Out.Flush ();
            s.Listen (1);
            int port = ((IPEndPoint) s.LocalEndPoint).Port;
            Console.WriteLine ("listen port=" + port); Console.Out.Flush ();
            c = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            c.Connect (new IPEndPoint (IPAddress.Loopback, port));
            Console.WriteLine ("connected"); Console.Out.Flush ();
            a = s.Accept ();
            Console.WriteLine ("accepted"); Console.Out.Flush ();
            byte [] b = System.Text.Encoding.ASCII.GetBytes ("ping");
            c.Send (b);
            byte [] r = new byte [4];
            int n = a.Receive (r);
            Console.WriteLine ("server got " + System.Text.Encoding.ASCII.GetString (r, 0, n)); Console.Out.Flush ();
            a.Send (b);
            n = c.Receive (r);
            Console.WriteLine ("client got " + System.Text.Encoding.ASCII.GetString (r, 0, n)); Console.Out.Flush ();
        } catch (Exception e) { Console.WriteLine ("SOCK EX " + e); Console.Out.Flush (); }
        finally {
            try { if (a != null) a.Close (); } catch {}
            try { if (c != null) c.Close (); } catch {}
            try { if (s != null) s.Close (); } catch {}
        }
        Console.WriteLine ("done");
    }
}
