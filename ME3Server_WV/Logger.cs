using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;

namespace ME3Server_WV
{
    public class LogEntry
    {
        public string Message { get; set; }
        public LogColor Color { get; set; }

        public LogEntry(string message, LogColor color)
        {
            Message = message;
            Color = color;
        }
    }

    public static class Logger
    {
        private static object _sync = new object();
        private static string PacketLogFile = "PacketLog";
        private static string loc = AppContext.BaseDirectory;
        public static string mainlogpath = loc + "logs" + Path.DirectorySeparatorChar + "MainServerLog.txt";
        public static ObservableCollection<LogEntry> LogEntries { get; } = new();
        public static int LogLevel = 0;

        public static void Log(string msg, LogColor c, int Level = 0)
        {
            lock (_sync)
            {
                try
                {
                    string s = string.Format(@"{0:yyyy.MM.dd HH:mm:ss}", DateTime.Now) + " " + msg;
                    if (!Directory.Exists(Path.GetDirectoryName(mainlogpath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(mainlogpath));
                    if (!File.Exists(mainlogpath))
                        File.WriteAllBytes(mainlogpath, new byte[0]);
                    File.AppendAllText(mainlogpath, s.Replace("\n", "\r\n") + "\r\n");
                    if (Level > LogLevel)
                        return;
                    var entry = new LogEntry(s, c);
                    try
                    {
                        DispatcherHelper.RunOnUI(() => LogEntries.Add(entry));
                    }
                    catch (Exception)
                    {
                        // UI not available yet
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public struct DumpStruct
        {
            public byte[] buff;
            public Player.PlayerInfo player;
        }

        public static void DumpPacket(byte[] buff, Player.PlayerInfo player)
        {
            Thread t = new Thread(ThreadPacketDump);
            DumpStruct d = new DumpStruct();
            d.buff = buff;
            d.player = player;
            t.Start(d);
        }

        public static void DeleteLogs()
        {            
            string[] files = Directory.GetFiles(loc + "logs" + Path.DirectorySeparatorChar);
            if (files.Length != 0)
            {
                if (files.Length == 1 && files[0].Contains("MainServerLog.txt"))
                    return;
                string autodelete = Config.FindEntry("AutoDeleteLogs");
                if (autodelete == "1")
                {
                    Log("Found Logs, deleting...", LogColor.Red);
                    foreach (string file in files)
                        if (!file.Contains("MainServerLog.txt"))
                        {
                            Log("Deleting : " + Path.GetFileName(file) + " ...", LogColor.Red);
                            File.Delete(file);
                        }                    
                }
            }            
        }

        public static void ThreadPacketDump(object objs)
        {
            DumpStruct d = (DumpStruct)objs;
            byte[] buff = d.buff;
            lock (_sync)
            {
                FileStream fs = new FileStream(loc + "logs" + Path.DirectorySeparatorChar + PacketLogFile + "_" + d.player.timestring + "_" + d.player.ID.ToString("00") + ".bin", FileMode.Append, FileAccess.Write);
                fs.Write(buff, 0, buff.Length);
                fs.Close();
            }
        }
    }
}
