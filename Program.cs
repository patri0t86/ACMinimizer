using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ACMinimizer;

internal static class Program
{
    private static ContextMenuStrip? menu;
    private static NotifyIcon? Icon;
    private static bool Enabled = true;
    private static System.Windows.Forms.Timer? Timer;
    private static Bitmap? CheckMark;
    private static Icon? AppIcon;
    private const int SW_MINIMIZE = 6;
    [DllImport("User32")]
    private static extern int ShowWindow(int hwnd, int nCmdShow);


    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        Setup();

        Application.Run();
    }

    public static void Setup()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Set the check mark icon
        using var bitmap = assembly.GetManifestResourceStream("ACMinimizer.Check.png");
        if (bitmap != null) CheckMark = new Bitmap(bitmap);

        // Set the notification area icon
        using var icon = assembly.GetManifestResourceStream("ACMinimizer.App.ico");
        if (icon != null) AppIcon = new Icon(icon);

        Timer = new()
        {
            Enabled = Enabled,
            Interval = 20000
        };
        Timer.Tick += Timer_Tick;

        menu = new ContextMenuStrip();
        menu.Items.Add("&Enable", CheckMark, Enable);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("E&xit", null, Close);
        menu.ShowCheckMargin = true;

        Icon = new NotifyIcon
        {
            Visible = true,
            Text = "AC Minimizer",
            Icon = AppIcon,
            ContextMenuStrip = menu
        };
    }

    public static void Enable(object? sender, EventArgs e)
    {
        Enabled = !Enabled;
        if (Enabled)
        {
            Timer!.Enabled = true;
            menu!.Items[0].Image = CheckMark;
        }
        else
        {
            Timer!.Enabled = false;
            menu!.Items[0].Image = null;
        }
    }

    private static void Timer_Tick(object? sender, EventArgs e) 
    {
        Process[] processes = Process.GetProcessesByName("acclient");
        foreach (Process process in processes)
        {
            ShowWindow(process.MainWindowHandle.ToInt32(), SW_MINIMIZE);        
        }
    }

    public static void Close(object? sender, EventArgs e)
    {
        Application.Exit();
    }
}