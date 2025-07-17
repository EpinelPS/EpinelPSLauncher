using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPSLauncher.Utils
{
    public class LaunchGame
    {
        public static async Task<Process?> Launch(string gameExe, string resourcePath, string loginData)
        {
            string processName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LauncherUtility.exe");
            string args = $"\"{gameExe.Replace("\\", "\\\\")}\" \"{resourcePath.Replace("\\", "\\\\")}\" {Convert.ToBase64String(Encoding.ASCII.GetBytes(loginData))}";

            if (!File.Exists(processName))
                throw new Exception("LauncherUtility.exe does not exist. Launcher was not compiled correctly.");
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sail_api_impl64.dll")))
                throw new Exception("sail_api_impl64.dll does not exist. Launcher was not compiled correctly.");

            var startInfo = new ProcessStartInfo(processName, args)
            {
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (!OperatingSystem.IsWindows())
            {
                if (!WineUtil.PrefixExists)
                {
                    await WineUtil.CreatePrefix();
                }
                if (!File.Exists(Configuration.Instance.WinePath)) throw new Exception($"Wine executable is missing ({Configuration.Instance.WinePath})");

                startInfo.Arguments = $"\"{processName}\" {args}";
                startInfo.FileName = Configuration.Instance.WinePath;
                startInfo.EnvironmentVariables.Add("WINEPREFIX", WineUtil.PrefixPath);
            }

            Console.WriteLine($"Executing: {startInfo.FileName} with args {startInfo.Arguments}");

            Process? proc = Process.Start(startInfo) ?? throw new Exception("Process.Start failed");
            proc.EnableRaisingEvents = true;

            return proc;
        }
    }
}
