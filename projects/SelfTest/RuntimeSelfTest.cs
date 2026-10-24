using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

public class RuntimeSelfTest
{
    static int pass, fail;
    static readonly StringBuilder report = new StringBuilder ();
    static readonly List<string> failures = new List<string> ();

    [DllImport ("kernel32.dll")]
    static extern uint GetTickCount ();

    static void Log (string s)
    {
        report.AppendLine (s);
        Console.WriteLine (s);
    }

    static void Check (string name, Action a)
    {
        try {
            a ();
            pass++;
            Log ("PASS " + name);
        } catch (Exception e) {
            fail++;
            string m = name + " : " + e.GetType ().Name + ": " + e.Message;
            failures.Add (m);
            Log ("FAIL " + m);
        }
    }

    static void CheckAsync (string name, Func<Task> f, int timeoutMs = 8000)
    {
        try {
            Task t = f ();
            if (!t.Wait (timeoutMs))
                throw new TimeoutException ("timeout " + timeoutMs + "ms");
            pass++;
            Log ("PASS " + name);
        } catch (Exception e) {
            Exception ex = e;
            while ((ex is AggregateException || ex is TypeInitializationException) && ex.InnerException != null)
                ex = ex.InnerException;
            fail++;
            string m = name + " : " + ex.GetType ().Name + ": " + ex.Message;
            failures.Add (m);
            Log ("FAIL " + m);
        }
    }

