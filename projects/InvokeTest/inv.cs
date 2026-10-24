using System;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

class T
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "PostMessageW")]
    static extern bool PostMessage (IntPtr h, int msg, IntPtr w, IntPtr l);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "FindWindowW")]
    static extern IntPtr FindWindow (string cls, string title);

    static void Main ()
    {
        Form f = new Form ();
        Label l = new Label ();
        l.Text = "before";
        l.AutoSize = true;
        f.Controls.Add (l);
        f.Shown += delegate {
            Thread th = new Thread (delegate () {
                Thread.Sleep (500);
                bool ok = PostMessage (f.Handle, 0x404, (IntPtr)5, (IntPtr)6);
                Console.WriteLine ("posted 404 ok=" + ok + " err=" + Marshal.GetLastWin32Error ());
                IntPtr foster = FindWindow (null, "Foster Parent Window");
                bool ok2 = PostMessage (foster, 0x404, (IntPtr)7, (IntPtr)8);
                Console.WriteLine ("foster=" + foster + " posted 404 ok=" + ok2 + " err=" + Marshal.GetLastWin32Error ());
                try {
                    f.Invoke ((MethodInvoker) delegate { l.Text = "invoked"; });
                    Console.WriteLine ("invoke returned");
                } catch (Exception e) {
                    Console.WriteLine ("EX " + e.GetType ().Name + ": " + e.Message);
                }
            });
            th.IsBackground = true;
            th.Start ();
        };
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer ();
        t.Interval = 700;
        int ticks = 0;
        t.Tick += delegate {
            ticks++;
            Console.WriteLine ("tick " + ticks + " label=" + l.Text);
            if (ticks == 2) {
                f.BeginInvoke ((MethodInvoker) delegate { Console.WriteLine ("begininvoke executed"); });
                Console.WriteLine ("begininvoke queued");
            }
            if (ticks >= 4) { t.Stop (); f.Close (); }
        };
        t.Start ();
        Application.Run (f);
        Console.WriteLine ("done");
    }
}
