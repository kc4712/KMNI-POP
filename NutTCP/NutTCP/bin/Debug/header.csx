
static CShareMemory.CShareMemory ShareCtl = new CShareMemory.CShareMemory();

//static UTSLib.UTSLib UTS = new UTSLib.UTSLib();
static pop_terminal.frmMain pop = new  pop_terminal.frmMain();

// Win32 API
[DllImport("user32.dll")]
public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
[DllImport("user32.dll")]
public static extern void BringWindowToTop(IntPtr hWnd);
[DllImport("user32.dll")]
public static extern void SetForegroundWindow(IntPtr hWnd);
[DllImport("User32.dll")]
public static extern uint RegisterWindowMessage(string lpString);
[DllImport("user32.dll")]
public extern static int FindWindow(string lpClassName,string lpWindowName); 
[DllImport("user32.dll")]
public static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);