using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

class Program
{
    static FileStream fs;
    static object gate = new object ();

    static void Log (string s)
    {
        try {
            lock (gate) {
                byte [] b = Encoding.ASCII.GetBytes (DateTime.Now.ToString ("HH:mm:ss.fff") + " " + s + "\r\n");
                fs.Write (b, 0, b.Length);
                fs.Flush ();
            }
        } catch { }
    }

    [DllImport ("kernel32.dll")] static extern IntPtr LoadLibraryA (string name);
    [DllImport ("kernel32.dll")] static extern IntPtr GetProcAddress (IntPtr h, string name);
    [UnmanagedFunctionPointer (CallingConvention.StdCall)] delegate bool SetCursorPosDel (int x, int y);
    [UnmanagedFunctionPointer (CallingConvention.StdCall)] delegate void MouseEventDel (uint flags, uint dx, uint dy, uint data, UIntPtr extra);
    [UnmanagedFunctionPointer (CallingConvention.StdCall)] delegate bool SetForegroundWindowDel (IntPtr h);

    static SetCursorPosDel SetCursorPos;
    static MouseEventDel mouse_event;
    static SetForegroundWindowDel SetForegroundWindow;

    static void ResolveUser32 ()
    {
        IntPtr u = LoadLibraryA ("USER32.DLL");
        Log ("user32=" + u);
        SetCursorPos = (SetCursorPosDel) System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer (
            GetProcAddress (u, "SetCursorPos"), typeof (SetCursorPosDel));
        mouse_event = (MouseEventDel) System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer (
            GetProcAddress (u, "mouse_event"), typeof (MouseEventDel));
        SetForegroundWindow = (SetForegroundWindowDel) System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer (
            GetProcAddress (u, "SetForegroundWindow"), typeof (SetForegroundWindowDel));
    }

    [DllImport ("kernel32.dll")] static extern uint GetTickCount ();

    const uint LEFTDOWN = 0x0002, LEFTUP = 0x0004, RIGHTDOWN = 0x0008, RIGHTUP = 0x0010;

    static Form form;
    static TextBox box;
    static Label status;
    static Timer timer;

    static void ClickAt (Point screen, uint down, uint up, string what)
    {
        Log ("click " + what + " at " + screen.X + "," + screen.Y + " before SetCursorPos");
        SetCursorPos (screen.X, screen.Y);
        Log ("click " + what + " at " + screen.X + "," + screen.Y + " before down");
        mouse_event (down, 0, 0, 0, UIntPtr.Zero);
        Log ("click " + what + " after down");
        System.Threading.Thread.Sleep (80);
        mouse_event (up, 0, 0, 0, UIntPtr.Zero);
        Log ("click " + what + " after up");
    }

    [STAThread]
    static void Main ()
    {
        fs = new FileStream ("C:\\MINICTX.TXT", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        Log ("start os=" + Environment.OSVersion.VersionString);
        try { ResolveUser32 (); Log ("user32 resolved"); }
        catch (Exception e) { Log ("user32 resolve failed " + e); }

        AppDomain.CurrentDomain.UnhandledException += delegate (object s, UnhandledExceptionEventArgs e) {
            Log ("UNHANDLED " + e.ExceptionObject);
        };
        Application.ThreadException += delegate (object s, System.Threading.ThreadExceptionEventArgs e) {
            Log ("THREADEX " + e.Exception);
        };

        form = new Form ();
        form.Text = "MiniCtx";
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point (60, 40);
        form.ClientSize = new Size (420, 300);

        box = new TextBox ();
        box.SetBounds (40, 40, 300, 22);
        box.Text = "right click me";
        form.Controls.Add (box);

        Button b = new Button ();
        b.Text = "target";
        b.SetBounds (40, 150, 120, 30);
        form.Controls.Add (b);

        status = new Label ();
        status.SetBounds (40, 240, 340, 40);
        status.Text = "status";
        form.Controls.Add (status);

        box.MouseDown += delegate (object s, MouseEventArgs e) { Log ("textbox mousedown " + e.Button); };
        box.MouseUp += delegate (object s, MouseEventArgs e) { Log ("textbox mouseup " + e.Button); };
        b.MouseDown += delegate (object s, MouseEventArgs e) { Log ("button mousedown " + e.Button); };
        b.MouseUp += delegate (object s, MouseEventArgs e) { Log ("button mouseup " + e.Button); };
        form.MouseDown += delegate (object s, MouseEventArgs e) { Log ("form mousedown " + e.Button); };
        form.MouseUp += delegate (object s, MouseEventArgs e) { Log ("form mouseup " + e.Button); };
        form.Activated += delegate { Log ("form activated"); };
        form.Deactivate += delegate { Log ("form deactivate"); };
        form.Shown += delegate { Log ("form shown"); };

        ContextMenu cm = box.ContextMenu;
        Log ("textbox contextmenu null=" + (cm == null));
        if (cm != null) {
            cm.Popup += delegate { Log ("contextmenu popup"); };
            cm.Collapse += delegate { Log ("contextmenu collapse"); };
        }

        int phase = 0;
        timer = new Timer ();
        timer.Interval = 700;
        timer.Tick += delegate {
            timer.Stop ();
            phase++;
            Log ("tick phase " + phase + " tickcount=" + GetTickCount ());
            if (phase == 1) {
                SetForegroundWindow (form.Handle);
                form.Activate ();
                box.Focus ();
                Log ("focus set; scheduling right click");
                Point p = box.PointToScreen (new Point (20, 10));
                ClickAt (p, RIGHTDOWN, RIGHTUP, "right-on-textbox");
                Log ("right click sent");
                timer.Interval = 1200;
                timer.Start ();
            } else if (phase == 2) {
                Log ("phase2: menu dismissed? box focus=" + box.Focused);
                Point p = new Point (form.Left + 20, form.Top + form.Height - 25);
                ClickAt (p, LEFTDOWN, LEFTUP, "left-on-form-empty");
                Log ("left click sent");
                timer.Interval = 1000;
                timer.Start ();
            } else if (phase == 3) {
                Log ("phase3 ALIVE box.text=" + box.Text);
                status.Text = "alive";
                Log ("closing");
                form.Close ();
            }
        };
        form.Shown += delegate { timer.Start (); };

        try {
            Application.Run (form);
            Log ("Application.Run returned");
        } catch (Exception e) {
            Log ("FATAL " + e);
        }
        Log ("exit");
        try { fs.Close (); } catch { }
    }
}
