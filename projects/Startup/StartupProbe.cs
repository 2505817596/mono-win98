using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

class P
{
    [STAThread]
    static void Main ()
    {
        Form f = new Form ();
        f.Text = "Startup Probe";
        f.ClientSize = new Size (320, 140);
        Label l = new Label ();
        l.Text = "probe";
        l.AutoSize = true;
        l.Location = new Point (20, 20);
        f.Controls.Add (l);
        f.Shown += delegate {
            try { File.WriteAllText ("C:/STARTUP.TICK", Environment.TickCount.ToString ()); } catch { }
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer ();
            t.Interval = 400;
            t.Tick += delegate { t.Stop (); f.Close (); };
            t.Start ();
        };
        Application.Run (f);
    }
}
