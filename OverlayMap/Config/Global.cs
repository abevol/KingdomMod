using BepInEx.Configuration;
using System.Globalization;
using System.IO;
using System;
using KingdomMod.OverlayMap.Config.Extensions;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Config;

public class Global
{
    public static ConfigFile ConfigFile;
    private static readonly ConfigFileWatcher _configFileWatcher = new();
    public static ConfigEntryWrapper<string> Language;
    public static ConfigEntryWrapper<string> MarkerStyleFile;
    public static ConfigEntryWrapper<string> GuiStyleFile;
    public static ConfigEntryWrapper<int> GuiUpdatesPerSecond;

    public static void ConfigBind(ConfigFile config)
    {
        LogDebug($"ConfigBind: {Path.GetFileName(config.ConfigFilePath)}");

        ConfigPrefabs.Initialize();

        ConfigFile = config;
        config.SaveOnConfigSet = true;
        config.Clear();

        Language = config.Bind("Global", "Language", "system", "");
        MarkerStyleFile = config.Bind("Global", "MarkerStyleFile", "KingdomMod.OverlayMap.MarkerStyle.cfg", "");
        GuiStyleFile = config.Bind("Global", "GuiStyleFile", "KingdomMod.OverlayMap.GuiStyle.cfg", "");
        GuiUpdatesPerSecond = config.Bind("Global", "GuiUpdatesPerSecond", 10, "Increase to be more accurate, decrease to reduce performance impact");

        LogDebug($"ConfigFilePath: {config.ConfigFilePath}");
        LogDebug($"Language: {Language.Value}");
        LogDebug($"MarkerStyleFile: {MarkerStyleFile.Value}");
        LogDebug($"GuiStyleFile: {GuiStyleFile.Value}");
        LogDebug($"GuiUpdatesPerSecond: {GuiUpdatesPerSecond.Value}");

        LogDebug($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");

        OnLanguageChanged();
        OnMarkerStyleFileChanged();
        OnGuiStyleFileChanged();

        SetConfigDelegates();
        _configFileWatcher.Set(Path.GetFileName(config.ConfigFilePath), OnConfigFileChanged);
    }

    private static void OnConfigFileChanged(object source, FileSystemEventArgs e)
    {
        try
        {
            // LogDebug($"OnConfigFileChanged: {e.Name}, {e.ChangeType}");

            ConfigFile.Reload();
        }
        catch (Exception exception)
        {
            LogError($"HResult: {exception.HResult:X}, {exception.Message}");
        }
    }

    public static void SetConfigDelegates()
    {
        Language.Entry.SettingChanged += (sender, args) => OnLanguageChanged();
        MarkerStyleFile.Entry.SettingChanged += (sender, args) => OnMarkerStyleFileChanged();
        GuiStyleFile.Entry.SettingChanged += (sender, args) => OnGuiStyleFileChanged();
    }

    public static void OnLanguageChanged()
    {
        LogDebug($"OnLanguageChanged: {Language.Entry.Value}");

        var lang = Language.Value;
        if (lang is "" or "system")
            lang = CultureInfo.CurrentCulture.Name;

        var langFile = Path.Combine(BepInExDir, "config", $"KingdomMod.OverlayMap.Language.{lang}.cfg");
        LogDebug($"Language file: {langFile}");

        if (!File.Exists(langFile))
        {
            LogWarning($"Language file do not exist: {langFile}");
            lang = lang.Split('-')[0];
            var files = Directory.GetFiles(Path.Combine(BepInExDir, "config"), $"KingdomMod.OverlayMap.Language.{lang}*.cfg");
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
            LogWarning($"Language file do not exist: {langFile}");
            if (Strings.ConfigFile != null)
                return;
            lang = "en-US";
            langFile = Path.Combine(BepInExDir, "config", $"KingdomMod.OverlayMap.Language.{lang}.cfg");
            LogWarning($"Try to use the default english language file: {langFile}");
        }

        if (Strings.ConfigFile != null)
        {
            if (Path.GetFileName(Strings.ConfigFile.ConfigFilePath) == Path.GetFileName(langFile))
            {
                LogDebug("Attempt to load the same configuration file. Skip.");
                return;
            }
        }

        LogDebug($"Try to bind Language file: {Path.GetFileName(langFile)}");
        Strings.ConfigBind(new ConfigFile(langFile, true));
    }

    public static void OnMarkerStyleFileChanged()
    {
        LogDebug($"OnMarkerStyleFileChanged: {MarkerStyleFile.Value}");

        var styleFile = Path.Combine(BepInExDir, "config", MarkerStyleFile);
        LogDebug($"MarkerStyle file: {styleFile}");

        if (!File.Exists(styleFile))
        {
            LogWarning($"MarkerStyle file do not exist: {styleFile}");
            if (MarkerStyle.ConfigFile != null)
                return;
            styleFile = Path.Combine(BepInExDir, "config", "KingdomMod.OverlayMap.MarkerStyle.cfg");
            LogWarning($"Try to use the default MarkerStyle file: {styleFile}");
        }

        if (MarkerStyle.ConfigFile != null)
        {
            if (Path.GetFileName(MarkerStyle.ConfigFile.ConfigFilePath) == Path.GetFileName(styleFile))
            {
                LogDebug("Attempt to load the same configuration file. Skip.");
                return;
            }
        }

        LogDebug($"Try to bind MarkerStyle file: {Path.GetFileName(styleFile)}");
        MarkerStyle.ConfigBind(new ConfigFile(styleFile, true));
    }

    public static void OnGuiStyleFileChanged()
    {
        LogDebug($"OnGuiStyleFileChanged: {GuiStyleFile.Value}");

        var styleFile = Path.Combine(BepInExDir, "config", GuiStyleFile);
        LogDebug($"GuiStyle file: {styleFile}");

        if (!File.Exists(styleFile))
        {
            LogWarning($"GuiStyle file do not exist: {styleFile}");
            if (GuiStyle.ConfigFile != null)
                return;
            styleFile = Path.Combine(BepInExDir, "config", "KingdomMod.OverlayMap.GuiStyle.cfg");
            LogWarning($"Try to use the default GuiStyle file: {styleFile}");
        }

        if (GuiStyle.ConfigFile != null)
        {
            if (Path.GetFileName(GuiStyle.ConfigFile.ConfigFilePath) == Path.GetFileName(styleFile))
            {
                LogDebug("Attempt to load the same configuration file. Skip.");
                return;
            }
        }

        LogDebug($"Try to bind GuiStyle file: {Path.GetFileName(styleFile)}");
        GuiStyle.ConfigBind(new ConfigFile(styleFile, true));
    }
}