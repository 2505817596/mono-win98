using System;
using System.Threading;
using System.Windows.Forms;
class F : Form {
    System.Windows.Forms.Timer t;
    public F() {
        Text = "MsgBox bigstack";
        t = new System.Windows.Forms.Timer(); t.Interval = 5000;
        t.Tick += delegate { t.Stop();
            MessageBox.Show(this, "Hello box", "Greeting", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Text = "MsgBox survived"; };
        t.Start();
    }
    static void Run() { Application.Run(new F()); }
    [STAThread] static void Main() {
        Thread th = new Thread(new ThreadStart(Run), 64 * 1024 * 1024);
        th.SetApartmentState(ApartmentState.STA);
        th.Start();
        th.Join();
    }
}
