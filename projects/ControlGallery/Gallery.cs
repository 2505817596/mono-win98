using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

class Gallery : Form
{
    static readonly List<string> report = new List<string>();
    static StreamWriter log;

    TabControl tabs;
    StatusStrip status;
    ToolStripStatusLabel statusLabel;
    Timer cycle;

    static void Note (string s)
    {
        report.Add (s);
        if (log != null) { log.WriteLine (s); log.Flush (); }
    }

    static T Safe<T> (string name, Func<T> make, Control parent, int x, int y) where T : Control
    {
        try {
            T c = make ();
            c.Location = new Point (x, y);
            parent.Controls.Add (c);
            Note ("OK   " + name);
            return c;
        } catch (Exception e) {
            Note ("FAIL " + name + " : " + e.GetType ().Name + ": " + e.Message);
            Label l = new Label ();
            l.Text = name + " FAILED";
            l.ForeColor = Color.Red;
            l.Location = new Point (x, y);
            l.AutoSize = true;
            parent.Controls.Add (l);
            return null;
        }
    }

    ImageList MakeImages ()
    {
        ImageList il = new ImageList ();
        il.ImageSize = new Size (16, 16);
        Color[] cols = { Color.Red, Color.Green, Color.Blue, Color.Orange };
        for (int i = 0; i < cols.Length; i++) {
            Bitmap b = new Bitmap (16, 16);
            using (Graphics g = Graphics.FromImage (b)) {
                g.Clear (cols[i]);
                g.DrawRectangle (Pens.Black, 0, 0, 15, 15);
            }
            il.Images.Add (b);
        }
        return il;
    }

    TabPage Page (string text)
    {
        TabPage p = new TabPage (text);
        p.Padding = new Padding (6);
        return p;
    }

    public Gallery ()
    {
        Text = "Win98 Control Gallery";
        ClientSize = new Size (780, 545);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = SystemColors.Control;

        // form-level strips
        try {
            MenuStrip menu = new MenuStrip ();
            menu.Items.Add ("File");
            ((ToolStripMenuItem) menu.Items [0]).DropDownItems.Add ("Exit");
            MainMenuStrip = menu;
            Controls.Add (menu);
            Note ("OK   MenuStrip");
        } catch (Exception e) { Note ("FAIL MenuStrip : " + e.Message); }

        try {
            ToolStrip ts = new ToolStrip ();
            ts.Items.Add (new ToolStripButton ("Button"));
            ts.Items.Add (new ToolStripLabel ("Label"));
            ts.Items.Add (new ToolStripComboBox ());
            ts.Items.Add (new ToolStripTextBox ());
            ts.Items.Add (new ToolStripSeparator ());
            ts.Items.Add (new ToolStripDropDownButton ("DropDown"));
            Controls.Add (ts);
            Note ("OK   ToolStrip (+Button,Label,ComboBox,TextBox,DropDownButton)");
        } catch (Exception e) { Note ("FAIL ToolStrip : " + e.GetType ().Name + ": " + e.Message); }

        try {
            status = new StatusStrip ();
            statusLabel = new ToolStripStatusLabel ("Gallery");
            status.Items.Add (statusLabel);
            Controls.Add (status);
            Note ("OK   StatusStrip");
        } catch (Exception e) { Note ("FAIL StatusStrip : " + e.Message); }

        tabs = new TabControl ();
        tabs.SetBounds (8, 48, 764, 470);
        Controls.Add (tabs);

        tabs.TabPages.Add (BuildBasics ());
        tabs.TabPages.Add (BuildText ());
        tabs.TabPages.Add (BuildLists ());
        tabs.TabPages.Add (BuildRanges ());
        tabs.TabPages.Add (BuildData ());
        tabs.TabPages.Add (BuildLayout ());

        // cycle tabs for screenshots
        cycle = new Timer ();
        cycle.Interval = 7000;
        cycle.Tick += delegate {
            int i = tabs.SelectedIndex + 1;
            if (i >= tabs.TabPages.Count) i = 0;
            tabs.SelectedIndex = i;
            if (statusLabel != null)
                statusLabel.Text = tabs.TabPages [i].Text;
        };
        if (Environment.GetEnvironmentVariable ("GALLERY_CYCLE") == "1")
            cycle.Start ();
    }

