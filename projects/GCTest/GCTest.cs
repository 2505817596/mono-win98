using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

class GCTest
{
    static int pass, fail;

    static void Check (string n, Action a)
    {
        try { a (); pass++; Console.WriteLine ("PASS " + n); }
        catch (Exception e) { fail++; Console.WriteLine ("FAIL " + n + " : " + e.GetType ().Name + ": " + e.Message); }
    }

    class Fin
    {
        public static int Count;
        ~Fin () { Interlocked.Increment (ref Count); }
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    static void AllocChurn (int n)
    {
        for (int i = 0; i < n; i++) { byte [] b = new byte [1024]; b [0] = (byte) i; }
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    static WeakReference MakeWeak ()
    {
        object o = new object ();
        return new WeakReference (o);
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    static void AllocList (int n)
    {
        List<object> l = new List<object> ();
        for (int i = 0; i < n; i++) l.Add (new byte [64]);
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    static void AllocLarge (int n)
    {
        for (int i = 0; i < n; i++) { byte [] big = new byte [200000]; big [0] = 1; }
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    static GCHandle MakeHandle (out WeakReference w)
    {
        object o = new object ();
        w = new WeakReference (o);
        return GCHandle.Alloc (o, GCHandleType.Normal);
    }

    static int Counts ()
    {
        return GC.CollectionCount (0) + GC.CollectionCount (1) + GC.CollectionCount (2);
    }

    static void Main ()
    {
        Console.WriteLine ("gc-test start, " + Environment.Version);

        Check ("gc/api-basics", delegate {
            long m = GC.GetTotalMemory (true);
            if (m <= 0) throw new Exception ("total=" + m);
        });

        Check ("gc/collection-counts", delegate {
            int c0 = Counts ();
            AllocChurn (20000);
            GC.Collect (); GC.WaitForPendingFinalizers (); GC.Collect ();
            int c1 = Counts ();
            if (c1 <= c0) throw new Exception ("counts " + c0 + " -> " + c1);
        });

        Check ("gc/weakreference", delegate {
            WeakReference w = MakeWeak ();
            GC.Collect (); GC.WaitForPendingFinalizers (); GC.Collect ();
            if (w.IsAlive) throw new Exception ("still alive");
        });

        Check ("gc/finalizers", delegate {
            Fin.Count = 0;
            for (int i = 0; i < 200; i++) { Fin f = new Fin (); }
            GC.Collect (); GC.WaitForPendingFinalizers (); GC.Collect ();
            if (Fin.Count < 50) throw new Exception ("finalized=" + Fin.Count);
        });

        Check ("gc/reclaim", delegate {
            long before = GC.GetTotalMemory (true);
            AllocList (100000);
            long during = GC.GetTotalMemory (false);
            if (during <= before) throw new Exception ("no growth");
            GC.Collect (); GC.WaitForPendingFinalizers (); GC.Collect ();
            long after = GC.GetTotalMemory (true);
            if (after > before + 3 * 1024 * 1024)
                throw new Exception ("before=" + before + " during=" + during + " after=" + after);
        });

        Check ("gc/large-objects", delegate {
            long before = GC.GetTotalMemory (true);
            AllocLarge (50);
            long during = GC.GetTotalMemory (false);
            GC.Collect (); GC.WaitForPendingFinalizers (); GC.Collect ();
            long after = GC.GetTotalMemory (true);
            if (during <= before) throw new Exception ("no growth");
            if (after > before + 8 * 1024 * 1024)
                throw new Exception ("los not reclaimed " + before + " -> " + after);
        });

        Check ("gc/gchandle", delegate {
            WeakReference w;
            GCHandle h = MakeHandle (out w);
            GC.Collect ();
            if (!w.IsAlive) throw new Exception ("collected while handle held");
            h.Free ();
            GC.Collect (); GC.WaitForPendingFinalizers (); GC.Collect ();
            if (w.IsAlive) throw new Exception ("not collected after free");
        });

        Check ("gc/thread-churn", delegate {
            for (int t = 0; t < 4; t++) {
                Thread th = new Thread (new ThreadStart (delegate {
                    for (int i = 0; i < 5000; i++) { byte [] x = new byte [256]; x [0] = (byte) i; }
                }));
                th.Start ();
                th.Join ();
            }
            GC.Collect (); GC.WaitForPendingFinalizers (); GC.Collect ();
        });

        Console.WriteLine ("SUMMARY pass=" + pass + " fail=" + fail);
        Environment.Exit (fail);
    }
}
