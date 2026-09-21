using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string extFolder = Path.Combine(appDir, "extension");

            try
            {
                if (!string.IsNullOrEmpty(browserExePath) && File.Exists(browserExePath))
                {
                    string args = "--app=" + url;
                    if (Directory.Exists(extFolder))
                    {
                        args += " --load-extension=\"" + extFolder + "\"";
                    }

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = browserExePath,
                        Arguments = args,
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
        private string extPath;
        private bool isStarting = false;
        private FileSystemWatcher orxWatcher;
        private System.Threading.Timer updateDebounceTimer;

        public LauncherAppContext()
        {
            appDir = AppDomain.CurrentDomain.BaseDirectory;
            orxPath = Path.Combine(appDir, "orx.exe");
            extPath = Path.Combine(appDir, @"extension\content.js");

            InitializeTray();

            // Auto-check and patch orx.exe if an upstream update replaced it
            EnsureOrxPatched(false);

            StartOrxServer();
            SetupFileWatcher();

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

            // Re-apply UX enhancements menu item for convenient manual trigger
            var patchItem = new ToolStripMenuItem("Re-apply UI Enhancements (Fix Missing Buttons)", null, (s, e) =>
            {
                bool patched = EnsureOrxPatched(true);
                if (patched)
                {
                    RestartServer();
                    MessageBox.Show("UI Enhancements (Copy Response, Markdown Export & Deliverable Downloader) are active!", "OpenResearch UX Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Could not patch orx.exe. Verify that orx.exe is not locked.", "OpenResearch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            });
            contextMenu.Items.Add(patchItem);

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

        private void SetupFileWatcher()
        {
            try
            {
                orxWatcher = new FileSystemWatcher(appDir, "orx.exe");
                orxWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size;
                orxWatcher.Changed += (s, e) => ScheduleAutoPatchAfterUpdate();
                orxWatcher.Created += (s, e) => ScheduleAutoPatchAfterUpdate();
                orxWatcher.EnableRaisingEvents = true;
            }
            catch { }
        }

        private void ScheduleAutoPatchAfterUpdate()
        {
            if (updateDebounceTimer != null)
            {
                updateDebounceTimer.Dispose();
            }

            // Wait 1.5 seconds for file write / download to finish completely
            updateDebounceTimer = new System.Threading.Timer(_ =>
            {
                if (!IsOrxPatched(orxPath))
                {
                    KillOrxProcesses();
                    Thread.Sleep(800);
                    if (EnsureOrxPatched(false))
                    {
                        StartOrxServer();
                        if (trayIcon != null)
                        {
                            trayIcon.BalloonTipTitle = "OpenResearch Updated";
                            trayIcon.BalloonTipText = "App update detected! UI enhancements (copy/download buttons) automatically restored.";
                            trayIcon.BalloonTipIcon = ToolTipIcon.Info;
                            trayIcon.ShowBalloonTip(3000);
                        }
                    }
                }
            }, null, 1500, Timeout.Infinite);
        }

        public bool IsOrxPatched(string path)
        {
            try
            {
                if (!File.Exists(path)) return true;
                byte[] data = File.ReadAllBytes(path);
                byte[] marker = Encoding.ASCII.GetBytes("__ORX_UX__");
                return IndexOfBytes(data, marker, 0) != -1;
            }
            catch
            {
                return true; // Don't block if locked
            }
        }

        public bool EnsureOrxPatched(bool showFeedback)
        {
            try
            {
                if (!File.Exists(orxPath)) return false;
                if (!File.Exists(extPath)) return false;

                byte[] data = File.ReadAllBytes(orxPath);
                byte[] marker = Encoding.ASCII.GetBytes("__ORX_UX__");
                if (IndexOfBytes(data, marker, 0) != -1)
                {
                    return true; // Already patched
                }

                // If currently running, stop before patching binary
                KillOrxProcesses();
                Thread.Sleep(600);

                const string targetCommentStr = "/**\n * @license lucide-react v1.23.0 - ISC\n *\n * This source code is licensed under the ISC license.\n * See the LICENSE file in the root directory of this source tree.\n */";
                byte[] targetComment = Encoding.UTF8.GetBytes(targetCommentStr);
                const string replacementStr = "/*ISC*/";

                List<int> matches = new List<int>();
                int pos = 0;
                while (true)
                {
                    int idx = IndexOfBytes(data, targetComment, pos);
                    if (idx == -1) break;
                    matches.Add(idx);
                    pos = idx + targetComment.Length;
                }

                if (matches.Count == 0) return false;

                int minPos = matches[0];
                int maxPos = matches[matches.Count - 1] + targetComment.Length;
                int regionLen = maxPos - minPos;

                byte[] origRegion = new byte[regionLen];
                Buffer.BlockCopy(data, minPos, origRegion, 0, regionLen);
                string regionStr = Encoding.UTF8.GetString(origRegion);

                string uxRaw = File.ReadAllText(extPath, Encoding.UTF8);
                string uxMin = Regex.Replace(uxRaw, @"(?m)^\s*//.*$", "");
                uxMin = Regex.Replace(uxMin, @"\s+", " ").Trim();
                byte[] uxBytes = Encoding.UTF8.GetBytes(uxMin);

                string replacedStr = regionStr.Replace(targetCommentStr, replacementStr);
                byte[] replacedBytes = Encoding.UTF8.GetBytes(replacedStr);

                int spaceSaved = origRegion.Length - replacedBytes.Length;
                int deficit = spaceSaved - uxBytes.Length;
                if (deficit < 4) return false;

                string pad = "/*" + new string(' ', deficit - 4) + "*/";
                int firstIsc = replacedStr.IndexOf(replacementStr);
                if (firstIsc == -1) return false;

                string finalRegionStr = replacedStr.Substring(0, firstIsc + replacementStr.Length)
                                      + uxMin
                                      + pad
                                      + replacedStr.Substring(firstIsc + replacementStr.Length);

                byte[] newRegionBytes = Encoding.UTF8.GetBytes(finalRegionStr);
                if (newRegionBytes.Length != regionLen) return false;

                Buffer.BlockCopy(newRegionBytes, 0, data, minPos, regionLen);

                // Bust browser cache in index.html so web app immediately pulls fresh JS
                try
                {
                    byte[] htmlTag = Encoding.UTF8.GetBytes("<!doctype html>");
                    int htmlPos = IndexOfBytes(data, htmlTag, 0);
                    if (htmlPos != -1)
                    {
                        int htmlEnd = IndexOfBytes(data, Encoding.UTF8.GetBytes("</html>"), htmlPos);
                        if (htmlEnd != -1)
                        {
                            int hLen = (htmlEnd + 7) - htmlPos;
                            byte[] hBytes = new byte[hLen];
                            Buffer.BlockCopy(data, htmlPos, hBytes, 0, hLen);
                            string hStr = Encoding.UTF8.GetString(hBytes);
                            const string oldC = "/* Pre-CSS background; keep in sync with --base in src/tailwind.css. */";
                            const string newC = "/* Pre-CSS background; keep in sync with --base in tailwind.css. */";
                            if (hStr.Contains(oldC))
                            {
                                string newH = hStr.Replace(oldC, newC);
                                newH = Regex.Replace(newH, @"(src=""/assets/index-[^""]+\.js)""", "$1?v=2\"");
                                byte[] newHBytes = Encoding.UTF8.GetBytes(newH);
                                if (newHBytes.Length == hLen)
                                {
                                    Buffer.BlockCopy(newHBytes, 0, data, htmlPos, hLen);
                                }
                            }
                        }
                    }
                }
                catch { }

                string bak = orxPath + ".orig";
                if (!File.Exists(bak))
                {
                    try { File.Copy(orxPath, bak); } catch { }
                }

                File.WriteAllBytes(orxPath, data);

                if (showFeedback && trayIcon != null)
                {
                    trayIcon.BalloonTipTitle = "UI Enhancements Restored";
                    trayIcon.BalloonTipText = "OpenResearch was auto-patched with Copy Response & Markdown Downloaders.";
                    trayIcon.BalloonTipIcon = ToolTipIcon.Info;
                    trayIcon.ShowBalloonTip(2500);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static int IndexOfBytes(byte[] haystack, byte[] needle, int startIndex)
        {
            if (needle.Length == 0 || haystack.Length < needle.Length) return -1;
            int max = haystack.Length - needle.Length;
            for (int i = startIndex; i <= max; i++)
            {
                if (haystack[i] == needle[0])
                {
                    bool match = true;
                    for (int j = 1; j < needle.Length; j++)
                    {
                        if (haystack[i + j] != needle[j])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match) return i;
                }
            }
            return -1;
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

            // Ensure orx.exe has UX enhancements before launch
            EnsureOrxPatched(false);

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

            if (orxWatcher != null)
            {
                orxWatcher.EnableRaisingEvents = false;
                orxWatcher.Dispose();
            }

            if (updateDebounceTimer != null)
            {
                updateDebounceTimer.Dispose();
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
