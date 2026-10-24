using System;
using System.Threading;
using System.Windows.Forms;
class F : Form {
    System.Windows.Forms.Timer t;
    public F() {
        Text = "Plain bigstack";
        t = new System.Windows.Forms.Timer(); t.Interval = 4000;
        t.Tick += delegate { t.Stop(); MessageBox.Show("Hello box"); Text = "Plain survived"; };
        t.Start();
    }
    static void Run() { Application.Run(new F()); }
    [STAThread] static void Main() {
        Thread th = new Thread(new ThreadStart(Run), 64 * 1024 * 1024);
        th.SetApartmentState(ApartmentState.STA);
        th.Start(); th.Join();
    }
}
