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

    public static class PlayerOverlay
    {
    }

    public static class TopMap
    {
        public static ConfigEntryWrapper<string> BackgroundColor;
        public static ConfigEntryWrapper<string> BackgroundImageFile;
        public static ConfigEntryWrapper<string> BackgroundImageArea;
        public static ConfigEntryWrapper<string> BackgroundImageBorder;

        public static class Sign
        {
            public static ConfigEntryWrapper<string> Font;
            public static ConfigEntryWrapper<float> FontSize;
            public static ConfigEntryWrapper<string> FallbackFonts;
        }

        public static class Title
        {
            public static ConfigEntryWrapper<string> Font;
            public static ConfigEntryWrapper<float> FontSize;
            public static ConfigEntryWrapper<string> FallbackFonts;
        }

        public static class Count
        {
            public static ConfigEntryWrapper<string> Font;
            public static ConfigEntryWrapper<float> FontSize;
            public static ConfigEntryWrapper<string> FallbackFonts;
        }
    }

    public static void ConfigBind(ConfigFile config)
    {
        LogDebug($"ConfigBind: {Path.GetFileName(config.ConfigFilePath)}");

        ConfigFile = config;
        config.SaveOnConfigSet = false;
        config.Clear();

        TopMap.BackgroundColor = config.Bind("TopMap", "BackgroundColor", "0,0,0,0", "");
        TopMap.BackgroundImageFile = config.Bind("TopMap", "BackgroundImageFile", "Background.png", "");
        TopMap.BackgroundImageArea = config.Bind("TopMap", "BackgroundImageArea", "17, 17, 94, 94", "");
        TopMap.BackgroundImageBorder = config.Bind("TopMap", "BackgroundImageBorder", "17, 17, 17, 17", "");

        TopMap.Sign.Font = config.Bind("TopMap.Sign", "Font", "msgothic.ttc", "");
        TopMap.Sign.FontSize = config.Bind("TopMap.Sign", "FontSize", 12.0f, "");
        TopMap.Sign.FallbackFonts = config.Bind("TopMap.Sign", "FallbackFonts", "", "");

        TopMap.Title.Font = config.Bind("TopMap.Title", "Font", "msyh.ttc", "");
        TopMap.Title.FontSize = config.Bind("TopMap.Title", "FontSize", 12.0f, "");
        TopMap.Title.FallbackFonts = config.Bind("TopMap.Title", "FallbackFonts", "", "");

        TopMap.Count.Font = config.Bind("TopMap.Count", "Font", "arial.ttf", "");
        TopMap.Count.FontSize = config.Bind("TopMap.Count", "FontSize", 12.0f, "");
        TopMap.Count.FallbackFonts = config.Bind("TopMap.Count", "FallbackFonts", "", "");

        LogDebug($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");

        _configFileWatcher.Set(Path.GetFileName(config.ConfigFilePath), OnConfigFileChanged);
    }

    private static void OnConfigFileChanged(object source, FileSystemEventArgs e)
    {
        try
        {
            LogDebug($"OnConfigFileChanged: {e.Name}, {e.ChangeType}");
            ConfigFile.Reload();
            OverlayMapHolder.Instance.NeedToReloadGuiBoxStyle = true;
        }
        catch (Exception exception)
        {
            LogError($"HResult: {exception.HResult:X}, {exception.Message}");
        }
    }
}