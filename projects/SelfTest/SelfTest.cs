using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

class SelfTest
{
    static TextBox log;
    static int pass, fail;
    static List<string> failures = new List<string> ();
    static int uiThread;

    [DllImport ("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SendMessageW")]
    static extern IntPtr SendMessage (IntPtr h, int msg, IntPtr w, IntPtr l);

    static void Log (string s)
    {
        if (log != null)
            log.AppendText (s + "\r\n");
    }

    static void Pass (string n)
    {
        pass++;
        Log ("PASS " + n);
    }

    static void Fail (string n, Exception e)
    {
        fail++;
        string m = n + " : " + e.GetType ().Name + ": " + e.Message;
        failures.Add (m);
        Log ("FAIL " + m);
    }

    static void Check (string n, Action a)
    {
        try { a (); Pass (n); }
        catch (Exception e) { Fail (n, e); }
    }

    static async Task CheckAsync (string n, Func<Task> f, int timeoutMs)
    {
        try {
            Task t = f ();
            Task done = await Task.WhenAny (t, Task.Delay (timeoutMs));
            if (done != t)
                throw new TimeoutException ("timeout " + timeoutMs + "ms");
            await t;
            Pass (n);
        } catch (Exception e) {
            Fail (n, e);
        }
    }

    [STAThread]
    static void Main ()
    {
        Form f = new Form ();
        f.Text = "Win98 Self Test";
        f.ClientSize = new Size (720, 500);
        log = new TextBox ();
        log.Multiline = true;
        log.ScrollBars = ScrollBars.Vertical;
        log.Dock = DockStyle.Fill;
        log.Font = new Font (FontFamily.GenericMonospace, 9);
        f.Controls.Add (log);
        f.Shown += OnShown;
        Application.Run (f);
    }

    static async void OnShown (object sender, EventArgs e)
    {
        Form f = (Form) sender;
        uiThread = Thread.CurrentThread.ManagedThreadId;
        Log ("Win98 self test, ui thread " + uiThread);
        await RunAll (f);
        string summary = "SUMMARY pass=" + pass + " fail=" + fail;
        Log (summary);
        try { File.WriteAllText ("C:\\SELFTEST.TXT", log.Text + "\r\n" + summary); } catch { }
        f.Close ();
    }

