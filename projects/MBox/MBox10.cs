using System;
using System.Threading;
using System.Windows.Forms;
class F : Form {
    System.Windows.Forms.Timer t;
    public F() {
        Text = "BigStack test";
        t = new System.Windows.Forms.Timer(); t.Interval = 5000;
        t.Tick += delegate { t.Stop(); Form d = new Form(); d.Text = "Second dialog";
            d.Width = 260; d.Height = 160; d.ShowDialog(this); Text = "Dlg survived"; };
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
