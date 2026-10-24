using System;
using System.Windows.Forms;
class F : Form {
    System.Windows.Forms.Timer t;
    public F() {
        Text = "Hide then dialog";
        t = new System.Windows.Forms.Timer(); t.Interval = 4000;
        t.Tick += delegate {
            t.Stop();
            Hide();
            Form d = new Form(); d.Text = "Hidden-owner dialog";
            d.Width = 260; d.Height = 160;
            d.ShowDialog(this);
            Show();
            Text = "returned ok";
        };
        t.Start();
    }
    [STAThread] static void Main() { Application.Run(new F()); }
}
