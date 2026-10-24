using System;
using System.Drawing;
using System.Windows.Forms;

class HelloWin98
{
    [STAThread]
    static void Main ()
    {
        Form form = new Form ();
        form.Text = "Hello Win98";
        form.StartPosition = FormStartPosition.CenterScreen;
        form.ClientSize = new Size (340, 160);

        Label label = new Label ();
        label.Text = "Mono 6.13.0 on Windows 98 SE";
        label.SetBounds (20, 20, 300, 24);
        form.Controls.Add (label);

        Button button = new Button ();
        button.Text = "Click me";
        button.SetBounds (20, 60, 120, 30);
        button.Click += delegate {
            label.Text = "Hello from .NET on Win98!";
        };
        form.Controls.Add (button);

        Application.Run (form);
    }
}
