using System.Globalization;
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

    public static class StatsInfo
    {
        public static ConfigEntryWrapper<string> BackgroundColor;
        public static ConfigEntryWrapper<string> BackgroundImageFile;
        public static ConfigEntryWrapper<string> BackgroundImageArea;
        public static ConfigEntryWrapper<string> BackgroundImageBorder;

        public static class Text
        {
            public static ConfigEntryWrapper<string> Font;
            public static ConfigEntryWrapper<float> FontSize;
            public static ConfigEntryWrapper<string> FallbackFonts;
        }
    }

    public static class ExtraInfo
    {
        public static class Text
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

        TopMap.Sign.Font = config.Bind("TopMap.Sign", "Font", "arial.ttf", "");
        TopMap.Sign.FontSize = config.Bind("TopMap.Sign", "FontSize", 12.0f, "");
        TopMap.Sign.FallbackFonts = config.Bind("TopMap.Sign", "FallbackFonts", "seguisym.ttf", "");

        TopMap.Title.Font = config.Bind("TopMap.Title", "Font", "fonts/notosanssc-medium", "");
        TopMap.Title.FontSize = config.Bind("TopMap.Title", "FontSize", 12.0f, "");
        TopMap.Title.FallbackFonts = config.Bind("TopMap.Title", "FallbackFonts", "", "");

        TopMap.Count.Font = config.Bind("TopMap.Count", "Font", "arial.ttf", "");
        TopMap.Count.FontSize = config.Bind("TopMap.Count", "FontSize", 12.0f, "");
        TopMap.Count.FallbackFonts = config.Bind("TopMap.Count", "FallbackFonts", "", "");

        StatsInfo.BackgroundColor = config.Bind("StatsInfo", "BackgroundColor", "0,0,0,0", "");
        StatsInfo.BackgroundImageFile = config.Bind("StatsInfo", "BackgroundImageFile", "Background.png", "");
        StatsInfo.BackgroundImageArea = config.Bind("StatsInfo", "BackgroundImageArea", "17, 17, 94, 94", "");
        StatsInfo.BackgroundImageBorder = config.Bind("StatsInfo", "BackgroundImageBorder", "17, 17, 17, 17", "");

        StatsInfo.Text.Font = config.Bind("StatsInfo.Text", "Font", "fonts/notosanssc-medium", "");
        StatsInfo.Text.FontSize = config.Bind("StatsInfo.Text", "FontSize", 13.0f, "");
        StatsInfo.Text.FallbackFonts = config.Bind("StatsInfo.Text", "FallbackFonts", "", "");

        ExtraInfo.Text.Font = config.Bind("ExtraInfo.Text", "Font", "fonts/notosanssc-medium", "");
        ExtraInfo.Text.FontSize = config.Bind("ExtraInfo.Text", "FontSize", 13.0f, "");
        ExtraInfo.Text.FallbackFonts = config.Bind("ExtraInfo.Text", "FallbackFonts", "", "");
        LogDebug($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");

        _configFileWatcher.Set(config.ConfigFilePath, OnConfigFileChanged);
    }

    private static void OnConfigFileChanged(object source, FileSystemEventArgs e)
    {
        try
        {
            LogDebug($"OnConfigFileChanged: {e.Name}, {e.ChangeType}");
            ConfigFile.Reload();
        }
        catch (Exception exception)
        {
            LogError($"HResult: 0x{exception.HResult:X}, {exception.Message}");
        }
    }

    public static void LoadLanguageConfig(string languageCode)
    {
        var langFile = Path.Combine(BepInExDir, "config", "KingdomMod.OverlayMap", $"GuiStyle.{languageCode}.cfg");
        LogDebug($"GuiStyle language file: {langFile}");

        if (!File.Exists(langFile))
        {
            LogWarning($"GuiStyle language file do not exist: {langFile}");
            var lang = languageCode.Split('-')[0];
            var files = Directory.GetFiles(Path.Combine(BepInExDir, "config", "KingdomMod.OverlayMap"), $"GuiStyle.{lang}*.cfg");
            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    langFile = file;
                    LogWarning($"Try to use the sub language file: {langFile}");
                    break;
                }
            }
        }

        if (!File.Exists(langFile))
        {
            LogWarning($"GuiStyle language file do not exist: {langFile}");
            if (ConfigFile != null)
                return;
            languageCode = "en-US";
            langFile = Path.Combine(BepInExDir, "config", "KingdomMod.OverlayMap", $"GuiStyle.{languageCode}.cfg");
            LogWarning($"Try to use the default english language file: {langFile}");
        }

        if (ConfigFile != null)
        {
            if (Path.GetFileName(ConfigFile.ConfigFilePath) == Path.GetFileName(langFile))
            {
                LogDebug("Attempt to load the same configuration file. Skip.");
                return;
            }
        }

        LogDebug($"Try to bind GuiStyle language file: {Path.GetFileName(langFile)}");
        ConfigBind(new ConfigFile(langFile, true));
    }

}
