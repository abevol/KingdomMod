using BepInEx.Configuration;
using System.IO;
using KingdomMod.OverlayMap.Config.Extensions;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Config;

public class SaveDataExtras
{
    public static ConfigFile ConfigFile;

    public static ConfigEntryWrapper<float> MapOffset;
    public static ConfigEntryWrapper<float> ZoomScale;
    public static ConfigEntryWrapper<float> ExploredLeft;
    public static ConfigEntryWrapper<float> ExploredRight;
    public static ConfigEntryWrapper<float> Time;
    public static ConfigEntryWrapper<int> Days;

    public static void ConfigInit()
    {
        if (ConfigFile != null)
            return;

        var configFilePath = Path.Combine(BepInExDir, "config", "KingdomMod.OverlayMap.SaveDataExtras.cfg");
        LogDebug($"SaveDataExtras file: {configFilePath}");

        ConfigFile = new ConfigFile(configFilePath, true);
        ConfigFile.SaveOnConfigSet = false;
        ConfigFile.Clear();
    }

    public static void ConfigBind(string archiveFilename)
    {
        MapOffset = ConfigFile.Bind(archiveFilename, "MapOffset", 0f);
        ZoomScale = ConfigFile.Bind(archiveFilename, "ZoomScale", 0f);
        ExploredLeft = ConfigFile.Bind(archiveFilename, "ExploredLeft", 0f);
        ExploredRight = ConfigFile.Bind(archiveFilename, "ExploredRight", 0f);
        Time = ConfigFile.Bind(archiveFilename, "Time", 0f);
        Days = ConfigFile.Bind(archiveFilename, "Days", 0);

        LogDebug($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");
    }

    public static void Save()
    {
        if (Managers.Inst == null)
            return;
        if (Managers.Inst.director == null)
            return;
        if (Time == null || Days == null)
            return;

        Time.Value = Managers.Inst.director.currentTime;
        Days.Value = Managers.Inst.director.CurrentDayForSpawning;
        ConfigFile.Save();

        LogDebug("ConfigFile Saved.");
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
            SaveDataExtras.ExploredLeft.Value = value;
        }
    }

    public float ExploredRight
    {
        get { return _exploredRight; }
        set
        {
            _exploredRight = value;
            SaveDataExtras.ExploredRight.Value = value;
        }
    }

    private static bool HasAvailableConfig()
    {
        if (SaveDataExtras.ExploredLeft == 0 && SaveDataExtras.ExploredRight == 0)
            return false;

        if (SaveDataExtras.Days > Managers.Inst.director.CurrentDayForSpawning)
            return false;

        if (SaveDataExtras.Days == Managers.Inst.director.CurrentDayForSpawning)
            if (SaveDataExtras.Time > Managers.Inst.director.currentTime)
                return false;

        return true;
    }

    public string GetArchiveFilename()
    {
        var campaignIndex = GlobalSaveData.loaded.currentCampaign;
        var land = CampaignSaveData.current != null ? CampaignSaveData.current.CurrentLand : (-1);
        var challengeId = GlobalSaveData.loaded.currentChallenge;
        var archiveFilename = IslandSaveData.GetFilePropsForLand(campaignIndex, land, challengeId).filename;

        LogDebug($"OnGameStart: _archiveFilename {archiveFilename}, Campaign {campaignIndex}, CurrentLand {land}, currentChallenge {challengeId}");

        return archiveFilename;
    }

    public void Init()
    {
        SaveDataExtras.ConfigInit();
        SaveDataExtras.Save();
        SaveDataExtras.ConfigBind(GetArchiveFilename());
        if (HasAvailableConfig())
        {
            _exploredLeft = SaveDataExtras.ExploredLeft;
            _exploredRight = SaveDataExtras.ExploredRight;
        }
        else
        {
            var player = GetLocalPlayer();
            _exploredLeft = player.transform.localPosition.x;
            _exploredRight = player.transform.localPosition.x;
        }
    }
}