using System;
using System.Runtime.InteropServices;
class P {
  [DllImport("kernel32.dll", SetLastError=true)]
  static extern bool SetThreadErrorMode(uint m, out uint o);
  [DllImport("kernel32.dll")]
  static extern uint GetTickCount();
  static void Main() {
    Console.WriteLine("tick=" + GetTickCount());
    try {
      uint o;
      bool r = SetThreadErrorMode(1, out o);
      Console.WriteLine("stem=" + r + " old=" + o);
    } catch (Exception e) { Console.WriteLine("EX: " + e.GetType().Name + ": " + e.Message); }
    Console.WriteLine("done");
  }
}
