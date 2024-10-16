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

public class ExploredRegion
{
    private float _exploredLeft;
    private float _exploredRight;

    public float ExploredLeft
    {
        get { return _exploredLeft; }
        set
        {
            _exploredLeft = value;
            SetExploredLeft(value);
        }
    }

    public float ExploredRight
    {
        get { return _exploredRight; }
        set
        {
            _exploredRight = value;
            SetExploredRight(value);
        }
    }

    private static void SetExploredLeft(float value)
    {
        ExploredRegions.ExploredLeft.Value = value;
        ExploredRegions.Time.Value = Managers.Inst.director.currentTime;
        ExploredRegions.Days.Value = Managers.Inst.director.CurrentDayForSpawning;
    }

    private static void SetExploredRight(float value)
    {
        ExploredRegions.ExploredRight.Value = value;
        ExploredRegions.Time.Value = Managers.Inst.director.currentTime;
        ExploredRegions.Days.Value = Managers.Inst.director.CurrentDayForSpawning;
    }

    private static bool HasAvailableConfig()
    {
        if (ExploredRegions.ExploredLeft == 0 && ExploredRegions.ExploredRight == 0)
            return false;

        if (ExploredRegions.Days > Managers.Inst.director.CurrentDayForSpawning)
            return false;

        if (ExploredRegions.Days == Managers.Inst.director.CurrentDayForSpawning)
            if (ExploredRegions.Time > Managers.Inst.director.currentTime)
                return false;

        return true;
    }

    public void Init(Player player, string archiveFilename)
    {
        ExploredRegions.ConfigBind(archiveFilename);
        if (HasAvailableConfig())
        {
            _exploredLeft = ExploredRegions.ExploredLeft;
            _exploredRight = ExploredRegions.ExploredRight;
        }
        else
        {
            _exploredLeft = player.transform.localPosition.x;
            _exploredRight = player.transform.localPosition.x;
        }
    }
}