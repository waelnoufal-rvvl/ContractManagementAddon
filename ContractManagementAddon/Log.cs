using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ContractManagementAddon
{
    internal static class Log
    {
        private static readonly object Sync = new object();
        private const long MaxBytes = 5_000_000; // ~5 MB

        public static string PathToLog => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? ".", "ContractManagementAddon.log");

        public static void StartupBanner()
        {
            try
            {
                WriteRaw($"===== ContractManagementAddon start {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} =====");
                Info($"BaseDir={AppDomain.CurrentDomain.BaseDirectory}");
                Info($"ProcessId={System.Diagnostics.Process.GetCurrentProcess().Id}; Is64Bit={Environment.Is64BitProcess}; OS={Environment.OSVersion}; CLR={Environment.Version}");
                try
                {
                    var entryAsm = Assembly.GetEntryAssembly() ?? typeof(Log).Assembly;
                    Info($"EntryAssembly={AssemblyInfoString(entryAsm)}");
                }
                catch { }
            }
            catch { }
        }

        public static void Info(string message) => Write("INFO", message);
        public static void Error(string message) => Write("ERROR", message);
        public static void Error(Exception ex, string contextMessage = null)
        {
            var msg = contextMessage == null ? ex.ToString() : contextMessage + Environment.NewLine + ex;
            Write("ERROR", msg);
            if (ex?.InnerException != null)
            {
                Write("ERROR", "InnerException: " + ex.InnerException);
            }
        }

        public static void DumpAssembly(Assembly asm, string label = null)
        {
            try
            {
                Info($"Asm{(label!=null?"["+label+"]":"")}: {AssemblyInfoString(asm)}");
            }
            catch { }
        }

        public static string AssemblyInfoString(Assembly asm)
        {
            if (asm == null) return "<null>";
            var name = asm.GetName();
            string fileVer = null;
            try
            {
                if (!string.IsNullOrEmpty(asm.Location) && File.Exists(asm.Location))
                {
                    var fvi = FileVersionInfo.GetVersionInfo(asm.Location);
                    fileVer = fvi.FileVersion;
                }
            }
            catch { }
            return $"{name.Name}, Version={name.Version}, FileVer={fileVer ?? "?"}, Location={asm.Location}";
        }

        private static void Write(string level, string message)
        {
            try
            {
                lock (Sync)
                {
                    EnsureLogSize();
                    File.AppendAllText(PathToLog, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}");
                }
            }
            catch
            {
                // Never throw from logger; keep add-on resilient
            }
        }

        private static void WriteRaw(string line)
        {
            try
            {
                lock (Sync)
                {
                    EnsureLogSize();
                    File.AppendAllText(PathToLog, line + Environment.NewLine);
                }
            }
            catch { }
        }

        private static void EnsureLogSize()
        {
            try
            {
                var path = PathToLog;
                var dir = System.IO.Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                if (File.Exists(path))
                {
                    var len = new FileInfo(path).Length;
                    if (len > MaxBytes)
                    {
                        var bak = path + ".1";
                        if (File.Exists(bak)) File.Delete(bak);
                        File.Move(path, bak);
                    }
                }
            }
            catch { }
        }
    }
}
