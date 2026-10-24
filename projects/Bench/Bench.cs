using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class B
{
    static void Run (string name, Action a)
    {
        Stopwatch sw = Stopwatch.StartNew ();
        try { a (); sw.Stop (); Console.WriteLine (name + " " + sw.ElapsedMilliseconds + " ms"); }
        catch (Exception e) { sw.Stop (); Console.WriteLine (name + " FAIL " + e.GetType ().Name + ": " + e.Message); }
    }

    static void Main ()
    {
        Console.WriteLine ("tick-start=" + Environment.TickCount);
        Run ("int-loop-50M", delegate {
            long s = 0;
            for (int i = 0; i < 50000000; i++) s += i;
            if (s == 0) Console.WriteLine ();
        });
        Run ("stringbuilder-100k", delegate {
            StringBuilder sb = new StringBuilder ();
            for (int i = 0; i < 100000; i++) sb.Append ((char) ('0' + i % 10));
            if (sb.Length != 100000) throw new Exception ("len");
        });
        Run ("linq-1M", delegate {
            int [] a = Enumerable.Range (1, 1000000).ToArray ();
            long s = a.Where (x => (x & 1) == 0).Select (x => (long) x * 2).Sum ();
            if (s <= 0) throw new Exception ("sum");
        });
        Run ("sort-200k", delegate {
            Random r = new Random (1);
            int [] a = new int [200000];
            for (int i = 0; i < a.Length; i++) a [i] = r.Next ();
            Array.Sort (a);
        });
        Run ("threads-50", delegate {
            Thread [] ts = new Thread [50];
            for (int i = 0; i < ts.Length; i++) {
                ts [i] = new Thread (new ThreadStart (delegate { Thread.Sleep (10); }));
                ts [i].Start ();
            }
            foreach (Thread t in ts) t.Join ();
        });
        Run ("async-500-delays", delegate {
            Task t = Task.WhenAll (Enumerable.Range (0, 500).Select (i => Task.Delay (1)));
            if (!t.Wait (15000)) throw new TimeoutException ();
        });
        Run ("file-1MB", delegate {
            byte [] b = new byte [1024 * 1024];
            new Random (2).NextBytes (b);
            string p = Path.Combine (Path.GetTempPath (), "bench.bin");
            File.WriteAllBytes (p, b);
            byte [] r = File.ReadAllBytes (p);
            File.Delete (p);
            if (r.Length != b.Length) throw new Exception ("len");
        });
        Run ("socket-500-roundtrips", delegate {
            TcpListener l = new TcpListener (IPAddress.Loopback, 0);
            l.Start ();
            int port = ((IPEndPoint) l.LocalEndpoint).Port;
            Task srv = Task.Run (new Action (delegate {
                using (TcpClient c = l.AcceptTcpClient ())
                using (NetworkStream ns = c.GetStream ()) {
                    byte [] buf = new byte [4];
                    int n;
                    while ((n = ns.Read (buf, 0, 4)) == 4)
                        ns.Write (buf, 0, 4);
                }
            }));
            using (TcpClient c = new TcpClient ()) {
                c.Connect (IPAddress.Loopback, port);
                using (NetworkStream ns = c.GetStream ()) {
                byte [] b = Encoding.ASCII.GetBytes ("ping");
                byte [] r = new byte [4];
                for (int i = 0; i < 500; i++) {
                    ns.Write (b, 0, 4);
                    int got = 0;
                    while (got < 4) { int n = ns.Read (r, got, 4 - got); if (n <= 0) throw new Exception ("eof"); got += n; }
                }
                }
            }
            l.Stop ();
            srv.Wait (5000);
        });
        try {
            IPAddress [] ips = Dns.GetHostAddresses ("localhost");
            Console.WriteLine ("dns-localhost " + ips.Length);
        } catch (Exception e) { Console.WriteLine ("dns-localhost FAIL " + e.GetType ().Name + ": " + e.Message); }
        Console.WriteLine ("tick-end=" + Environment.TickCount);
        Console.WriteLine ("done");
    }
}
