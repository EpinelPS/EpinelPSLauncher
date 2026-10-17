//#define GameAssemblyNeedsPatch // remove if running on versions before v124 or on v137+
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using EpinelPSLauncher.Utils;

namespace EpinelPSLauncher.Utils;

public class ServerSwitcher
{
    private const string HostsStartMarker = "# begin ServerSelector entries";
    private const string HostsEndMarker = "# end ServerSelector entries";

    private static PathUtil util = new();

    public static bool IsUsingLocalServer()
    {
        return File.ReadAllText(util.SystemHostsFile).Contains("global-lobby.nikke-kr.com");
    }

    public static bool IsOffline()
    {
        return File.ReadAllText(util.SystemHostsFile ?? "").Contains("cloud.nikke-kr.com");
    }

    public static (bool, string?) SetBasePath(string basePath)
    {
        return util.SetBasePath(basePath);
    }

    public static async Task<string> CheckIntegrity()
    {
        if (!IsUsingLocalServer())
            return "Official server";

        if (File.Exists(util.LauncherCertificatePath))
        {
            string certList1 = await File.ReadAllTextAsync(util.LauncherCertificatePath);

            if (!certList1.Contains("Good SSL Ca"))
                return "SSL Cert Patch missing Launcher";
        }

        if (File.Exists(util.GameCertificatePath))
        {
            string certList2 = await File.ReadAllTextAsync(util.GameCertificatePath);

            if (!certList2.Contains("Good SSL Ca"))
                return "SSL Cert Patch missing Game";
        }

        // TODO: Check sodium lib
        // TODO: check hosts file

        return "OK";
    }

