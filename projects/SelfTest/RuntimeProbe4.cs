using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

class P
{
    [DllImport ("kernel32.dll")]
    static extern int GetCurrentProcessId ();

    static void Main ()
    {
        Console.WriteLine ("pinvoke pid=" + GetCurrentProcessId ());
        try { Process p = Process.GetCurrentProcess (); Console.WriteLine ("process id=" + p.Id); }
        catch (Exception e) { Console.WriteLine ("PROC EX " + e.Message); }

        string d = "C:/WINDOWS/TEMP/p4_dir";
        try {
            Directory.CreateDirectory (d);
            Console.WriteLine ("created=" + Directory.Exists (d));
        } catch (Exception e) { Console.WriteLine ("CREATE EX " + e.Message); }
        try {
            File.WriteAllText (d + "/a.txt", "x");
            Console.WriteLine ("wrote");
        } catch (Exception e) { Console.WriteLine ("WRITE EX " + e.Message); }
        try {
            string [] f = Directory.GetFiles (d);
            Console.WriteLine ("files=" + f.Length);
        } catch (Exception e) { Console.WriteLine ("GETFILES EX " + e.Message); }
        try {
            Directory.Delete (d, true);
            Console.WriteLine ("deleted=" + !Directory.Exists (d));
        } catch (Exception e) { Console.WriteLine ("DELETE EX " + e.Message); }
        Console.WriteLine ("done");
    }
}
