using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

class Dialogs : Form
{
    static StreamWriter log;
    static void Note (string s)
    {
        if (log != null) { log.WriteLine (s); log.Flush (); }
    }

    Label result;
    DataGridView grid;
    RichTextBox rtb;
    ToolTip tip = new ToolTip ();
    ContextMenuStrip cm;
    NotifyIcon tray;

    void SetResult (string s)
    {
        if (result != null) result.Text = s;
        Note (s);
    }

    Button AddButton (string text, EventHandler h, int index)
    {
        Button b = new Button ();
        b.Text = text;
        b.SetBounds (10, 10 + index * 34, 200, 28);
        b.Click += delegate {
            try { h (b, EventArgs.Empty); }
            catch (Exception e) { SetResult (text + " EX " + e.GetType ().Name + ": " + e.Message); }
        };
        Controls.Add (b);
        return b;
    }

    public Dialogs ()
    {
        Text = "Win98 Dialog Tests";
        ClientSize = new Size (620, 520);
        StartPosition = FormStartPosition.CenterScreen;

        result = new Label ();
        result.SetBounds (230, 10, 380, 60);
        result.Text = "ready";
        Controls.Add (result);

        int i = 0;
        AddButton ("OpenFileDialog", delegate {
            using (OpenFileDialog d = new OpenFileDialog ()) {
                d.Filter = "Text files|*.txt|All files|*.*";
                SetResult ("OpenFile ret=" + d.ShowDialog (this) + " file=" + d.FileName);
            }
        }, i++);

        AddButton ("SaveFileDialog", delegate {
            using (SaveFileDialog d = new SaveFileDialog ()) {
                d.Filter = "Text files|*.txt";
                d.FileName = "test.txt";
                SetResult ("SaveFile ret=" + d.ShowDialog (this) + " file=" + d.FileName);
            }
        }, i++);

        AddButton ("FolderBrowserDialog", delegate {
            using (FolderBrowserDialog d = new FolderBrowserDialog ()) {
                SetResult ("Folder ret=" + d.ShowDialog (this) + " path=" + d.SelectedPath);
            }
        }, i++);

        AddButton ("PrintDialog", delegate {
            using (PrintDialog d = new PrintDialog ()) {
                d.Document = MakeDocument ();
                SetResult ("Print ret=" + d.ShowDialog (this));
            }
        }, i++);

        AddButton ("PrintPreviewDialog", delegate {
            using (PrintPreviewDialog d = new PrintPreviewDialog ()) {
                d.Document = MakeDocument ();
                SetResult ("Preview opening");
                d.ShowDialog (this);
                SetResult ("Preview closed");
            }
        }, i++);

        AddButton ("PageSetupDialog", delegate {
            using (PageSetupDialog d = new PageSetupDialog ()) {
                d.Document = MakeDocument ();
                SetResult ("PageSetup ret=" + d.ShowDialog (this));
            }
        }, i++);

        AddButton ("ContextMenu", delegate {
            if (cm == null) {
                cm = new ContextMenuStrip ();
                cm.Items.Add ("Item 1");
                cm.Items.Add ("Item 2");
                cm.Items.Add ("Item 3");
            }
            cm.Show (this, new Point (300, 200));
            SetResult ("ContextMenu shown");
        }, i++);

        AddButton ("ToolTip", delegate {
            tip.Show ("ToolTip text visible", this, new Point (330, 160), 4000);
            SetResult ("ToolTip shown");
        }, i++);

        AddButton ("GridEdit", delegate {
            if (grid == null) {
                grid = new DataGridView ();
                grid.SetBounds (230, 90, 380, 120);
                grid.Columns.Add ("Name", "Name");
                grid.Columns.Add ("Value", "Value");
                grid.Rows.Add ("Keyboard", "2");
                grid.Rows.Add ("Mouse", "1");
                Controls.Add (grid);
            }
            grid.Focus ();
            grid.CurrentCell = grid.Rows [0].Cells [0];
            grid.BeginEdit (true);
            SetResult ("GridEdit started");
        }, i++);

        AddButton ("RichText", delegate {
            if (rtb == null) {
                rtb = new RichTextBox ();
                rtb.SetBounds (230, 220, 380, 120);
                Controls.Add (rtb);
            }
            rtb.Text = "Normal Bold Red";
            rtb.Select (7, 4);
            rtb.SelectionFont = new Font (rtb.Font, FontStyle.Bold);
            rtb.Select (12, 3);
            rtb.SelectionColor = Color.Red;
            rtb.Select (0, 0);
            SetResult ("RichText styled");
        }, i++);

        AddButton ("MDI", delegate {
            Form mdi = new Form ();
            mdi.Text = "MDI Parent";
            mdi.IsMdiContainer = true;
            mdi.ClientSize = new Size (360, 220);
            mdi.StartPosition = FormStartPosition.Manual;
            mdi.Location = new Point (60, 120);
            Form child = new Form ();
            child.Text = "MDI Child";
            child.MdiParent = mdi;
            mdi.Show ();
            child.Show ();
            SetResult ("MDI shown");
            Timer t = new Timer ();
            t.Interval = 6000;
            t.Tick += delegate { t.Stop (); mdi.Close (); };
            t.Start ();
        }, i++);

        AddButton ("NotifyIcon", delegate {
            if (tray == null) {
                tray = new NotifyIcon ();
                tray.Icon = SystemIcons.Application;
                tray.Text = "Mono tray";
                tray.Visible = true;
                SetResult ("NotifyIcon visible");
            } else {
                tray.Visible = false;
                SetResult ("NotifyIcon hidden");
            }
        }, i++);

        Load += delegate {
            try {
                log = new StreamWriter ("C:\\DIAG.TXT", false, Encoding.UTF8);
                log.AutoFlush = true;
                Note ("== dialogs start ==");
            } catch { log = null; }
        };
    }

    PrintDocument MakeDocument ()
    {
        PrintDocument doc = new PrintDocument ();
        doc.PrintPage += delegate (object s, PrintPageEventArgs e) {
            e.Graphics.DrawString ("Win98 print test", new Font ("Arial", 14), Brushes.Black, 100, 100);
        };
        return doc;
    }

    [STAThread]
    static void Main ()
    {
        try { Application.EnableVisualStyles (); } catch { }
        try {
            Application.Run (new Dialogs ());
        } catch (Exception e) {
            Note ("FATAL " + e.GetType ().Name + ": " + e.Message);
            try { File.WriteAllText ("C:\\DIAG_FATAL.TXT", e.ToString ()); } catch { }
        }
    }
}
