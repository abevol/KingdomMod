using BepInEx.Configuration;
using System.IO;
using System;
using KingdomMod.OverlayMap.Config.Extensions;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Config;

public class GuiStyle
{
    public static ConfigFile ConfigFile;
    private static readonly ConfigFileWatcher _configFileWatcher = new();

    public static ConfigEntryWrapper<string> BackgroundColor;
    public static ConfigEntryWrapper<string> BackgroundImageFile;
    public static ConfigEntryWrapper<string> BackgroundImageArea;
    public static ConfigEntryWrapper<string> BackgroundImageBorder;

    public static void ConfigBind(ConfigFile config)
    {
        LogMessage($"ConfigBind: {Path.GetFileName(config.ConfigFilePath)}");

        ConfigFile = config;
        config.SaveOnConfigSet = false;
        config.Clear();

        BackgroundColor = config.Bind("GuiStyle", "BackgroundColor", "0,0,0,0", "");
        BackgroundImageFile = config.Bind("GuiStyle", "BackgroundImageFile", "Background.png", "");
        BackgroundImageArea = config.Bind("GuiStyle", "BackgroundImageArea", "17, 17, 94, 94", "");
        BackgroundImageBorder = config.Bind("GuiStyle", "BackgroundImageBorder", "17, 17, 17, 17", "");

        LogMessage($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");

        _configFileWatcher.Set(Path.GetFileName(config.ConfigFilePath), OnConfigFileChanged);
    }

    private static void OnConfigFileChanged(object source, FileSystemEventArgs e)
    {
        try
        {
            LogMessage($"OnConfigFileChanged: {e.Name}, {e.ChangeType}");
            ConfigFile.Reload();
            OverlayMapHolder.Instance.NeedToReloadGuiBoxStyle = true;
        }
        catch (Exception exception)
        {
            LogMessage($"HResult: {exception.HResult:X}, {exception.Message}");
        }
    }
}