    TabPage BuildBasics ()
    {
        TabPage p = Page ("Basics");
        Safe ("Label", delegate { return new Label { Text = "Label", AutoSize = true }; }, p, 10, 10);
        Safe ("LinkLabel", delegate {
            LinkLabel l = new LinkLabel { Text = "LinkLabel", AutoSize = true };
            l.LinkClicked += delegate { };
            return l;
        }, p, 10, 40);
        Button b = Safe ("Button", delegate { return new Button { Text = "Button", Size = new Size (100, 28) }; }, p, 10, 70);
        Safe ("CheckBox", delegate { return new CheckBox { Text = "CheckBox", AutoSize = true }; }, p, 10, 110);
        Safe ("RadioButton", delegate { return new RadioButton { Text = "RadioButton", AutoSize = true }; }, p, 10, 140);

        GroupBox gb = Safe ("GroupBox", delegate { return new GroupBox { Text = "GroupBox", Size = new Size (220, 90) }; }, p, 250, 10);
        if (gb != null) {
            gb.Controls.Add (new RadioButton { Text = "Radio A", AutoSize = true, Location = new Point (10, 22) });
            gb.Controls.Add (new RadioButton { Text = "Radio B", AutoSize = true, Location = new Point (10, 52) });
        }

        Panel panel = Safe ("Panel", delegate { return new Panel { BorderStyle = BorderStyle.FixedSingle, Size = new Size (220, 90) }; }, p, 250, 120);
        if (panel != null) panel.Controls.Add (new Label { Text = "Label in Panel", AutoSize = true, Location = new Point (8, 8) });

        if (b != null) {
            try {
                ToolTip tip = new ToolTip ();
                tip.SetToolTip (b, "ToolTip text");
                ContextMenuStrip cm = new ContextMenuStrip ();
                cm.Items.Add ("Menu item 1");
                cm.Items.Add ("Menu item 2");
                b.ContextMenuStrip = cm;
                Note ("OK   ToolTip");
                Note ("OK   ContextMenuStrip");
            } catch (Exception e) { Note ("FAIL ToolTip/ContextMenuStrip : " + e.Message); }
        }
        return p;
    }

    TabPage BuildText ()
    {
        TabPage p = Page ("Text");
        Safe ("TextBox", delegate { return new TextBox { Text = "TextBox", Width = 200 }; }, p, 10, 10);
        Safe ("TextBox (multiline)", delegate { return new TextBox { Multiline = true, Height = 60, Width = 200, Text = "line1\r\nline2" }; }, p, 10, 45);
        Safe ("MaskedTextBox", delegate { return new MaskedTextBox { Mask = "000-0000", Width = 120 }; }, p, 230, 10);
        Safe ("RichTextBox", delegate { return new RichTextBox { Width = 200, Height = 80, Text = "RichTextBox" }; }, p, 10, 115);
        Safe ("NumericUpDown", delegate { return new NumericUpDown { Value = 25, Width = 80 }; }, p, 230, 45);
        Safe ("DomainUpDown", delegate {
            DomainUpDown d = new DomainUpDown { Width = 100 };
            d.Items.Add ("Red"); d.Items.Add ("Green"); d.Items.Add ("Blue");
            d.SelectedIndex = 0;
            return d;
        }, p, 230, 80);
        Safe ("ComboBox (DropDown)", delegate {
            ComboBox c = new ComboBox { Width = 140 };
            c.Items.Add ("One"); c.Items.Add ("Two"); c.Items.Add ("Three");
            c.SelectedIndex = 0;
            return c;
        }, p, 230, 115);
        Safe ("ComboBox (DropDownList)", delegate {
            ComboBox c = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            c.Items.Add ("A"); c.Items.Add ("B");
            c.SelectedIndex = 1;
            return c;
        }, p, 230, 150);
        return p;
    }

    TabPage BuildLists ()
    {
        TabPage p = Page ("Lists");
        Safe ("ListBox", delegate {
            ListBox l = new ListBox { Size = new Size (160, 120) };
            for (int i = 0; i < 8; i++) l.Items.Add ("Item " + (i + 1));
            l.SelectedIndex = 1;
            return l;
        }, p, 10, 10);
        Safe ("CheckedListBox", delegate {
            CheckedListBox l = new CheckedListBox { Size = new Size (160, 120) };
            for (int i = 0; i < 6; i++) l.Items.Add ("Check " + (i + 1), i % 2 == 0);
            return l;
        }, p, 10, 140);
        Safe ("ComboBox", delegate {
            ComboBox c = new ComboBox { Width = 160 };
            c.Items.Add ("Combo 1"); c.Items.Add ("Combo 2");
            c.SelectedIndex = 0;
            return c;
        }, p, 10, 270);

        ImageList il = null;
        try { il = MakeImages (); Note ("OK   ImageList"); }
        catch (Exception e) { Note ("FAIL ImageList : " + e.Message); }

        Safe ("ListView", delegate {
            ListView l = new ListView { Size = new Size (250, 140), View = View.Details, FullRowSelect = true };
            l.Columns.Add ("Name", 110); l.Columns.Add ("Value", 80);
            if (il != null) { l.SmallImageList = il; }
            for (int i = 0; i < 4; i++) {
                ListViewItem it = new ListViewItem ("Row " + (i + 1), il != null ? i % 4 : -1);
                it.SubItems.Add ((i * 10).ToString ());
                l.Items.Add (it);
            }
            return l;
        }, p, 190, 10);

        Safe ("TreeView", delegate {
            TreeView t = new TreeView { Size = new Size (160, 140), CheckBoxes = true };
            if (il != null) t.ImageList = il;
            TreeNode root = t.Nodes.Add ("Root");
            root.Nodes.Add ("Child 1");
            root.Nodes.Add ("Child 2");
            root.Checked = true;
            root.ExpandAll ();
            return t;
        }, p, 190, 160);

        Safe ("Label + Image", delegate {
            PictureBox pb = new PictureBox { Size = new Size (48, 48), BorderStyle = BorderStyle.FixedSingle };
            if (il != null) pb.Image = il.Images [2];
            return pb;
        }, p, 370, 160);
        return p;
    }

