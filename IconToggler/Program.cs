using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IconToggler
{
    class Program
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private static NotifyIcon trayIcon;
        private static bool iconsVisible = false;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            trayIcon = new NotifyIcon()
            {
                Icon = LoadEmbeddedIcon("IconToggler.Desktop_Icon_Toggle.ico"),
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Beenden", Exit)
                }),
                Visible = true
            };

            trayIcon.DoubleClick += ToggleIcons;

            // Beim Start Icons verstecken
            SetDesktopIconsVisible(false);

            // Ereignis beim Schließen registrieren
            Application.ApplicationExit += OnApplicationExit;

            Application.Run();
        }

        private static Icon LoadEmbeddedIcon(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    return new Icon(stream);
                }
                throw new FileNotFoundException("Icon-Ressource nicht gefunden: " + resourceName);
            }
        }

        private static void ToggleIcons(object sender, EventArgs e)
        {
            iconsVisible = !iconsVisible;
            SetDesktopIconsVisible(iconsVisible);
        }

        private static void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            // Beim Beenden Icons wieder einblenden
            SetDesktopIconsVisible(true);
        }

        private static void SetDesktopIconsVisible(bool show)
        {
            IntPtr hWndProgman = FindWindow("Progman", null);
            IntPtr hWndDefView = FindWindowEx(hWndProgman, IntPtr.Zero, "SHELLDLL_DefView", null);

            if (hWndDefView == IntPtr.Zero)
            {
                IntPtr hWndWorkerW = IntPtr.Zero;
                do
                {
                    hWndWorkerW = FindWindowEx(IntPtr.Zero, hWndWorkerW, "WorkerW", null);
                    hWndDefView = FindWindowEx(hWndWorkerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                }
                while (hWndWorkerW != IntPtr.Zero && hWndDefView == IntPtr.Zero);
            }

            if (hWndDefView != IntPtr.Zero)
            {
                ShowWindow(hWndDefView, show ? SW_SHOW : SW_HIDE);
            }
        }
    }
}
