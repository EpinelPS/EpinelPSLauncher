using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EpinelPSLauncher.Models;

namespace EpinelPSLauncher.Utils
{
    public class CoreInfo
    {
        public List<AccountEntry> Accounts { get; set; } = [];
        public string GamePath { get; set; } = "";
        public string? GameResourcePath { get; set; } = "";
        public string WinePath { get; set; } = "/usr/bin/wine";
        public string WineTricksPath { get; set; } = "/usr/bin/winetricks";
        public bool DisableAC { get; set; }
        public string? GameVersion { get; set; }

        public string GetFullResourcePath()
        {
            if (string.IsNullOrEmpty(GameResourcePath))
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "Unity");
            else return GameResourcePath;
        }
    }

    public class Configuration
    {
        public static CoreInfo Instance { get; internal set; }

        static Configuration()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/config.json"))
            {
                Instance = new();
                Save();
            }

            var j = JsonSerializer.Deserialize(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/config.json"), SourceGenerationContext.Default.CoreInfo);
            if (j != null)
            {
                Instance = j;
            }
            else
            {
                Instance = new();
            }
        }

        public static void Save()
        {
            if (Instance != null)
            {
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/config.json", JsonSerializer.Serialize(Instance, SourceGenerationContext.Default.CoreInfo));
            }
        }
    }
}
