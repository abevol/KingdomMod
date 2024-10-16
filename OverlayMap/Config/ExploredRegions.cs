using BepInEx.Configuration;
using System.IO;
using KingdomMod.OverlayMap.Config.Extensions;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Config;

public class ExploredRegions
{
    public static ConfigFile ConfigFile;

    public static ConfigEntryWrapper<float> ExploredLeft;
    public static ConfigEntryWrapper<float> ExploredRight;
    public static ConfigEntryWrapper<float> Time;
    public static ConfigEntryWrapper<int> Days;

    public static void ConfigBind(string archiveFilename)
    {
        var bepInExDir = GetBepInExDir();
        var configFilePath = Path.Combine(bepInExDir, "config", "KingdomMod.OverlayMap.ExploredRegions.cfg");
        LogMessage($"ExploredRegions file: {configFilePath}");

        ConfigFile = new ConfigFile(configFilePath, true);
        ConfigFile.SaveOnConfigSet = true;
        ConfigFile.Clear();

        ExploredLeft = ConfigFile.Bind(archiveFilename, "ExploredLeft", 0f);
        ExploredRight = ConfigFile.Bind(archiveFilename, "ExploredRight", 0f);
        Time = ConfigFile.Bind(archiveFilename, "Time", 0f);
        Days = ConfigFile.Bind(archiveFilename, "Days", 0);

        LogMessage($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");
    }
}