    TabPage BuildRanges ()
    {
        TabPage p = Page ("Ranges");
        Safe ("ProgressBar", delegate { return new ProgressBar { Value = 40, Width = 220 }; }, p, 10, 10);
        Safe ("TrackBar", delegate { return new TrackBar { Width = 220, Maximum = 100, Value = 30 }; }, p, 10, 45);
        Safe ("HScrollBar", delegate { return new HScrollBar { Width = 220, Value = 20 }; }, p, 10, 90);
        Safe ("VScrollBar", delegate { return new VScrollBar { Height = 100, Value = 20 }; }, p, 260, 10);
        Safe ("DateTimePicker", delegate { return new DateTimePicker { Width = 160 }; }, p, 10, 120);
        Safe ("MonthCalendar", delegate { return new MonthCalendar (); }, p, 10, 160);
        Safe ("PictureBox (GDI+)", delegate {
            Bitmap bm = new Bitmap (140, 70);
            using (Graphics g = Graphics.FromImage (bm)) {
                for (int y = 0; y < bm.Height; y++) {
                    int c = (y * 255) / bm.Height;
                    using (Pen pen = new Pen (Color.FromArgb (255 - c, 80, c)))
                        g.DrawLine (pen, 0, y, bm.Width, y);
                }
                g.DrawString ("GDI+", new Font ("Arial", 12, FontStyle.Bold), Brushes.White, 4, 4);
            }
            return new PictureBox { Image = bm, Size = new Size (140, 70), BorderStyle = BorderStyle.FixedSingle };
        }, p, 300, 60);
        return p;
    }

    class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    TabPage BuildData ()
    {
        TabPage p = Page ("Data");
        Safe ("DataGridView", delegate {
            DataGridView g = new DataGridView { Width = 480, Height = 220 };
            g.Columns.Add ("Name", "Name");
            g.Columns.Add ("Value", "Value");
            g.Rows.Add ("Keyboard", "2");
            g.Rows.Add ("Mouse", "1");
            g.Rows.Add ("Monitor", "3");
            return g;
        }, p, 10, 10);
        Safe ("PropertyGrid", delegate {
            PropertyGrid pg = new PropertyGrid { Width = 240, Height = 220 };
            pg.SelectedObject = new Person { Name = "Mono", Age = 6 };
            return pg;
        }, p, 500, 10);
        return p;
    }

    TabPage BuildLayout ()
    {
        TabPage p = Page ("Layout");
        Safe ("SplitContainer", delegate {
            SplitContainer sc = new SplitContainer { Size = new Size (300, 200), Orientation = Orientation.Vertical };
            sc.Panel1.Controls.Add (new Label { Text = "Panel1", AutoSize = true, Location = new Point (8, 8) });
            sc.Panel2.Controls.Add (new Label { Text = "Panel2", AutoSize = true, Location = new Point (8, 8) });
            return sc;
        }, p, 10, 10);

        Safe ("FlowLayoutPanel", delegate {
            FlowLayoutPanel f = new FlowLayoutPanel { Size = new Size (300, 100), BorderStyle = BorderStyle.FixedSingle };
            for (int i = 0; i < 4; i++) f.Controls.Add (new Button { Text = "B" + (i + 1), Size = new Size (60, 26) });
            return f;
        }, p, 330, 10);

        Safe ("TableLayoutPanel", delegate {
            TableLayoutPanel t = new TableLayoutPanel { Size = new Size (200, 100), ColumnCount = 2, RowCount = 2 };
            t.Controls.Add (new Label { Text = "0,0", AutoSize = true }, 0, 0);
            t.Controls.Add (new TextBox { Text = "0,1", Width = 80 }, 0, 1);
            t.Controls.Add (new CheckBox { Text = "1,0", AutoSize = true }, 1, 0);
            t.Controls.Add (new Button { Text = "1,1", Size = new Size (70, 24) }, 1, 1);
            return t;
        }, p, 330, 130);
        return p;
    }

    [STAThread]
    static void Main ()
    {
        try {
            log = new StreamWriter ("C:\\GALLERY.TXT", false, Encoding.UTF8);
            log.AutoFlush = true;
        } catch { log = null; }
        try {
            Application.EnableVisualStyles ();
        } catch { }
        Note ("== start ==");
        try {
            Application.Run (new Gallery ());
        } catch (Exception e) {
            Note ("FATAL " + e.GetType ().Name + ": " + e.Message);
            try { File.WriteAllText ("C:\\GALLERY_FATAL.TXT", e.ToString ()); } catch { }
        }
    }
}
