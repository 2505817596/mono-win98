using System;
using System.Drawing;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Win98ControlDemo
{
    public class MainForm : Form
    {
        TabControl tabs;
        StatusStrip status;
        ToolStripStatusLabel statusLabel;
        TextBox nameBox;
        TextBox logBox;
        ListBox listBox;
        CheckedListBox checkedList;
        ListView listView;
        DataGridView grid;
        RichTextBox rich;
        ProgressBar progress;
        TrackBar track;
        ContextMenuStrip ctx;
        ToolTip tip = new ToolTip ();
        NotifyIcon tray;
        TextBox threadLog;
        ProgressBar threadProgress;
        Thread worker;

        public MainForm ()
        {
            Text = "Win98 Control Demo - " + Environment.OSVersion.VersionString;
            ClientSize = new Size (780, 520);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = SystemColors.Control;

            BuildStrips ();

            tabs = new TabControl ();
            tabs.SetBounds (8, 50, 764, 440);
            Controls.Add (tabs);

            tabs.TabPages.Add (BuildBasics ());
            tabs.TabPages.Add (BuildText ());
            tabs.TabPages.Add (BuildLists ());
            tabs.TabPages.Add (BuildRanges ());
            tabs.TabPages.Add (BuildData ());
            tabs.TabPages.Add (BuildLayout ());
            tabs.TabPages.Add (BuildDialogs ());
            tabs.TabPages.Add (BuildThreads ());
            tabs.TabPages.Add (BuildMore ());

            System.Windows.Forms.Timer clock = new System.Windows.Forms.Timer ();
            clock.Interval = 1000;
            clock.Tick += delegate { SetStatus ("Ready - " + DateTime.Now.ToString ("HH:mm:ss")); };
            clock.Start ();
        }

        void SetStatus (string s)
        {
            if (statusLabel != null) statusLabel.Text = s;
        }

        void BuildStrips ()
        {
            MenuStrip menu = new MenuStrip ();
            ToolStripMenuItem file = new ToolStripMenuItem ("File");
            file.DropDownItems.Add ("Exit", null, delegate { Close (); });
            ToolStripMenuItem demo = new ToolStripMenuItem ("Demo");
            demo.DropDownItems.Add ("About", null, delegate {
                MessageBox.Show (this,
                    "Mono " + Environment.Version + " WinForms on " + Environment.OSVersion.VersionString,
                    "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
            menu.Items.Add (file);
            menu.Items.Add (demo);
            MainMenuStrip = menu;
            Controls.Add (menu);

            ToolStrip ts = new ToolStrip ();
            ts.Items.Add (new ToolStripButton ("Say Hello", null, delegate { SayHello (); }));
            ts.Items.Add (new ToolStripButton ("Clear", null, delegate { ClearAll (); }));
            ts.Items.Add (new ToolStripSeparator ());
            ts.Items.Add (new ToolStripButton ("Next Tab", null, delegate { NextTab (); }));
            ts.Items.Add (new ToolStripSeparator ());
            ts.Items.Add (new ToolStripButton ("Open...", null, delegate { OpenFile (); }));
            ts.Items.Add (new ToolStripButton ("Save...", null, delegate { SaveFile (); }));
            ts.Items.Add (new ToolStripButton ("Folder...", null, delegate { BrowseFolder (); }));
            Controls.Add (ts);

            status = new StatusStrip ();
            statusLabel = new ToolStripStatusLabel ("Ready");
            status.Items.Add (statusLabel);
            Controls.Add (status);
        }

        void NextTab ()
        {
            int i = tabs.SelectedIndex + 1;
            if (i >= tabs.TabPages.Count) i = 0;
            tabs.SelectedIndex = i;
            SetStatus (tabs.TabPages [i].Text);
        }

        void SayHello ()
        {
            string name = nameBox != null && nameBox.Text.Length > 0 ? nameBox.Text : "World";
            SetStatus ("Hello, " + name + "!");
            MessageBox.Show (this, "Hello, " + name + "!", "Greeting",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void ClearAll ()
        {
            if (listBox != null) listBox.Items.Clear ();
            if (checkedList != null) checkedList.Items.Clear ();
            if (listView != null) listView.Items.Clear ();
            if (grid != null) grid.Rows.Clear ();
            if (logBox != null) logBox.Text = "";
            SetStatus ("Cleared");
        }

        TabPage Page (string text)
        {
            TabPage p = new TabPage (text);
            p.Padding = new Padding (6);
            return p;
        }

        TabPage BuildBasics ()
        {
            TabPage p = Page ("Basics");
            p.Controls.Add (new Label { Text = "Label", AutoSize = true, Location = new Point (10, 10) });
            LinkLabel link = new LinkLabel { Text = "LinkLabel", AutoSize = true, Location = new Point (10, 40) };
            link.LinkClicked += delegate { SetStatus ("LinkLabel clicked"); };
            p.Controls.Add (link);

            Button b = new Button { Text = "Button", Size = new Size (100, 28), Location = new Point (10, 70) };
            b.Click += delegate { SetStatus ("Button clicked"); };
            p.Controls.Add (b);
            tip.SetToolTip (b, "ToolTip for Button");

            ctx = new ContextMenuStrip ();
            ctx.Items.Add ("Item 1");
            ctx.Items.Add ("Item 2");
            ctx.Items.Add ("Item 3");
            b.ContextMenuStrip = ctx;

            p.Controls.Add (new CheckBox { Text = "CheckBox", AutoSize = true, Location = new Point (10, 110) });
            p.Controls.Add (new RadioButton { Text = "RadioButton", AutoSize = true, Location = new Point (10, 140) });

            GroupBox gb = new GroupBox { Text = "GroupBox", Size = new Size (220, 90), Location = new Point (250, 10) };
            gb.Controls.Add (new RadioButton { Text = "Radio A", AutoSize = true, Location = new Point (10, 22) });
            gb.Controls.Add (new RadioButton { Text = "Radio B", AutoSize = true, Location = new Point (10, 52) });
            p.Controls.Add (gb);

            Panel panel = new Panel { BorderStyle = BorderStyle.FixedSingle, Size = new Size (220, 90), Location = new Point (250, 120) };
            panel.Controls.Add (new Label { Text = "Label in Panel", AutoSize = true, Location = new Point (8, 8) });
            p.Controls.Add (panel);

            Button mb = new Button { Text = "MessageBox", Size = new Size (120, 28), Location = new Point (500, 10) };
            mb.Click += delegate {
                MessageBox.Show (this, "Modal dialog works", "Demo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            p.Controls.Add (mb);

            Button cm = new Button { Text = "ContextMenu", Size = new Size (120, 28), Location = new Point (500, 50) };
            cm.Click += delegate { ctx.Show (cm, new Point (0, cm.Height)); };
            p.Controls.Add (cm);

            Button tt = new Button { Text = "ToolTip", Size = new Size (120, 28), Location = new Point (500, 90) };
            tt.Click += delegate { tip.Show ("ToolTip text visible", tt, new Point (0, tt.Height), 4000); };
            p.Controls.Add (tt);
            return p;
        }

        TabPage BuildText ()
        {
            TabPage p = Page ("Text");
            nameBox = new TextBox { Text = "World", Width = 200, Location = new Point (10, 10) };
            p.Controls.Add (nameBox);
            p.Controls.Add (new Label { Text = "Name:", AutoSize = true, Location = new Point (10, 40) });

            logBox = new TextBox {
                Multiline = true, Height = 60, Width = 200,
                Location = new Point (10, 70), ScrollBars = ScrollBars.Vertical,
                Text = "line 1\r\nline 2"
            };
            p.Controls.Add (logBox);

            p.Controls.Add (new MaskedTextBox { Mask = "000-0000", Width = 120, Location = new Point (240, 10) });
            p.Controls.Add (new NumericUpDown { Value = 25, Width = 80, Location = new Point (240, 45) });

            DomainUpDown dud = new DomainUpDown { Width = 100, Location = new Point (240, 80) };
            dud.Items.Add ("Red");
            dud.Items.Add ("Green");
            dud.Items.Add ("Blue");
            dud.SelectedIndex = 0;
            p.Controls.Add (dud);

            ComboBox cb1 = new ComboBox { Width = 140, Location = new Point (240, 115) };
            cb1.Items.Add ("One");
            cb1.Items.Add ("Two");
            cb1.Items.Add ("Three");
            cb1.SelectedIndex = 0;
            p.Controls.Add (cb1);

            ComboBox cb2 = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point (240, 150) };
            cb2.Items.Add ("A");
            cb2.Items.Add ("B");
            cb2.SelectedIndex = 1;
            p.Controls.Add (cb2);

            rich = new RichTextBox { Width = 420, Height = 140, Location = new Point (10, 145) };
            rich.Text = "Normal Bold Red";
            rich.Select (7, 4);
            rich.SelectionFont = new Font (rich.Font, FontStyle.Bold);
            rich.Select (12, 3);
            rich.SelectionColor = Color.Red;
            rich.Select (0, 0);
            p.Controls.Add (rich);

            Button style = new Button { Text = "Restyle RichText", Size = new Size (140, 28), Location = new Point (10, 295) };
            style.Click += delegate {
                rich.Text = "Normal Bold Red";
                rich.Select (7, 4);
                rich.SelectionFont = new Font (rich.Font, FontStyle.Bold);
                rich.Select (12, 3);
                rich.SelectionColor = Color.Red;
                rich.Select (0, 0);
                SetStatus ("RichTextBox styled");
            };
            p.Controls.Add (style);
            return p;
        }

        TabPage BuildLists ()
        {
            TabPage p = Page ("Lists");
            listBox = new ListBox { Size = new Size (160, 120), Location = new Point (10, 10) };
            for (int i = 1; i <= 8; i++) listBox.Items.Add ("Item " + i);
            listBox.SelectedIndex = 1;
            p.Controls.Add (listBox);

            checkedList = new CheckedListBox { Size = new Size (160, 120), Location = new Point (10, 140) };
            for (int i = 1; i <= 6; i++) checkedList.Items.Add ("Check " + i, i % 2 == 0);
            p.Controls.Add (checkedList);

            ImageList il = new ImageList ();
            il.ImageSize = new Size (16, 16);
            Color[] cols = { Color.Red, Color.Green, Color.Blue, Color.Orange };
            for (int i = 0; i < cols.Length; i++) {
                Bitmap bmp = new Bitmap (16, 16);
                using (Graphics g = Graphics.FromImage (bmp)) {
                    g.Clear (cols [i]);
                    g.DrawRectangle (Pens.Black, 0, 0, 15, 15);
                }
                il.Images.Add (bmp);
            }

            listView = new ListView { Size = new Size (260, 140), View = View.Details, FullRowSelect = true, Location = new Point (190, 10) };
            listView.Columns.Add ("Name", 120);
            listView.Columns.Add ("Value", 90);
            listView.SmallImageList = il;
            for (int i = 0; i < 4; i++) {
                ListViewItem it = new ListViewItem ("Row " + (i + 1), i % 4);
                it.SubItems.Add ((i * 10).ToString ());
                listView.Items.Add (it);
            }
            p.Controls.Add (listView);

            TreeView tv = new TreeView { Size = new Size (180, 140), CheckBoxes = true, ImageList = il, Location = new Point (190, 160) };
            TreeNode root = new TreeNode ("Root", 2, 2);
            tv.Nodes.Add (root);
            root.Nodes.Add ("Child 1");
            root.Nodes.Add ("Child 2");
            root.Checked = true;
            root.ExpandAll ();
            p.Controls.Add (tv);

            PictureBox pb = new PictureBox { Size = new Size (48, 48), BorderStyle = BorderStyle.FixedSingle, Location = new Point (390, 160), Image = il.Images [1] };
            p.Controls.Add (pb);
            return p;
        }

        TabPage BuildRanges ()
        {
            TabPage p = Page ("Ranges");
            progress = new ProgressBar { Value = 40, Width = 220, Location = new Point (10, 10) };
            p.Controls.Add (progress);
            track = new TrackBar { Width = 220, Maximum = 100, Value = 40, Location = new Point (10, 45) };
            track.Scroll += delegate {
                progress.Value = track.Value;
                SetStatus ("TrackBar: " + track.Value);
            };
            p.Controls.Add (track);

            p.Controls.Add (new HScrollBar { Width = 220, Value = 20, Location = new Point (10, 95) });
            p.Controls.Add (new VScrollBar { Height = 100, Value = 20, Location = new Point (250, 10) });
            p.Controls.Add (new DateTimePicker { Width = 160, Location = new Point (10, 125) });
            p.Controls.Add (new MonthCalendar { Location = new Point (10, 165) });

            Bitmap bmp = new Bitmap (140, 70);
            using (Graphics g = Graphics.FromImage (bmp)) {
                for (int y = 0; y < bmp.Height; y++) {
                    int c = (y * 255) / bmp.Height;
                    using (Pen pen = new Pen (Color.FromArgb (255 - c, 80, c)))
                        g.DrawLine (pen, 0, y, bmp.Width, y);
                }
                g.DrawString ("GDI+", new Font ("Arial", 12, FontStyle.Bold), Brushes.White, 4, 4);
            }
            p.Controls.Add (new PictureBox { Image = bmp, Size = new Size (140, 70), BorderStyle = BorderStyle.FixedSingle, Location = new Point (300, 60) });
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
            grid = new DataGridView { Width = 460, Height = 260, Location = new Point (10, 10) };
            grid.Columns.Add ("Name", "Name");
            grid.Columns.Add ("Value", "Value");
            grid.Rows.Add ("Keyboard", "2");
            grid.Rows.Add ("Mouse", "1");
            grid.Rows.Add ("Monitor", "3");
            p.Controls.Add (grid);

            PropertyGrid pg = new PropertyGrid { Width = 270, Height = 260, Location = new Point (480, 10) };
            pg.SelectedObject = new Person { Name = "Mono", Age = 6 };
            p.Controls.Add (pg);

            Button edit = new Button { Text = "Edit Cell", Size = new Size (110, 28), Location = new Point (10, 285) };
            edit.Click += delegate {
                grid.Focus ();
                grid.CurrentCell = grid.Rows [0].Cells [0];
                grid.BeginEdit (true);
                SetStatus ("Editing cell - type something");
            };
            p.Controls.Add (edit);
            return p;
        }

        TabPage BuildLayout ()
        {
            TabPage p = Page ("Layout");
            SplitContainer sc = new SplitContainer { Size = new Size (300, 200), Location = new Point (10, 10) };
            sc.Panel1.Controls.Add (new Label { Text = "Panel1", AutoSize = true, Location = new Point (8, 8) });
            sc.Panel2.Controls.Add (new Label { Text = "Panel2", AutoSize = true, Location = new Point (8, 8) });
            p.Controls.Add (sc);

            FlowLayoutPanel flow = new FlowLayoutPanel { Size = new Size (300, 100), BorderStyle = BorderStyle.FixedSingle, Location = new Point (330, 10) };
            for (int i = 1; i <= 4; i++) flow.Controls.Add (new Button { Text = "B" + i, Size = new Size (60, 26) });
            p.Controls.Add (flow);

            TableLayoutPanel table = new TableLayoutPanel { Size = new Size (200, 100), ColumnCount = 2, RowCount = 2, Location = new Point (330, 130) };
            table.Controls.Add (new Label { Text = "0,0", AutoSize = true }, 0, 0);
            table.Controls.Add (new TextBox { Text = "0,1", Width = 80 }, 0, 1);
            table.Controls.Add (new CheckBox { Text = "1,0", AutoSize = true }, 1, 0);
            table.Controls.Add (new Button { Text = "1,1", Size = new Size (70, 24) }, 1, 1);
            p.Controls.Add (table);
            return p;
        }

        Label dialogResult;

        TabPage BuildDialogs ()
        {
            TabPage p = Page ("Dialogs");
            Button open = new Button { Text = "OpenFileDialog", Size = new Size (180, 28), Location = new Point (10, 10) };
            open.Click += delegate { OpenFile (); };
            p.Controls.Add (open);

            Button save = new Button { Text = "SaveFileDialog", Size = new Size (180, 28), Location = new Point (10, 44) };
            save.Click += delegate { SaveFile (); };
            p.Controls.Add (save);

            Button folder = new Button { Text = "FolderBrowserDialog", Size = new Size (180, 28), Location = new Point (10, 78) };
            folder.Click += delegate { BrowseFolder (); };
            p.Controls.Add (folder);

            dialogResult = new Label { Location = new Point (10, 120), Size = new Size (700, 60), Text = "Result:" };
            p.Controls.Add (dialogResult);

            Label note = new Label {
                Location = new Point (10, 200), Size = new Size (700, 80),
                Text = "Printing dialogs (PrintDialog / PrintPreviewDialog / PageSetupDialog)\n" +
                       "are not available: Win9x has no GetDefaultPrinter."
            };
            p.Controls.Add (note);
            return p;
        }

        void OpenFile ()
        {
            using (OpenFileDialog d = new OpenFileDialog ()) {
                d.Filter = "Text files|*.txt|All files|*.*";
                DialogResult r = d.ShowDialog (this);
                ShowResult ("OpenFileDialog", r + " " + d.FileName);
            }
        }

        void SaveFile ()
        {
            using (SaveFileDialog d = new SaveFileDialog ()) {
                d.Filter = "Text files|*.txt";
                d.FileName = "test.txt";
                DialogResult r = d.ShowDialog (this);
                ShowResult ("SaveFileDialog", r + " " + d.FileName);
            }
        }

        void BrowseFolder ()
        {
            using (FolderBrowserDialog d = new FolderBrowserDialog ()) {
                DialogResult r = d.ShowDialog (this);
                ShowResult ("FolderBrowserDialog", r + " " + d.SelectedPath);
            }
        }

        void ShowResult (string what, string text)
        {
            if (dialogResult != null) dialogResult.Text = what + ": " + text;
            SetStatus (what + " - " + text);
        }

        TabPage BuildThreads ()
        {
            TabPage p = Page ("Threads");
            threadProgress = new ProgressBar { Width = 300, Maximum = 100, Location = new Point (190, 12) };
            p.Controls.Add (threadProgress);

            threadLog = new TextBox {
                Multiline = true, ScrollBars = ScrollBars.Vertical,
                Location = new Point (190, 50), Size = new Size (520, 240)
            };
            p.Controls.Add (threadLog);

            Button w = new Button { Text = "Start Worker", Size = new Size (160, 28), Location = new Point (10, 10) };
            w.Click += delegate { StartWorker (); };
            p.Controls.Add (w);

            Button pool = new Button { Text = "ThreadPool", Size = new Size (160, 28), Location = new Point (10, 44) };
            pool.Click += delegate { RunPool (); };
            p.Controls.Add (pool);

            Button timer = new Button { Text = "Timer", Size = new Size (160, 28), Location = new Point (10, 78) };
            timer.Click += delegate { RunTimer (); };
            p.Controls.Add (timer);

            Button sync = new Button { Text = "Lock + Events", Size = new Size (160, 28), Location = new Point (10, 112) };
            sync.Click += delegate { RunSync (); };
            p.Controls.Add (sync);

            Button bw = new Button { Text = "BackgroundWorker", Size = new Size (160, 28), Location = new Point (10, 146) };
            bw.Click += delegate { RunBackgroundWorker (); };
            p.Controls.Add (bw);

            Button a1 = new Button { Text = "async/await", Size = new Size (160, 28), Location = new Point (10, 300) };
            a1.Click += RunAsyncHandler;
            p.Controls.Add (a1);

            Button a2 = new Button { Text = "Task.WhenAll", Size = new Size (160, 28), Location = new Point (10, 334) };
            a2.Click += RunWhenAllHandler;
            p.Controls.Add (a2);

            Button a3 = new Button { Text = "TaskCompletionSource", Size = new Size (160, 28), Location = new Point (10, 368) };
            a3.Click += RunTcsHandler;
            p.Controls.Add (a3);

            p.Controls.Add (new Label {
                Text = "Cross-thread UI updates use Control.Invoke / BeginInvoke.",
                Location = new Point (10, 190), Size = new Size (170, 60)
            });
            return p;
        }

        void LogThread (string s)
        {
            if (threadLog != null)
                threadLog.AppendText (s + "\r\n");
        }

        void StartWorker ()
        {
            if (worker != null && worker.IsAlive) {
                SetStatus ("Worker already running");
                return;
            }
            threadProgress.Value = 0;
            worker = new Thread (new ThreadStart (WorkerLoop));
            worker.IsBackground = true;
            worker.Start ();
            SetStatus ("Worker started");
        }

        void WorkerLoop ()
        {
            try {
                for (int i = 1; i <= 10; i++) {
                    Thread.Sleep (150);
                    int v = i * 10;
                    Invoke ((MethodInvoker) delegate {
                        threadProgress.Value = v;
                        LogThread ("worker tick " + v + "%");
                    });
                }
                Invoke ((MethodInvoker) delegate { SetStatus ("Worker finished"); });
            } catch (Exception e) {
                try { Invoke ((MethodInvoker) delegate { LogThread ("worker EX " + e.GetType ().Name + ": " + e.Message); }); } catch { }
            }
        }

        void RunPool ()
        {
            ThreadPool.QueueUserWorkItem (delegate (object st) {
                int sum = 0;
                for (int i = 1; i <= 100; i++)
                    sum += i;
                Thread.Sleep (300);
                BeginInvoke ((MethodInvoker) delegate { LogThread ("ThreadPool sum=" + sum); });
            });
            SetStatus ("ThreadPool queued");
        }

        void RunTimer ()
        {
            int n = 0;
            System.Threading.Timer t = null;
            t = new System.Threading.Timer (delegate (object st) {
                n++;
                int cur = n;
                BeginInvoke ((MethodInvoker) delegate { LogThread ("Timer tick " + cur); });
                if (cur >= 5)
                    t.Dispose ();
            }, null, 300, 300);
            SetStatus ("Timer started");
        }

        void RunSync ()
        {
            object gate = new object ();
            ManualResetEvent produced = new ManualResetEvent (false);
            ManualResetEvent consumed = new ManualResetEvent (false);
            int shared = 0;

            Thread producer = new Thread (delegate () {
                lock (gate) { shared = 42; }
                produced.Set ();
                consumed.WaitOne ();
                BeginInvoke ((MethodInvoker) delegate { LogThread ("producer: released by consumer"); });
            });
            Thread consumer = new Thread (delegate () {
                produced.WaitOne ();
                int v;
                lock (gate) { v = shared; }
                consumed.Set ();
                BeginInvoke ((MethodInvoker) delegate { LogThread ("consumer: shared=" + v); });
            });
            producer.IsBackground = true;
            consumer.IsBackground = true;
            producer.Start ();
            consumer.Start ();
            SetStatus ("Lock + events test running");
        }

        void RunBackgroundWorker ()
        {
            BackgroundWorker bw = new BackgroundWorker ();
            bw.WorkerReportsProgress = true;
            bw.DoWork += delegate (object s, DoWorkEventArgs e) {
                for (int i = 1; i <= 5; i++) {
                    Thread.Sleep (200);
                    bw.ReportProgress (i * 20);
                }
            };
            bw.ProgressChanged += delegate (object s, ProgressChangedEventArgs e) {
                threadProgress.Value = e.ProgressPercentage;
                LogThread ("BackgroundWorker " + e.ProgressPercentage + "%");
            };
            bw.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs e) {
                LogThread ("BackgroundWorker done");
                SetStatus ("BackgroundWorker done");
            };
            bw.RunWorkerAsync ();
            SetStatus ("BackgroundWorker started");
        }

        async void RunAsyncHandler (object sender, EventArgs e)
        {
            threadProgress.Value = 10;
            LogThread ("async start tid=" + Thread.CurrentThread.ManagedThreadId);
            await Task.Delay (500);
            LogThread ("after Delay tid=" + Thread.CurrentThread.ManagedThreadId + " invokeRequired=" + InvokeRequired);
            threadProgress.Value = 40;
            int sum = await Task.Run (delegate {
                int s = 0;
                for (int i = 1; i <= 100; i++) s += i;
                return s;
            });
            LogThread ("Task.Run sum=" + sum + " tid=" + Thread.CurrentThread.ManagedThreadId + " invokeRequired=" + InvokeRequired);
            threadProgress.Value = 70;
            await Task.Delay (300);
            threadProgress.Value = 100;
            LogThread ("async done tid=" + Thread.CurrentThread.ManagedThreadId + " invokeRequired=" + InvokeRequired);
        }

        async void RunWhenAllHandler (object sender, EventArgs e)
        {
            LogThread ("WhenAll start");
            Task<int> a = Task.Run (new Func<int> (delegate { Thread.Sleep (300); return 1; }));
            Task<int> b = Task.Run (new Func<int> (delegate { Thread.Sleep (500); return 2; }));
            int [] r = await Task.WhenAll (a, b);
            LogThread ("WhenAll " + r [0] + "," + r [1] + " tid=" + Thread.CurrentThread.ManagedThreadId + " invokeRequired=" + InvokeRequired);
        }

        async void RunTcsHandler (object sender, EventArgs e)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int> ();
            Thread th = new Thread (new ThreadStart (delegate { Thread.Sleep (400); tcs.SetResult (42); }));
            th.IsBackground = true;
            th.Start ();
            LogThread ("TCS waiting");
            int v = await tcs.Task;
            LogThread ("TCS result=" + v + " tid=" + Thread.CurrentThread.ManagedThreadId + " invokeRequired=" + InvokeRequired);
        }

        TabPage BuildMore ()
        {
            TabPage p = Page ("More");
            Button mdi = new Button { Text = "MDI", Size = new Size (140, 28), Location = new Point (10, 10) };
            mdi.Click += delegate { ShowMdi (); };
            p.Controls.Add (mdi);

            Button trayBtn = new Button { Text = "NotifyIcon", Size = new Size (140, 28), Location = new Point (10, 44) };
            trayBtn.Click += delegate { ToggleTray (); };
            p.Controls.Add (trayBtn);

            Button menu = new Button { Text = "ContextMenu", Size = new Size (140, 28), Location = new Point (10, 78) };
            menu.Click += delegate { ctx.Show (menu, new Point (0, menu.Height)); };
            p.Controls.Add (menu);

            Button ttip = new Button { Text = "ToolTip", Size = new Size (140, 28), Location = new Point (10, 112) };
            ttip.Click += delegate { tip.Show ("ToolTip text visible", ttip, new Point (0, ttip.Height), 4000); };
            p.Controls.Add (ttip);

            Label info = new Label { Location = new Point (200, 10), Size = new Size (520, 60), Text = "ContextMenuStrip, ToolTip, MDI, NotifyIcon and modal dialogs are all wired up." };
            p.Controls.Add (info);
            return p;
        }

        void ShowMdi ()
        {
            Form mdi = new Form ();
            mdi.Text = "MDI Parent";
            mdi.IsMdiContainer = true;
            mdi.ClientSize = new Size (360, 220);
            mdi.StartPosition = FormStartPosition.Manual;
            mdi.Location = new Point (80, 120);
            Form child = new Form ();
            child.Text = "MDI Child";
            child.MdiParent = mdi;
            mdi.Show ();
            child.Show ();
            SetStatus ("MDI shown");
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer ();
            t.Interval = 6000;
            t.Tick += delegate { t.Stop (); mdi.Close (); };
            t.Start ();
        }

        void ToggleTray ()
        {
            if (tray == null) {
                tray = new NotifyIcon ();
                tray.Icon = SystemIcons.Application;
                tray.Text = "Win98 Control Demo";
                tray.Visible = true;
                SetStatus ("NotifyIcon visible");
            } else {
                tray.Visible = !tray.Visible;
                SetStatus (tray.Visible ? "NotifyIcon visible" : "NotifyIcon hidden");
            }
        }
    }
}