    static int Main ()
    {
        Log ("Win98 runtime self test, " + Environment.OSVersion.VersionString);

        Check ("core/string", delegate {
            string s = string.Format ("{0}-{1:000}", "a", 7);
            if (s != "a-007") throw new Exception (s);
            if (new StringBuilder ("abc").Append ("def").ToString () != "abcdef") throw new Exception ("sb");
        });

        Check ("core/datetime", delegate {
            DateTime d = new DateTime (2026, 10, 24, 12, 0, 0);
            TimeSpan t = d.AddDays (1.5) - d;
            if (Math.Abs (t.TotalHours - 36) > 0.001) throw new Exception ("span " + t);
            if (DateTime.Now.Year < 2000) throw new Exception ("now");
        });

        Check ("core/timezone", delegate {
            TimeZoneInfo z = TimeZoneInfo.Local;
            if (z == null) throw new Exception ("local null");
            string id = z.Id;
            if (id == null || id.Length == 0) throw new Exception ("id empty");
        });

        Check ("core/math-decimal-bigint", delegate {
            if (Math.Sqrt (144) != 12) throw new Exception ("sqrt");
            decimal m = 1.5m * 4m;
            if (m != 6m) throw new Exception ("dec");
            BigInteger b = BigInteger.Pow (2, 100);
            if (b.ToString () != "1267650600228229401496703205376") throw new Exception ("bigint " + b);
        });

        Check ("core/collections", delegate {
            List<int> l = new List<int> { 3, 1, 2 };
            l.Sort ();
            if (string.Join (",", l) != "1,2,3") throw new Exception ("sort");
            Dictionary<string, int> d = new Dictionary<string, int> ();
            d ["a"] = 1; d ["b"] = 2;
            if (d.Count != 2 || d ["b"] != 2) throw new Exception ("dict");
            HashSet<int> h = new HashSet<int> { 1, 2, 2 };
            if (h.Count != 2) throw new Exception ("hashset");
            Queue<int> q = new Queue<int> (); q.Enqueue (1); q.Dequeue ();
            Stack<int> st = new Stack<int> (); st.Push (5);
            if (st.Peek () != 5) throw new Exception ("stack");
        });

        Check ("core/linq", delegate {
            int [] a = { 1, 2, 3, 4, 5, 6 };
            var r = from x in a where x % 2 == 0 select x * 10;
            if (string.Join (",", r.ToArray ()) != "20,40,60") throw new Exception ("where");
            var g = a.GroupBy (x => x % 3).OrderBy (x => x.Key).Select (x => x.Key + ":" + x.Count ()).ToArray ();
            if (g.Length != 3) throw new Exception ("group");
            var j = a.Join (a, x => x, y => y, (x, y) => x * y).Sum ();
            if (j != 91) throw new Exception ("join " + j);
        });

        Check ("core/reflection", delegate {
            Type t = typeof (List<>);
            if (!t.IsGenericTypeDefinition) throw new Exception ("generic");
            object o = Activator.CreateInstance (typeof (Version), new object [] { 1, 2, 3, 4 });
            if (o.ToString () != "1.2.3.4") throw new Exception ("activator");
            MethodInfo m = typeof (string).GetMethod ("ToUpper", new Type [] { });
            if (m == null) throw new Exception ("method");
        });

        Check ("core/expression", delegate {
            System.Linq.Expressions.Expression<Func<int, int>> e = x => x * x + 1;
            Func<int, int> f = e.Compile ();
            if (f (5) != 26) throw new Exception ("expr " + f (5));
        });

        Check ("core/exceptions", delegate {
            bool fin = false;
            try {
                try { throw new InvalidOperationException ("inner"); }
                finally { fin = true; }
            } catch (InvalidOperationException e) {
                if (e.Message != "inner") throw new Exception ("msg");
            }
            if (!fin) throw new Exception ("finally");
        });

        Check ("io/text-utf8", delegate {
            string p = Path.Combine (Path.GetTempPath (), "rt_utf8.txt");
            string s = "hello \u4f60\u597d";
            File.WriteAllText (p, s, Encoding.UTF8);
            string r = File.ReadAllText (p, Encoding.UTF8);
            File.Delete (p);
            if (r != s) throw new Exception ("roundtrip");
        });

        Check ("io/text-gbk", delegate {
            Encoding gbk = Encoding.GetEncoding (936);
            string p = Path.Combine (Path.GetTempPath (), "rt_gbk.txt");
            string s = "\u4e2d\u6587\u6d4b\u8bd5";
            File.WriteAllText (p, s, gbk);
            byte [] raw = File.ReadAllBytes (p);
            string r = File.ReadAllText (p, gbk);
            File.Delete (p);
            if (raw.Length != 8) throw new Exception ("len=" + raw.Length);
            if (r != s) throw new Exception ("roundtrip=" + r);
        });

        Check ("io/streams", delegate {
            string p = Path.Combine (Path.GetTempPath (), "rt_bin.dat");
            using (FileStream fs = new FileStream (p, FileMode.Create, FileAccess.Write)) {
                byte [] b = { 1, 2, 3, 4, 5 };
                fs.Write (b, 0, b.Length);
            }
            using (FileStream fs = new FileStream (p, FileMode.Open, FileAccess.Read)) {
                if (fs.Length != 5) throw new Exception ("len");
                if (fs.ReadByte () != 1) throw new Exception ("byte");
            }
            File.Delete (p);
        });

        Check ("io/path-directory", delegate {
            string d = Path.Combine (Path.GetTempPath (), "rt_dir_" + Environment.TickCount);
            Directory.CreateDirectory (d);
            File.WriteAllText (Path.Combine (d, "a.txt"), "x");
            string [] files = Directory.GetFiles (d);
            Directory.Delete (d, true);
            if (files.Length != 1) throw new Exception ("files=" + files.Length);
        });

        Check ("io/drives", delegate {
            DriveInfo [] ds = DriveInfo.GetDrives ();
            if (ds.Length == 0) throw new Exception ("none");
            bool found = false;
            foreach (DriveInfo d in ds) {
                try { if (d.Name.ToUpper ().StartsWith ("C")) { long sz = d.TotalSize; if (sz > 0) found = true; } } catch { }
            }
            if (!found) throw new Exception ("c: size");
        });

        Check ("xml/xmldocument", delegate {
            XmlDocument doc = new XmlDocument ();
            doc.LoadXml ("<r><a v='1'/><a v='2'/></r>");
            if (doc.SelectNodes ("//a").Count != 2) throw new Exception ("nodes");
        });

        Check ("xml/xdocument", delegate {
            XDocument d = XDocument.Parse ("<r><i>1</i><i>2</i></r>");
            int sum = d.Root.Elements ("i").Sum (e => int.Parse (e.Value));
            if (sum != 3) throw new Exception ("sum=" + sum);
        });

        Check ("xml/serializer", delegate {
            Person p = new Person { Name = "Mono", Age = 6 };
            XmlSerializer s = new XmlSerializer (typeof (Person));
            StringWriter sw = new StringWriter ();
            s.Serialize (sw, p);
            Person q = (Person) s.Deserialize (new StringReader (sw.ToString ()));
            if (q.Name != "Mono" || q.Age != 6) throw new Exception ("roundtrip");
        });

        Check ("serialization/binary", delegate {
            Person p = new Person { Name = "Mono", Age = 6 };
            BinaryFormatter bf = new BinaryFormatter ();
            MemoryStream ms = new MemoryStream ();
            bf.Serialize (ms, p);
            ms.Position = 0;
            Person q = (Person) bf.Deserialize (ms);
            if (q.Name != "Mono" || q.Age != 6) throw new Exception ("roundtrip");
        });

        Check ("regex", delegate {
            System.Text.RegularExpressions.Regex re =
                new System.Text.RegularExpressions.Regex (@"(\d+)-(\d+)");
            System.Text.RegularExpressions.Match m = re.Match ("abc 12-34 def");
            if (!m.Success || m.Groups [1].Value != "12" || m.Groups [2].Value != "34")
                throw new Exception ("match");
        });

        Check ("crypto/md5-sha256", delegate {
            byte [] h = MD5.Create ().ComputeHash (Encoding.ASCII.GetBytes ("abc"));
            string md5 = BitConverter.ToString (h).Replace ("-", "").ToLower ();
            if (md5 != "900150983cd24fb0d6963f7d28e17f72") throw new Exception ("md5=" + md5);
            byte [] s = SHA256.Create ().ComputeHash (Encoding.ASCII.GetBytes ("abc"));
            string sha = BitConverter.ToString (s).Replace ("-", "").ToLower ();
            if (sha != "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad")
                throw new Exception ("sha=" + sha);
        });

        Check ("crypto/rng", delegate {
            byte [] b = new byte [32];
            using (RandomNumberGenerator r = RandomNumberGenerator.Create ())
                r.GetBytes (b);
            bool nz = false;
            foreach (byte x in b) if (x != 0) { nz = true; break; }
            if (!nz) throw new Exception ("all zero");
        });

        Check ("globalization/culture", delegate {
            CultureInfo c = new CultureInfo ("zh-CN");
            if (c.Name != "zh-CN") throw new Exception ("name=" + c.Name);
            if (string.Compare ("a", "b", false, c) >= 0) throw new Exception ("compare");
        });

        Check ("globalization/encoding936", delegate {
            Encoding e = Encoding.GetEncoding (936);
            byte [] b = { 0xD6, 0xD0, 0xCE, 0xC4 };
            string s = e.GetString (b);
            if (s != "\u4e2d\u6587") throw new Exception ("s=" + s);
            byte [] back = e.GetBytes ("\u4e2d\u6587");
            if (back.Length != 4 || back [0] != 0xD6) throw new Exception ("bytes");
        });

        Check ("data/datatable", delegate {
            DataTable t = new DataTable ("t");
            t.Columns.Add ("name", typeof (string));
            t.Columns.Add ("val", typeof (int));
            t.Rows.Add ("a", 1);
            t.Rows.Add ("b", 2);
            DataRow [] r = t.Select ("val > 1");
            if (t.Rows.Count != 2 || r.Length != 1) throw new Exception ("table");
        });

        Check ("diagnostics/stopwatch-process", delegate {
            Stopwatch sw = Stopwatch.StartNew ();
            Thread.Sleep (100);
            sw.Stop ();
            if (sw.ElapsedMilliseconds < 50) throw new Exception ("sw=" + sw.ElapsedMilliseconds);
            // Win9x PIDs are full 32-bit values and often appear negative when
            // read through Mono's signed Process.Id; non-zero is the contract.
            if (Process.GetCurrentProcess ().Id == 0) throw new Exception ("pid");
        });

        Check ("pinvoke/tickcount", delegate {
            uint a = GetTickCount ();
            if (a == 0) throw new Exception ("zero");
        });

        Check ("gc/weakreference", delegate {
            WeakReference w = MakeWeak ();
            GC.Collect ();
            GC.WaitForPendingFinalizers ();
            GC.Collect ();
            if (w.IsAlive) throw new Exception ("still alive");
        });

        Check ("thread/monitor", delegate {
            object gate = new object ();
            int v = 0;
            Thread th = new Thread (new ThreadStart (delegate {
                lock (gate) { v = 1; }
            }));
            th.Start ();
            th.Join ();
            lock (gate) { if (v != 1) throw new Exception ("v=" + v); }
        });

        Check ("thread/mutex-semaphore", delegate {
            using (Mutex m = new Mutex (false, "rt_selftest_mutex")) {
                if (!m.WaitOne (2000)) throw new Exception ("mutex wait");
                m.ReleaseMutex ();
            }
            using (Semaphore s = new Semaphore (1, 1)) {
                if (!s.WaitOne (2000)) throw new Exception ("sem wait");
                s.Release ();
            }
        });

        Check ("thread/threadpool", delegate {
            ManualResetEvent ev = new ManualResetEvent (false);
            int v = 0;
            ThreadPool.QueueUserWorkItem (delegate { v = 5; ev.Set (); });
            if (!ev.WaitOne (5000)) throw new TimeoutException ("pool");
            if (v != 5) throw new Exception ("v=" + v);
        });

        CheckAsync ("task/run-result", delegate {
            return Task.Run (new Func<int> (delegate {
                int s = 0;
                for (int i = 1; i <= 100; i++) s += i;
                if (s != 5050) throw new Exception ("sum " + s);
                return 0;
            }));
        });

        CheckAsync ("task/parallel-for", delegate {
            int total = 0;
            Parallel.For (0, 1000, delegate (int i) { Interlocked.Add (ref total, 1); });
            if (total != 1000) throw new Exception ("total=" + total);
            return Task.FromResult (0);
        });

        CheckAsync ("task/async-await", async delegate {
            int v = 0;
            await Task.Delay (50);
            v = await Task.Run (new Func<int> (delegate { return 7; }));
            if (v != 7) throw new Exception ("v=" + v);
        });

        CheckAsync ("task/cancellation", delegate {
            CancellationTokenSource cts = new CancellationTokenSource ();
            Task t = Task.Delay (5000, cts.Token);
            cts.Cancel ();
            try { t.Wait (3000); } catch (AggregateException) { return Task.FromResult (0); }
            throw new Exception ("not cancelled");
        });

        CheckAsync ("net/tcp-loopback", delegate {
            return Task.Run (delegate {
                System.Net.Sockets.TcpListener srv =
                    new System.Net.Sockets.TcpListener (System.Net.IPAddress.Loopback, 0);
                srv.Start ();
                int port = ((System.Net.IPEndPoint) srv.LocalEndpoint).Port;
                Task<string> server = Task.Run (new Func<string> (delegate {
                    using (System.Net.Sockets.TcpClient c = srv.AcceptTcpClient ())
                    using (NetworkStream ns = c.GetStream ()) {
                        byte [] b = new byte [4];
                        int n = ns.Read (b, 0, b.Length);
                        byte [] outb = Encoding.ASCII.GetBytes ("pong");
                        ns.Write (outb, 0, outb.Length);
                        return Encoding.ASCII.GetString (b, 0, n);
                    }
                }));
                using (System.Net.Sockets.TcpClient c = new System.Net.Sockets.TcpClient ()) {
                    c.Connect (System.Net.IPAddress.Loopback, port);
                    using (NetworkStream ns = c.GetStream ()) {
                        byte [] b = Encoding.ASCII.GetBytes ("ping");
                        ns.Write (b, 0, b.Length);
                        byte [] r = new byte [4];
                        int n = ns.Read (r, 0, r.Length);
                        if (Encoding.ASCII.GetString (r, 0, n) != "pong") throw new Exception ("reply");
                    }
                }
                if (server.Result != "ping") throw new Exception ("server got " + server.Result);
                srv.Stop ();
            });
        });

        Check ("net/uri", delegate {
            Uri u = new Uri ("http://example.com:8080/a/b?x=1");
            if (u.Host != "example.com" || u.Port != 8080 || u.AbsolutePath != "/a/b")
                throw new Exception ("uri");
        });

        string summary = "SUMMARY pass=" + pass + " fail=" + fail;
        Log (summary);
        try { File.WriteAllText ("C:\\RTSELFTEST.TXT", report.ToString ()); } catch { }
        return fail;
    }

    static WeakReference MakeWeak ()
    {
        object o = new object ();
        return new WeakReference (o);
    }

    [Serializable]
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
