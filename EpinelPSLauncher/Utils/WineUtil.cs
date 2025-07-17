using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPSLauncher.Utils
{
    public class WineUtil
    {
        public static string PrefixPath
        {
            get
            {
                return Path.Combine(Configuration.Instance.GamePath, "wine_prefix");
            }
        }
        public static bool PrefixExists
        {
            get
            {
                return Directory.Exists(PrefixPath) && File.Exists(PrefixPath + "/.configured");
            }
        }
        public static async Task CreatePrefix()
        {
            if (!File.Exists(Configuration.Instance.WineTricksPath))
                throw new Exception(Configuration.Instance.WineTricksPath + " is required");

            Directory.CreateDirectory(PrefixPath);

            await RunWinetricks(PrefixPath, "corefonts");

            File.WriteAllText(PrefixPath + "/.configured", "");
        }

        public static async Task RunWinetricks(string prefixPath, string args)
        {
            ProcessStartInfo info = new(Configuration.Instance.WineTricksPath, args);
            info.Environment.Add("WINEPREFIX", prefixPath);

            Process proc = Process.Start(info) ?? throw new Exception("failed to start winetricks process");
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0)
            {
                throw new Exception($"Exectuting \"{Configuration.Instance.WineTricksPath} {args}\" failed with error code {proc.ExitCode}");
            }
        }
    }
}