    private static async Task WriteHostsFileAsync(string filePath, string content)
    {
        if (OperatingSystem.IsLinux() && filePath == "/etc/hosts")
        {
            string tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, content);

            using var x = new Process();
            x.StartInfo.FileName = "pkexec";
            x.StartInfo.Arguments = $"cp \"{tempFile}\" \"{filePath}\"";
            
            x.Start();
            await x.WaitForExitAsync(); 

            File.Delete(tempFile);

            if (x.ExitCode != 0)
            {
                throw new Exception($"Command to edit {filePath} failed with exit code {x.ExitCode}");
            }
        }
        else
        {
            FileInfo fi = new(filePath);
            if (fi.Exists && fi.IsReadOnly)
            {
                fi.IsReadOnly = false;
            }
            await File.WriteAllTextAsync(filePath, content);
        }
    }

    private static string CleanHostsText(string txt)
    {
        int startIdx = txt.IndexOf(HostsStartMarker);
        int endIdx = -1;

        if (startIdx != -1)
        {
            if (startIdx >= 2 && txt[startIdx - 1] == '\n' && txt[startIdx - 2] == '\r')
                startIdx -= 2;
            else if (startIdx >= 1 && txt[startIdx - 1] == '\n')
                startIdx -= 1;

            endIdx = txt.IndexOf(HostsEndMarker, startIdx);
            if (endIdx != -1)
            {
                endIdx += HostsEndMarker.Length;
            }
        }
        else
        {
            // Fallback for legacy format
            int legacyStart = txt.IndexOf("cloud.nikke-kr.com");
            if (legacyStart != -1)
            {
                startIdx = txt.LastIndexOf('\n', legacyStart);
                if (startIdx == -1) startIdx = 0;

                int legacyEnd = txt.IndexOf("y.io", legacyStart);
                if (legacyEnd != -1)
                {
                    endIdx = legacyEnd + "y.io".Length;
                }
            }
        }

        if (startIdx != -1 && endIdx != -1 && endIdx > startIdx)
        {
            return string.Concat(txt.AsSpan(0, startIdx), txt.AsSpan(endIdx));
        }

        return txt;
    }

    public static async Task RevertHostsFile(string hostsFilePath)
    {
        if (!File.Exists(hostsFilePath)) return;

        try
        {
            string currentText = await File.ReadAllTextAsync(hostsFilePath);
            string cleanedText = CleanHostsText(currentText);

            // Only write if necessary
            if (currentText != cleanedText)
            {
                await WriteHostsFileAsync(hostsFilePath, cleanedText);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to revert hosts file '{hostsFilePath}': {ex.Message}");
        }
    }

    private static async Task UpdateHostsFileIfNeeded(string filePath, string newBlock)
    {
        if (!File.Exists(filePath)) return;

        try
        {
            string currentText = await File.ReadAllTextAsync(filePath);
            
            // Normalize line endings to avoid false-positives between Windows (\r\n) and Linux (\n)
            string normalizedCurrent = currentText.Replace("\r\n", "\n");
            string normalizedBlock = newBlock.Replace("\r\n", "\n").Trim();

            if (normalizedCurrent.Contains(normalizedBlock))
            {
                // The exact configuration (IP and offline mode) is already present.
                return; 
            }

            // Remove any old/different configuration block
            string cleanedText = CleanHostsText(currentText);

            if (!cleanedText.EndsWith("\n") && cleanedText.Length > 0)
            {
                cleanedText += Environment.NewLine;
            }

            string newText = cleanedText + newBlock;
            await WriteHostsFileAsync(filePath, newText);
        }
        catch (Exception ex)
        {
            throw new Exception($"Cannot modify \"{filePath}\" file to redirect to server, check your antivirus software", ex);
        }
    }

    private static async Task ModifyHosts(bool offlineMode, string ip)
    {
        string hosts = $@"
{HostsStartMarker}
{ip} global-lobby.nikke-kr.com
";
        if (offlineMode)
        {
            hosts += $"{ip} cloud.nikke-kr.com" + Environment.NewLine;
        }

        hosts += $@"{ip} jp-lobby.nikke-kr.com
{ip} us-lobby.nikke-kr.com
{ip} kr-lobby.nikke-kr.com
{ip} sea-lobby.nikke-kr.com
{ip} hmt-lobby.nikke-kr.com
{ip} aws-na-dr.intlgame.com
0.0.0.0 o428077.ingest.sentry.io
0.0.0.0 pc.crashsight.wetest.net
{ip} sg-vas.intlgame.com
{ip} aws-na.intlgame.com
{ip} na-community.playerinfinite.com
{ip} common-web.intlgame.com
{ip} li-sg.intlgame.com
255.255.221.21 na.fleetlogd.com
{ip} www.jupiterlauncher.com
{ip} data-aws-na.intlgame.com
255.255.221.21 sentry.io
{HostsEndMarker}";

        // Safely evaluate and update the system hosts file
        await UpdateHostsFileIfNeeded(util.SystemHostsFile, hosts);

        // Safely evaluate and update the Wine hosts file if necessary
        if (OperatingSystem.IsLinux() && !string.IsNullOrEmpty(util.WineHostsFile))
        {
            await UpdateHostsFileIfNeeded(util.WineHostsFile, hosts);
        }
    }

    public static async Task<ServerSwitchResult> SaveCfg(bool useOffical, string ip, bool offlineMode)
    {
        string CAcert = await File.ReadAllTextAsync(AppDomain.CurrentDomain.BaseDirectory + "myCA.pem");
        string sodiumLib = AppDomain.CurrentDomain.BaseDirectory + "sodium.dll";

        bool supported = true;
        if (useOffical)
        {
            await RevertHostsFile(util.SystemHostsFile);
            if (OperatingSystem.IsLinux() && util.WineHostsFile != null)
            {
                await RevertHostsFile(util.WineHostsFile);
            }

            try
            {
                // remove cert
                if (OperatingSystem.IsWindows())
                {
                    X509Store store = new(StoreName.Root, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadWrite);
                    store.Remove(new X509Certificate2(X509Certificate.CreateFromCertFile(AppDomain.CurrentDomain.BaseDirectory + "myCA.pfx")));
                    store.Close();
                }
            }
            catch
            {
                // may not be installed
            }

            // restore sodium
            if (!File.Exists(util.GameSodiumBackupPath))
            {
                throw new Exception("sodium backup does not exist. Repair the game in the launcher and switch to local server and back to official.");
            }
            File.Copy(util.GameSodiumBackupPath, util.GameSodiumPath, true);

            if (util.LauncherCertificatePath != null && File.Exists(util.LauncherCertificatePath))
            {
                string certList = await File.ReadAllTextAsync(util.LauncherCertificatePath);

                int goodSslIndex1 = certList.IndexOf("Good SSL Ca");
                if (goodSslIndex1 != -1)
                    await File.WriteAllTextAsync(util.LauncherCertificatePath, certList[..goodSslIndex1]);
            }

            if (File.Exists(util.GameCertificatePath))
            {
                string certList = await File.ReadAllTextAsync(util.GameCertificatePath);

                int newCertIndex = certList.IndexOf("Good SSL Ca");
                if (newCertIndex != -1)
                    await File.WriteAllTextAsync(util.GameCertificatePath, certList[..newCertIndex]);
            }
        }
        else
        {
            await ModifyHosts(offlineMode, ip);

            // trust CA. TODO is this needed?
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    X509Store store = new(StoreName.Root, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(new X509Certificate2(X509Certificate.CreateFromCertFile(AppDomain.CurrentDomain.BaseDirectory + "myCA.pfx")));
                    store.Close();
                }
            }
            catch { }

            if (!File.Exists(util.GameSodiumPath))
            {
                throw new Exception("expected sodium library to exist at path " + util.GameSodiumPath);
            }

            // copy backup if sodium size is correct
            byte[] sod = await File.ReadAllBytesAsync(util.GameSodiumPath);
            if (sod.Length <= 307200) // TODO this is awful
            {
                // orignal file size, copy backup
                await File.WriteAllBytesAsync(util.GameSodiumBackupPath, sod);
            }

            // write new sodium library
            await File.WriteAllBytesAsync(util.GameSodiumPath, await File.ReadAllBytesAsync(sodiumLib));

            // Add generated CA certificate to launcher/game curl certificate list
            if (util.LauncherCertificatePath != null && File.Exists(util.LauncherCertificatePath))
            {
                await File.WriteAllTextAsync(util.LauncherCertificatePath,
                    await File.ReadAllTextAsync(util.LauncherCertificatePath)
                    + "\nGood SSL Ca\n===============================\n"
                    + CAcert);
            }

            if (util.GameCertificatePath != null && File.Exists(util.GameCertificatePath))
            {
                await File.WriteAllTextAsync(util.GameCertificatePath,
                await File.ReadAllTextAsync(util.GameCertificatePath)
                + "\nGood SSL Ca\n===============================\n"
                + CAcert);
            }
        }

        return new ServerSwitchResult(true, null, supported);
    }
}

public class ServerSwitchResult(bool ok, Exception? exception, bool isSupported)
{
    public bool Ok { get; set; } = ok;
    public Exception? Exception { get; set; } = exception;
    public bool IsSupported { get; set; } = isSupported;
}
