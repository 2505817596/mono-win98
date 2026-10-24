using System;
using System.Threading;
using System.Threading.Tasks;

class A
{
    static async Task<int> Work ()
    {
        Console.WriteLine ("work start tid=" + Thread.CurrentThread.ManagedThreadId);
        await Task.Delay (300);
        Console.WriteLine ("after Delay tid=" + Thread.CurrentThread.ManagedThreadId);
        int sum = await Task.Run (delegate {
            int s = 0;
            for (int i = 1; i <= 100; i++) s += i;
            return s;
        });
        Console.WriteLine ("after Task.Run sum=" + sum + " tid=" + Thread.CurrentThread.ManagedThreadId);
        return sum;
    }

    static void Main ()
    {
        Console.WriteLine ("main start tid=" + Thread.CurrentThread.ManagedThreadId);
        Task<int> t = Work ();
        int r = t.Result;
        Console.WriteLine ("result=" + r);
        var tcs = new TaskCompletionSource<int> ();
        Thread th = new Thread (new ThreadStart (delegate { Thread.Sleep (200); tcs.SetResult (42); }));
        th.IsBackground = true;
        th.Start ();
        Console.WriteLine ("tcs=" + tcs.Task.Result);
        Console.WriteLine ("done");
    }
}
