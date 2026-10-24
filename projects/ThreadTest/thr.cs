using System;
using System.Threading;

class T
{
    static void Main ()
    {
        Console.WriteLine ("main tid=" + Thread.CurrentThread.ManagedThreadId);
        Thread t = new Thread (delegate () {
            Console.WriteLine ("worker tid=" + Thread.CurrentThread.ManagedThreadId);
            int sum = 0;
            for (int i = 1; i <= 100; i++) sum += i;
            Console.WriteLine ("sum=" + sum);
        });
        t.IsBackground = true;
        t.Start ();
        t.Join (5000);
        Console.WriteLine ("joined=" + !t.IsAlive);
        Console.WriteLine ("done");
    }
}
