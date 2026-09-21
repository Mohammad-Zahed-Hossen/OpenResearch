using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace OpenResearchDesktop
{
    static class Program
    {
        private static Mutex appMutex = null;

        [STAThread]
        static void Main()
        {
            const string appName = "OpenResearchDesktopApp_LauncherMutex";
            bool createdNew;
            appMutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                OpenAppWindow(GetDefaultBrowser());
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LauncherAppContext());
        }

        public static string GetCometPath()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string cometPath = Path.Combine(localAppData, @"Perplexity\Comet\Application\comet.exe");
            return File.Exists(cometPath) ? cometPath : null;
        }

        public static string GetBravePath()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            string braveUserPath = Path.Combine(localAppData, @"BraveSoftware\Brave-Browser\Application\brave.exe");
            if (File.Exists(braveUserPath)) return braveUserPath;

            string braveProgPath = Path.Combine(progFiles, @"BraveSoftware\Brave-Browser\Application\brave.exe");
            if (File.Exists(braveProgPath)) return braveProgPath;

            return null;
        }

        public static string GetEdgePath()
        {
            string progFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string edgePath = Path.Combine(progFilesX86, @"Microsoft\Edge\Application\msedge.exe");
            if (File.Exists(edgePath)) return edgePath;

            string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string edgePath64 = Path.Combine(progFiles, @"Microsoft\Edge\Application\msedge.exe");
            if (File.Exists(edgePath64)) return edgePath64;

            return null;
        }

        public static string GetDefaultBrowser()
        {
            string comet = GetCometPath();
            if (comet != null) return comet;

            string brave = GetBravePath();
            if (brave != null) return brave;

            return GetEdgePath();
        }

        public static void OpenAppWindow(string browserExePath)
        {
            const string url = "http://127.0.0.1:4791";

            try
            {
                if (!string.IsNullOrEmpty(browserExePath) && File.Exists(browserExePath))
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = browserExePath,
                        Arguments = "--app=" + url,
                        UseShellExecute = false
                    };
                    Process.Start(psi);
                }
                else
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open OpenResearch: " + ex.Message, "OpenResearch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }


    public class LauncherAppContext : ApplicationContext
    {
        private const int PORT = 4791;

        private NotifyIcon trayIcon;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem statusMenuItem;
        private ToolStripMenuItem startStopMenuItem;
        private System.Windows.Forms.Timer healthCheckTimer;
        private Process orxProcess;
        private string appDir;
        private string orxPath;
        private bool isStarting = false;

        public LauncherAppContext()
        {
            appDir = AppDomain.CurrentDomain.BaseDirectory;
            orxPath = Path.Combine(appDir, "orx.exe");

            InitializeTray();
            StartOrxServer();

            healthCheckTimer = new System.Windows.Forms.Timer();
            healthCheckTimer.Interval = 4000;
            healthCheckTimer.Tick += (s, e) => UpdateServerStatus();
            healthCheckTimer.Start();
        }

        private void InitializeTray()
        {
            contextMenu = new ContextMenuStrip();

            string cometPath = Program.GetCometPath();
            string bravePath = Program.GetBravePath();

            if (cometPath != null)
            {
                var cometItem = new ToolStripMenuItem("Open in Comet (App Mode)", null, (s, e) => Program.OpenAppWindow(cometPath));
                cometItem.Font = new Font(cometItem.Font, FontStyle.Bold);
                contextMenu.Items.Add(cometItem);
            }

            if (bravePath != null)
            {
                var braveItem = new ToolStripMenuItem("Open in Brave (App Mode)", null, (s, e) => Program.OpenAppWindow(bravePath));
                if (cometPath == null) braveItem.Font = new Font(braveItem.Font, FontStyle.Bold);
                contextMenu.Items.Add(braveItem);
            }

            contextMenu.Items.Add(new ToolStripMenuItem("Open in Default Browser", null, (s, e) =>
            {
                try { Process.Start(new ProcessStartInfo("http://127.0.0.1:" + PORT) { UseShellExecute = true }); } catch { }
            }));

            contextMenu.Items.Add(new ToolStripSeparator());

            statusMenuItem = new ToolStripMenuItem("Status: Checking...");
            statusMenuItem.Enabled = false;
            contextMenu.Items.Add(statusMenuItem);

            startStopMenuItem = new ToolStripMenuItem("Stop OpenResearch", null, (s, e) => ToggleServer());
            contextMenu.Items.Add(startStopMenuItem);

            contextMenu.Items.Add(new ToolStripMenuItem("Restart Server", null, (s, e) => RestartServer()));

            contextMenu.Items.Add(new ToolStripSeparator());

            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string artifactsDir = Path.Combine(userProfile, @".local\share\openresearch\files");
            var filesFolderItem = new ToolStripMenuItem("Open Generated Files Folder", null, (s, e) =>
            {
                try
                {
                    if (Directory.Exists(artifactsDir))
                        Process.Start("explorer.exe", artifactsDir);
                    else
                    {
                        string baseOrxDir = Path.Combine(userProfile, @".local\share\openresearch");
                        if (Directory.Exists(baseOrxDir))
                            Process.Start("explorer.exe", baseOrxDir);
                    }
                }
                catch { }
            });
            filesFolderItem.Font = new Font(filesFolderItem.Font, FontStyle.Bold);
            contextMenu.Items.Add(filesFolderItem);

            string extFolder = Path.Combine(appDir, "extension");
            contextMenu.Items.Add(new ToolStripMenuItem("Open Browser Extension Folder", null, (s, e) =>
            {
                try
                {
                    if (Directory.Exists(extFolder))
                        Process.Start("explorer.exe", extFolder);
                }
                catch { }
            }));

            contextMenu.Items.Add(new ToolStripSeparator());

            contextMenu.Items.Add(new ToolStripMenuItem("Exit Launcher", null, (s, e) => ExitApplication()));


            Icon icon = null;
            string icoPath = Path.Combine(appDir, "app.ico");
            if (File.Exists(icoPath))
            {
                try { icon = new Icon(icoPath); } catch { }
            }
            if (icon == null)
            {
                try { icon = Icon.ExtractAssociatedIcon(orxPath); } catch { }
            }
            if (icon == null)
            {
                icon = SystemIcons.Application;
            }

            trayIcon = new NotifyIcon
            {
                Icon = icon,
                ContextMenuStrip = contextMenu,
                Text = "OpenResearch Desktop (127.0.0.1:" + PORT + ")",
                Visible = true
            };

            trayIcon.DoubleClick += (s, e) => Program.OpenAppWindow(Program.GetDefaultBrowser());
        }

        private bool IsPortListening(int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    IAsyncResult result = client.BeginConnect("127.0.0.1", port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(300);
                    if (success && client.Connected)
                    {
                        client.EndConnect(result);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private void UpdateServerStatus()
        {
            bool running = IsPortListening(PORT);
            if (running)
            {
                statusMenuItem.Text = "Status: Running (Port " + PORT + ")";
                statusMenuItem.ForeColor = Color.DarkGreen;
                startStopMenuItem.Text = "Stop OpenResearch";
                startStopMenuItem.Enabled = true;
                trayIcon.Text = "OpenResearch: Running (127.0.0.1:" + PORT + ")";
            }
            else if (isStarting)
            {
                statusMenuItem.Text = "Status: Starting up...";
                statusMenuItem.ForeColor = Color.DarkOrange;
                startStopMenuItem.Enabled = false;
                trayIcon.Text = "OpenResearch: Starting...";
            }
            else
            {
                statusMenuItem.Text = "Status: Stopped";
                statusMenuItem.ForeColor = Color.DarkRed;
                startStopMenuItem.Text = "Start OpenResearch";
                startStopMenuItem.Enabled = true;
                trayIcon.Text = "OpenResearch: Stopped";
            }
        }

        private void StartOrxServer()
        {
            if (IsPortListening(PORT))
            {
                UpdateServerStatus();
                Program.OpenAppWindow(Program.GetDefaultBrowser());
                return;
            }

            if (!File.Exists(orxPath))
            {
                MessageBox.Show("orx.exe not found in: " + appDir, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            isStarting = true;
            UpdateServerStatus();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = orxPath,
                        Arguments = "up --no-browser",
                        WorkingDirectory = appDir,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    orxProcess = Process.Start(psi);

                    int attempts = 0;
                    while (attempts < 50)
                    {
                        if (IsPortListening(PORT))
                        {
                            break;
                        }
                        Thread.Sleep(300);
                        attempts++;
                    }

                    isStarting = false;

                    trayIcon.BalloonTipTitle = "OpenResearch Ready";
                    trayIcon.BalloonTipText = "Running cleanly on 127.0.0.1:" + PORT;
                    trayIcon.BalloonTipIcon = ToolTipIcon.Info;
                    trayIcon.ShowBalloonTip(2000);

                    Program.OpenAppWindow(Program.GetDefaultBrowser());
                }
                catch (Exception ex)
                {
                    isStarting = false;
                    MessageBox.Show("Failed to start orx.exe: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        private void KillOrxProcesses()
        {
            try
            {
                if (orxProcess != null && !orxProcess.HasExited)
                {
                    orxProcess.Kill();
                    orxProcess.WaitForExit(2000);
                }
            }
            catch { }

            try
            {
                foreach (var p in Process.GetProcessesByName("orx"))
                {
                    try { p.Kill(); p.WaitForExit(1000); } catch { }
                }
            }
            catch { }
        }

        private void ToggleServer()
        {
            if (IsPortListening(PORT))
            {
                KillOrxProcesses();
                Thread.Sleep(500);
                UpdateServerStatus();
            }
            else
            {
                StartOrxServer();
            }
        }

        private void RestartServer()
        {
            KillOrxProcesses();
            Thread.Sleep(800);
            StartOrxServer();
        }

        private void ExitApplication()
        {
            var result = MessageBox.Show(
                "Do you want to stop the OpenResearch server before exiting?",
                "Exit OpenResearch Launcher",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Cancel) return;

            if (result == DialogResult.Yes)
            {
                KillOrxProcesses();
            }

            if (healthCheckTimer != null)
            {
                healthCheckTimer.Stop();
                healthCheckTimer.Dispose();
            }

            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }

            Application.Exit();
        }
    }
}
