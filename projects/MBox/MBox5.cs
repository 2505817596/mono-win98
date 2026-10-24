using System;
using System.Windows.Forms;
class F : Form {
    Timer t;
    public F() {
        Text = "MT test";
        t = new Timer(); t.Interval = 2500;
        t.Tick += delegate { t.Stop(); var s = TextRenderer.MeasureText("Hello box", this.Font);
            Text = "MT " + s.Width + "x" + s.Height; };
        t.Start();
    }
    [STAThread] static void Main() { Application.Run(new F()); }
}
