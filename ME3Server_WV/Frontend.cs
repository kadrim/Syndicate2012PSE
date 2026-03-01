using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace ME3Server_WV
{
    public partial class Frontend : Window
    {
        public static string loc = AppContext.BaseDirectory;
        public static string sysdir = Environment.GetFolderPath(Environment.SpecialFolder.System) + Path.DirectorySeparatorChar;
        private static Frontend frontend;

        public Frontend()
        {
            InitializeComponent();
            frontend = this;
            this.Opened += Frontend_Shown;
        }

        public static void UpdateLogLevelMenu()
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (frontend == null) return;
                var level0 = frontend.FindControl<MenuItem>("level0MenuItem");
                var level3 = frontend.FindControl<MenuItem>("level3MenuItem");
                var level5 = frontend.FindControl<MenuItem>("level5MenuItem");
                // Avalonia MenuItems don't have Checked, use Icon marker instead
                if (level0 != null) level0.Header = (Logger.LogLevel == 0 ? "* " : "") + "Level 0 Most Critical";
                if (level3 != null) level3.Header = (Logger.LogLevel == 3 ? "* " : "") + "Level 3 Moderate";
                if (level5 != null) level5.Header = (Logger.LogLevel == 5 ? "* " : "") + "Level 5 Everything";
            });
        }

        public static void UpdateMITMMenuState()
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (frontend == null) return;
                var activateItem = frontend.FindControl<MenuItem>("activateMITMMenuItem");
                var recordItem = frontend.FindControl<MenuItem>("recordPlayerSettingsMenuItem");
                var importItem = frontend.FindControl<MenuItem>("importPlayerSettingsMenuItem");
                if (activateItem != null) activateItem.Header = ME3Server.isMITM ? "Deactivate" : "Activate";
                if (recordItem != null) recordItem.IsEnabled = ME3Server.isMITM;
                if (importItem != null) importItem.IsEnabled = ME3Server.isMITM;
            });
        }

        private void aktivateRedirectionMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ActivateRedirection(Config.FindEntry("RedirectIP"));
        }

        public static void ActivateRedirection(string hostIP)
        {
            DeactivateRedirection(false);
            try
            {
                List<string> r = new List<string>(File.ReadAllLines(loc + "conf" + Path.DirectorySeparatorChar + "redirect.txt"));
                string hostsPath = GetHostsFilePath();
                List<string> h = new List<string>(File.ReadAllLines(hostsPath));
                foreach (string url in r)
                {
                    string s = hostIP + " " + url;
                    if (!h.Contains(s))
                        h.Add(s);
                }
                File.WriteAllLines(hostsPath, h);
                Logger.Log("Redirection activated.", LogColor.Black);
            }
            catch (Exception ex)
            {
                Logger.Log("Activate Redirection Error: " + ME3Server.GetExceptionMessage(ex), LogColor.Red);
            }
        }

        private void deactivateRedirectionMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            DeactivateRedirection();
        }

        public static void DeactivateRedirection(bool bShowMsg = true)
        {
            try
            {
                List<string> r = new List<string>(File.ReadAllLines(loc + "conf" + Path.DirectorySeparatorChar + "redirect.txt"));
                string hostsPath = GetHostsFilePath();
                List<string> h = new List<string>(File.ReadAllLines(hostsPath));
                foreach (string url in r)
                {
                    for (int i = (h.Count - 1); i >= 0; i--)
                    {
                        if (h[i].EndsWith(url) && !h[i].StartsWith("#"))
                            h.RemoveAt(i);
                    }
                }
                File.WriteAllLines(hostsPath, h);
                if (bShowMsg)
                    Logger.Log("Redirection deactivated.", LogColor.Black);
            }
            catch (Exception ex)
            {
                if (bShowMsg)
                    Logger.Log("Deactivate Redirection Error: " + ME3Server.GetExceptionMessage(ex), LogColor.Red);
                else
                    System.Diagnostics.Debug.Print("DeactivateRedirection | " + ME3Server.GetExceptionMessage(ex));
            }
        }

        public static bool IsRedirectionActive()
        {
            try
            {
                int count = 0;
                List<string> r = new List<string>(File.ReadAllLines(loc + "conf" + Path.DirectorySeparatorChar + "redirect.txt"));
                string hostsPath = GetHostsFilePath();
                List<string> h = new List<string>(File.ReadAllLines(hostsPath));
                foreach (string url in r)
                {
                    for (int i = (h.Count - 1); i >= 0; i--)
                    {
                        if (h[i].EndsWith(url) && !h[i].StartsWith("#"))
                            count++;
                    }
                }
                return count > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print("IsRedirectionActive | Error:\n" + ME3Server.GetExceptionMessage(ex));
                return false;
            }
        }

        private static string GetHostsFilePath()
        {
            if (OperatingSystem.IsWindows())
                return sysdir + @"drivers" + Path.DirectorySeparatorChar + "etc" + Path.DirectorySeparatorChar + "hosts";
            else
                return "/etc/hosts";
        }

        private void showContentMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                string content = File.ReadAllText(GetHostsFilePath());
                Logger.Log("Hosts file content:\n" + content, LogColor.Black);
            }
            catch (Exception ex)
            {
                Logger.Log("Show Content Error: " + ME3Server.GetExceptionMessage(ex), LogColor.Red);
            }
        }

        private void packetEditorMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            GUI_PacketEditor p = new GUI_PacketEditor();
            p.Show();
        }

        private void showLogMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var tabControl = this.FindControl<TabControl>("tabControl");
            var logTab = this.FindControl<TabItem>("logTab");
            if (tabControl != null && logTab != null)
                tabControl.SelectedItem = logTab;
        }

        private void showPlayerListMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var tabControl = this.FindControl<TabControl>("tabControl");
            var playerTab = this.FindControl<TabItem>("playerTab");
            if (tabControl != null && playerTab != null)
                tabControl.SelectedItem = playerTab;
        }

        private void showGameListMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var tabControl = this.FindControl<TabControl>("tabControl");
            var gameListTab = this.FindControl<TabItem>("gameListTab");
            if (tabControl != null && gameListTab != null)
                tabControl.SelectedItem = gameListTab;
        }

        private void localProfileCreatorMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            GUI_ProfileCreator pc = new GUI_ProfileCreator();
            pc.Show();
        }

        private void deleteLogsMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string[] logs = Directory.GetFiles(loc + "logs" + Path.DirectorySeparatorChar);
            foreach (string log in logs)
                File.Delete(log);
            Logger.Log("Logs deleted.", LogColor.Black);
        }

        private void level0MenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Logger.LogLevel == 0) return;
            Logger.LogLevel = 0;
            Logger.Log("Log Level Changed to : " + Logger.LogLevel, LogColor.Black);
            UpdateLogLevelMenu();
        }

        private void level3MenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Logger.LogLevel == 3) return;
            Logger.LogLevel = 3;
            Logger.Log("Log Level Changed to : " + Logger.LogLevel, LogColor.Black);
            UpdateLogLevelMenu();
        }

        private void level5MenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Logger.LogLevel == 5) return;
            Logger.LogLevel = 5;
            Logger.Log("Log Level Changed to : " + Logger.LogLevel, LogColor.Black);
            UpdateLogLevelMenu();
        }

        private void recordPlayerSettingsMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ME3Server.bRecordPlayerSettings = !ME3Server.bRecordPlayerSettings;
            Logger.Log("MITM | Record player settings = " + ME3Server.bRecordPlayerSettings, LogColor.Black);
        }

        private async void importPlayerSettingsMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            bool activePlayer = false;
            foreach (Player.PlayerInfo p in Player.AllPlayers)
                activePlayer |= p.isActive;
            if (!activePlayer)
            {
                Logger.Log("You must be already connected through PSE before using Import player settings.", LogColor.Red);
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Import player settings",
                FileTypeFilter = new[] { new Avalonia.Platform.Storage.FilePickerFileType("Text files") { Patterns = new[] { "*.txt" } } },
                AllowMultiple = false
            });

            if (files.Count == 0) return;
            var file = files[0];

            try
            {
                using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var text = await reader.ReadToEndAsync();
                List<string> Lines = new List<string>(text.Split('\n').Select(l => l.TrimEnd('\r')));
                if (Lines.Count < 6)
                {
                    Logger.Log("[Import player settings] Invalid player file (number of lines)", LogColor.Red);
                    return;
                }
                Lines.RemoveRange(0, 5);
                List<string> keys = new List<string>();
                List<string> values = new List<string>();
                foreach (string line in Lines)
                {
                    string[] s = line.Split(Char.Parse("="));
                    if (s.Length != 2)
                    {
                        Logger.Log("[Import player settings] Invalid player file (line split)", LogColor.Red);
                        return;
                    }
                    keys.Add(s[0]);
                    values.Add(s[1]);
                }
                ME3Server.importKeys = keys;
                ME3Server.importValues = values;
            }
            catch (Exception ex)
            {
                Logger.Log("[Import player settings] " + ME3Server.GetExceptionMessage(ex), LogColor.Red);
            }
        }

        private void restartMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Cross-platform restart: start a new process and exit current
            var exePath = Environment.ProcessPath;
            if (exePath != null)
            {
                System.Diagnostics.Process.Start(exePath);
                Environment.Exit(0);
            }
        }

        private void activateMITMMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ME3Server.isMITM = !ME3Server.isMITM;
            Logger.Log("MITM mode = " + ME3Server.isMITM, LogColor.Black);
            UpdateMITMMenuState();
            ME3Server.bRecordPlayerSettings = false;
        }

        private void Frontend_Shown(object sender, EventArgs e)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            this.Title = "Syndicate 2012 Private Server Emulator by Kadrim, build: " + version;
            ME3Server.Start();
        }
    }
}
