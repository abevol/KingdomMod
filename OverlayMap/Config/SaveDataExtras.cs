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


    public static void Init()
    {
        ConfigInit();
        Save();
        ConfigBind(GetArchiveFilename());

        if (!HasAvailableConfig())
        {
            var player = GetLocalPlayer();
            ExploredLeft.Value = player.transform.localPosition.x;
            ExploredRight.Value = player.transform.localPosition.x;
        }

        if (ZoomScale <= 0)
            ZoomScale.Value = 1.0f;
    }

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
        ZoomScale = ConfigFile.Bind(archiveFilename, "ZoomScale", 1.0f);
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

    private static bool HasAvailableConfig()
    {
        if (ExploredLeft == 0 && ExploredRight == 0)
            return false;

        if (Days > Managers.Inst.director.CurrentDayForSpawning)
            return false;

        if (Days == Managers.Inst.director.CurrentDayForSpawning)
            if (Time > Managers.Inst.director.currentTime)
                return false;

        return true;
    }

    public static string GetArchiveFilename()
    {
        var campaignIndex = GlobalSaveData.loaded.currentCampaign;
        var land = CampaignSaveData.current != null ? CampaignSaveData.current.CurrentLand : (-1);
        var challengeId = GlobalSaveData.loaded.currentChallenge;
        var archiveFilename = IslandSaveData.GetFilePropsForLand(campaignIndex, land, challengeId).filename;

        LogInfo($"ArchiveFilename {archiveFilename}, Campaign {campaignIndex}, CurrentLand {land}, currentChallenge {challengeId}");

        return archiveFilename;
    }
}
