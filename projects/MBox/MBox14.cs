using System;
using System.Windows.Forms;
class P {
    [STAThread] static void Main() {
        Form d = new Form();
        d.Text = "Only dialog";
        d.Width = 260; d.Height = 160;
        d.ShowDialog();
        // nothing after: app would exit
    }
}