    static async Task RunAll (Form f)
    {
        // ---------- controls ----------
        Check ("controls/label-button", delegate {
            Label l = new Label { Text = "hello" };
            Button b = new Button { Text = "ok" };
            if (l.Text != "hello" || b.Text != "ok") throw new Exception ("text");
        });

        Check ("controls/textbox", delegate {
            TextBox t = new TextBox { Text = "World" };
            t.AppendText ("-x");
            if (t.Text != "World-x") throw new Exception ("text=" + t.Text);
        });

        Check ("controls/combobox", delegate {
            ComboBox c = new ComboBox ();
            c.Items.Add ("One");
            c.Items.Add ("Two");
            c.SelectedIndex = 1;
            if (c.SelectedItem.ToString () != "Two") throw new Exception ("sel");
        });

        Check ("controls/listbox-checkedlistbox", delegate {
            ListBox b = new ListBox ();
            b.Items.Add ("a"); b.Items.Add ("b");
            CheckedListBox c = new CheckedListBox ();
            c.Items.Add ("x", true);
            if (b.Items.Count != 2 || !c.GetItemChecked (0)) throw new Exception ("items");
        });

        Check ("controls/listview", delegate {
            ListView v = new ListView { View = View.Details };
            v.Columns.Add ("A", 60); v.Columns.Add ("B", 60);
            v.Items.Add (new ListViewItem (new string [] { "r1", "1" }));
            if (v.Items.Count != 1 || v.Columns.Count != 2) throw new Exception ("lv");
        });

        Check ("controls/treeview", delegate {
            TreeView t = new TreeView ();
            TreeNode r = t.Nodes.Add ("root");
            r.Nodes.Add ("child");
            r.Checked = true;
            if (t.Nodes.Count != 1 || !r.Checked) throw new Exception ("tv");
        });

        Check ("controls/datagridview", delegate {
            DataGridView g = new DataGridView ();
            g.Columns.Add ("N", "N");
            g.Columns.Add ("V", "V");
            g.Rows.Add ("Keyboard", "2");
            if (g.Rows.Count != 2) throw new Exception ("rows=" + g.Rows.Count);
            if (g.Rows [0].Cells [0].Value.ToString () != "Keyboard") throw new Exception ("cell");
        });

        Check ("controls/propertygrid", delegate {
            PropertyGrid p = new PropertyGrid ();
            p.SelectedObject = new object ();
            if (p.SelectedObject == null) throw new Exception ("sel");
        });

        Check ("controls/calendar-datetime", delegate {
            MonthCalendar m = new MonthCalendar ();
            DateTimePicker d = new DateTimePicker ();
            m.SetDate (DateTime.Today);
            d.Value = DateTime.Today;
            if (d.Value.Date != DateTime.Today) throw new Exception ("date");
        });

        Check ("controls/numeric-domain-updown", delegate {
            NumericUpDown n = new NumericUpDown { Maximum = 100, Value = 42 };
            DomainUpDown d = new DomainUpDown ();
            d.Items.Add ("Red"); d.SelectedIndex = 0;
            if (n.Value != 42 || d.SelectedItem.ToString () != "Red") throw new Exception ("updown");
        });

        Check ("controls/progressbar-trackbar", delegate {
            ProgressBar p = new ProgressBar { Maximum = 100, Value = 30 };
            TrackBar t = new TrackBar { Maximum = 100, Value = 30 };
            if (p.Value != 30 || t.Value != 30) throw new Exception ("range");
        });

        Check ("controls/richtextbox", delegate {
            RichTextBox r = new RichTextBox ();
            r.Text = "abc";
            r.Select (0, 1);
            r.SelectionFont = new Font (r.Font, FontStyle.Bold);
            if (!r.Rtf.Contains ("\\b")) throw new Exception ("rtf");
        });

        Check ("controls/layout", delegate {
            SplitContainer s = new SplitContainer ();
            FlowLayoutPanel fl = new FlowLayoutPanel ();
            TableLayoutPanel tp = new TableLayoutPanel { ColumnCount = 2, RowCount = 2 };
            fl.Controls.Add (new Button ());
            tp.Controls.Add (new Label { Text = "0,0" }, 0, 0);
            if (s.Panel1 == null || fl.Controls.Count != 1) throw new Exception ("layout");
        });

        Check ("controls/tooltip-contextmenu", delegate {
            ToolTip tip = new ToolTip ();
            tip.SetToolTip (new Button (), "tip");
            ContextMenuStrip cm = new ContextMenuStrip ();
            cm.Items.Add ("item");
            if (cm.Items.Count != 1) throw new Exception ("cm");
        });

        // ---------- drawing ----------
        Check ("draw/gdiplus", delegate {
            using (Bitmap b = new Bitmap (16, 16)) {
                using (Graphics g = Graphics.FromImage (b)) {
                    g.Clear (Color.Red);
                    g.FillRectangle (Brushes.Blue, 8, 8, 8, 8);
                    g.DrawString ("g", new Font ("Arial", 8), Brushes.Black, 0, 0);
                }
                Color c1 = b.GetPixel (2, 2);
                Color c2 = b.GetPixel (12, 12);
                if (c1.R < 200 || c1.B > 60) throw new Exception ("red=" + c1);
                if (c2.B < 200 || c2.R > 60) throw new Exception ("blue=" + c2);
            }
        });

        // ---------- clipboard ----------
        Check ("clipboard/ascii", delegate {
            Clipboard.SetDataObject ("Hello98", true);
            string s = Clipboard.GetText ();
            if (s != "Hello98") throw new Exception ("got=" + s);
        });

        Check ("clipboard/chinese", delegate {
            string t = "\u4e2d\u6587\u7c98\u8d34\u6d4b\u8bd5";
            Clipboard.SetDataObject (t, true);
            string s = Clipboard.GetText ();
            if (s != t) throw new Exception ("got=" + s);
        });

        // ---------- IME DBCS (simulated WM_CHAR bytes) ----------
        Check ("ime/dbcs-wmchar", delegate {
            TextBox tb = new TextBox ();
            f.Controls.Add (tb);
            tb.CreateControl ();
            tb.Focus ();
            SendMessage (tb.Handle, 0x0102, (IntPtr) 0xC4, (IntPtr) 1); // 你 (GBK)
            SendMessage (tb.Handle, 0x0102, (IntPtr) 0xE3, (IntPtr) 1);
            if (tb.Text != "\u4f60") throw new Exception ("text=" + tb.Text);
            f.Controls.Remove (tb);
        });

        // ---------- async ----------
        await CheckAsync ("async/delay", async delegate {
            DateTime t0 = DateTime.Now;
            await Task.Delay (300);
            double ms = (DateTime.Now - t0).TotalMilliseconds;
            if (ms < 200) throw new Exception ("only " + ms + "ms");
        }, 5000);

        await CheckAsync ("async/ui-context", async delegate {
            await Task.Delay (100);
            if (Thread.CurrentThread.ManagedThreadId != uiThread)
                throw new Exception ("thread=" + Thread.CurrentThread.ManagedThreadId);
            if (f.InvokeRequired)
                throw new Exception ("invoke required");
        }, 5000);

        await CheckAsync ("async/task-run", async delegate {
            int v = await Task.Run (new Func<int> (delegate {
                int s = 0;
                for (int i = 1; i <= 100; i++) s += i;
                return s;
            }));
            if (v != 5050) throw new Exception ("v=" + v);
        }, 5000);

        await CheckAsync ("async/whenall", async delegate {
            Task<int> a = Task.Run (new Func<int> (delegate { Thread.Sleep (100); return 1; }));
            Task<int> b = Task.Run (new Func<int> (delegate { Thread.Sleep (200); return 2; }));
            int [] r = await Task.WhenAll (a, b);
            if (r [0] != 1 || r [1] != 2) throw new Exception ("results");
        }, 5000);

        await CheckAsync ("async/tcs", async delegate {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int> ();
            Thread th = new Thread (new ThreadStart (delegate { Thread.Sleep (150); tcs.SetResult (42); }));
            th.IsBackground = true;
            th.Start ();
            int v = await tcs.Task;
            if (v != 42) throw new Exception ("v=" + v);
        }, 5000);

        // ---------- threading ----------
        await CheckAsync ("thread/invoke", async delegate {
            int v = 0;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool> ();
            Thread th = new Thread (new ThreadStart (delegate {
                f.Invoke ((MethodInvoker) delegate { v = 7; tcs.SetResult (true); });
            }));
            th.IsBackground = true;
            th.Start ();
            await tcs.Task;
            if (v != 7) throw new Exception ("v=" + v);
        }, 5000);

        await CheckAsync ("thread/begininvoke", async delegate {
            int v = 0;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool> ();
            Thread th = new Thread (new ThreadStart (delegate {
                f.BeginInvoke ((MethodInvoker) delegate { v = 9; tcs.SetResult (true); });
            }));
            th.IsBackground = true;
            th.Start ();
            await tcs.Task;
            if (v != 9) throw new Exception ("v=" + v);
        }, 5000);

        await CheckAsync ("thread/event", async delegate {
            ManualResetEvent ev = new ManualResetEvent (false);
            int v = 0;
            Thread th = new Thread (new ThreadStart (delegate { v = 42; ev.Set (); }));
            th.IsBackground = true;
            th.Start ();
            if (!ev.WaitOne (3000)) throw new TimeoutException ("event");
            if (v != 42) throw new Exception ("v=" + v);
        }, 5000);

        await CheckAsync ("thread/system-timer", async delegate {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int> ();
            int n = 0;
            System.Threading.Timer t = null;
            t = new System.Threading.Timer (delegate {
                if (Interlocked.Increment (ref n) >= 3)
                    tcs.TrySetResult (n);
            }, null, 100, 100);
            int r = await tcs.Task;
            t.Dispose ();
            if (r < 3) throw new Exception ("n=" + r);
        }, 5000);

        // ---------- UI mechanics ----------
        await CheckAsync ("ui/winforms-timer", async delegate {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool> ();
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer ();
            t.Interval = 100;
            t.Tick += delegate { t.Stop (); tcs.TrySetResult (true); };
            t.Start ();
            await tcs.Task;
        }, 5000);

        Check ("ui/modal-dialog", delegate {
            DateTime t0 = DateTime.Now;
            Form d = new Form ();
            d.Text = "modal test";
            d.ClientSize = new Size (200, 100);
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer ();
            t.Interval = 250;
            t.Tick += delegate { t.Stop (); d.Close (); };
            t.Start ();
            d.ShowDialog (f);
            double ms = (DateTime.Now - t0).TotalMilliseconds;
            if (ms < 200) throw new Exception ("returned after " + ms + "ms");
        });

        Check ("ui/notifyicon", delegate {
            NotifyIcon n = new NotifyIcon ();
            n.Icon = SystemIcons.Application;
            n.Text = "self test";
            n.Visible = true;
            n.Visible = false;
            n.Dispose ();
        });

        Check ("ui/mdi", delegate {
            Form p = new Form ();
            p.IsMdiContainer = true;
            Form c = new Form ();
            c.MdiParent = p;
            p.Show ();
            c.Show ();
            if (p.MdiChildren.Length != 1) throw new Exception ("children=" + p.MdiChildren.Length);
            p.Close ();
        });

        Check ("ui/control-invoke-required", delegate {
            if (f.InvokeRequired) throw new Exception ("main thread marked invokerequired");
        });
    }
}
