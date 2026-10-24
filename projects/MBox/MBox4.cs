using System;
using System.Windows.Forms;
class F : Form {
    Timer t;
    public F() {
        Text = "MBox test";
        t = new Timer();
        t.Interval = 3000;
        t.Tick += delegate {
            t.Stop();
            MessageBox.Show(this, "Hello box");
            Text = "MBox survived";
        };
        t.Start();
    }
    [STAThread] static void Main() { Application.Run(new F()); }
}
