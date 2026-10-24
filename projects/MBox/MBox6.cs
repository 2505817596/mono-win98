using System;
using System.Windows.Forms;
class F : Form {
    Timer t;
    public F() {
        Text = "WA test";
        t = new Timer(); t.Interval = 2500;
        t.Tick += delegate { t.Stop(); var r = Screen.GetWorkingArea(this);
            Text = "WA " + r.Width + "x" + r.Height; };
        t.Start();
    }
    [STAThread] static void Main() { Application.Run(new F()); }
}
