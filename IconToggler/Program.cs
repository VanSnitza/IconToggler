using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using IconToggler.Properties;

namespace IconToggler
{
    class Program
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_COMMAND = 0x0111;
        private const int TOGGLE_DESKTOP_ICONS = 0x7402;

        private static NotifyIcon trayIcon;
        private static bool autoHideEnabled = false;
        private static bool autoStartEnabled = false;
        private static Timer autoHideTimer;
        private static int autoHideInterval = 10; // Default 10 seconds
        private static ToolStripMenuItem autoHideMenuItem;
        private static ToolStripMenuItem autoHideLabel;
        private static TrackBar autoHideTrackBar;
        private static ToolStripMenuItem autoStartMenuItem;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            autoHideTimer = new Timer();
            autoHideTimer.Tick += AutoHideTimer_Tick;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            autoHideMenuItem = new ToolStripMenuItem("Auto-Hide Icons", null, ToggleAutoHide) { Checked = autoHideEnabled };
            autoHideLabel = new ToolStripMenuItem("Auto-Hide Time: " + autoHideInterval + "s");
            autoStartMenuItem = new ToolStripMenuItem("Auto-Start", null, ToggleAutoStart) { Checked = autoStartEnabled };
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ToolStripMenuItem aboutMenuItem = new ToolStripMenuItem($"About - IconToggler v{version}", null, ShowAboutDialog); 
            //ToolStripMenuItem aboutMenuItem = new ToolStripMenuItem("Über", null, ShowAboutDialog);

            autoHideTrackBar = new TrackBar()
            {
                Minimum = 5,
                Maximum = 60,
                Value = autoHideInterval,
                TickFrequency = 5,
                SmallChange = 1,
                LargeChange = 5,
                Width = aboutMenuItem.GetPreferredSize(Size.Empty).Width
        };

            autoHideTrackBar.ValueChanged += (s, e) =>
            {
                autoHideInterval = autoHideTrackBar.Value;
                autoHideLabel.Text = "Auto-Hide Time: " + autoHideInterval + "s";
                SaveSettings();
            };

            ToolStripControlHost trackBarHost = new ToolStripControlHost(autoHideTrackBar);

            contextMenu.Items.Add(autoHideMenuItem);
            contextMenu.Items.Add(autoHideLabel);
            contextMenu.Items.Add(trackBarHost);
            contextMenu.Items.Add(autoStartMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(aboutMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Beenden", null, Exit);

            trayIcon = new NotifyIcon()
            {
                Icon = LoadEmbeddedIcon("IconToggler.Desktop_Icon_Toggle.ico"),
                ContextMenuStrip = contextMenu,
                Visible = true
            };

            trayIcon.DoubleClick += ToggleIcons;

            LoadSettings();
            autoHideMenuItem.Checked = autoHideEnabled;
            autoHideTrackBar.Value = autoHideInterval;
            autoStartMenuItem.Checked = autoStartEnabled;

            // Beim Start Icons verstecken
            if (GetDesktopIconsVisible())
            {
                SetDesktopIconsVisible(false);
            }

            // Ereignis beim Schließen registrieren
            Application.ApplicationExit += OnApplicationExit;

            Application.Run();
        }

        private static void ShowAboutDialog(object sender, EventArgs e)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MessageBox.Show($"IconToggler\nVersion: {version}\n\n© 2025, github.com/VanSnitza", "About IconToggler", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        private static void AutoHideTimer_Tick(object sender, EventArgs e)
        {
            autoHideTimer.Stop();
            SetDesktopIconsVisible(false);
        }

        private static void ToggleIcons(object sender, EventArgs e)
        {
            bool currentlyVisible = GetDesktopIconsVisible();
            SetDesktopIconsVisible(!currentlyVisible);

            if (!currentlyVisible && autoHideEnabled)
            {
                autoHideTimer.Interval = autoHideInterval * 1000;
                autoHideTimer.Start();
            }
            else
            {
                autoHideTimer.Stop();
            }
        }

        private static void ToggleAutoHide(object sender, EventArgs e)
        {
            autoHideEnabled = !autoHideEnabled;
            autoHideMenuItem.Checked = autoHideEnabled;
            SaveSettings();
        }

        private static void ToggleAutoStart(object sender, EventArgs e)
        {
            autoStartEnabled = !autoStartEnabled;
            SetAutoStart(autoStartEnabled);
            autoStartMenuItem.Checked = autoStartEnabled;
            SaveSettings();
        }

        private static void SaveSettings()
        {
            Settings.Default.AutoHideEnabled = autoHideEnabled;
            Settings.Default.AutoHideInterval = autoHideInterval;
            Settings.Default.AutoStartEnabled = autoStartEnabled;
            Settings.Default.Save();
        }

        private static void LoadSettings()
        {
            autoHideEnabled = Settings.Default.AutoHideEnabled;
            autoHideInterval = Settings.Default.AutoHideInterval;
            autoStartEnabled = Settings.Default.AutoStartEnabled;
        }

        private static bool GetDesktopIconsVisible()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", false))
            {
                return key?.GetValue("HideIcons") is int hideIcons && hideIcons == 0;
            }
        }

        private static void SetDesktopIconsVisible(bool show)
        {
            if (GetDesktopIconsVisible() != show)
            {
                IntPtr hWndDefView = GetDesktopWindowHandle();
                if (hWndDefView != IntPtr.Zero)
                {
                    PostMessage(hWndDefView, WM_COMMAND, (IntPtr)TOGGLE_DESKTOP_ICONS, IntPtr.Zero);
                }
            }
        }

        private static IntPtr GetDesktopWindowHandle()
        {
            IntPtr hWndProgman = FindWindow("Progman", null);
            IntPtr hWndDefView = FindWindowEx(hWndProgman, IntPtr.Zero, "SHELLDLL_DefView", null);

            if (hWndDefView == IntPtr.Zero)
            {
                // Falls Progman nicht das DefView-Fenster hat, versuchen wir WorkerW
                IntPtr hWndWorkerW = IntPtr.Zero;
                do
                {
                    hWndWorkerW = FindWindowEx(IntPtr.Zero, hWndWorkerW, "WorkerW", null);
                    hWndDefView = FindWindowEx(hWndWorkerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                }
                while (hWndWorkerW != IntPtr.Zero && hWndDefView == IntPtr.Zero);
            }

            return hWndDefView;
        }


        private static void SetAutoStart(bool enable)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (enable)
                    key.SetValue("IconToggler", Application.ExecutablePath);
                else
                    key.DeleteValue("IconToggler", false);
            }
        }

        private static void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            SetDesktopIconsVisible(true);
        }

        private static Icon LoadEmbeddedIcon(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                return stream != null ? new Icon(stream) : throw new FileNotFoundException("Icon-Ressource nicht gefunden: " + resourceName);
            }
        }
    }
}
