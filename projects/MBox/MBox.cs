using System;
using System.Windows.Forms;
class F : Form {
    public F() {
        Text = "MBox test";
        Button b = new Button();
        b.Text = "Show";
        b.Location = new System.Drawing.Point(20, 20);
        b.Click += delegate { MessageBox.Show(this, "Hello box", "Greeting", MessageBoxButtons.OK, MessageBoxIcon.Information); };
        Controls.Add(b);
    }
    [STAThread] static void Main() { Application.Run(new F()); }
}
