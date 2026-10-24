using System;
using System.Windows.Forms;

class Demo2 {
    [STAThread]
    static void Main() {
        Form f = new Form();
        f.Text = "Strips test";
        f.Width = 500; f.Height = 300;
        MenuStrip ms = new MenuStrip();
        ms.Items.Add("File");
        ToolStrip ts = new ToolStrip();
        ts.Items.Add("ToolButton");
        StatusStrip ss = new StatusStrip();
        ss.Items.Add("Ready");
        f.MainMenuStrip = ms;
        f.Controls.Add(ms);
        f.Controls.Add(ts);
        f.Controls.Add(ss);
        Application.Run(f);
    }
}
