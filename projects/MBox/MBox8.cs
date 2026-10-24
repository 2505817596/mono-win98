using System;
using System.Windows.Forms;
class F : Form {
    Timer t;
    public F() {
        Text = "Dlg test";
        t = new Timer(); t.Interval = 5000;
        t.Tick += delegate { t.Stop(); Form d = new Form(); d.Text = "Second dialog";
            d.Width = 260; d.Height = 160; d.ShowDialog(this); Text = "Dlg survived"; };
        t.Start();
    }
    [STAThread] static void Main() { Application.Run(new F()); }
}
