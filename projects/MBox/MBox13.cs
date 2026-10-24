using System;
using System.Drawing;
using System.Windows.Forms;
class F : Form {
    System.Windows.Forms.Timer t;
    public F() {
        Text = "DrawString test";
        t = new System.Windows.Forms.Timer(); t.Interval = 3000;
        t.Tick += delegate { t.Stop();
            using (Graphics g = this.CreateGraphics())
                g.DrawString("On-screen GDI+", this.Font, Brushes.Black, 20, 20);
            Text = "DrawString survived"; };
        t.Start();
    }
    [STAThread] static void Main() { Application.Run(new F()); }
}
