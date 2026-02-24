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
    public static ConfigEntryWrapper<int> GuiUpdatesPerSecond;

    public static void ConfigBind(ConfigFile config)
    {
        LogDebug($"ConfigBind: {Path.GetFileName(config.ConfigFilePath)}");

        ConfigPrefabs.Initialize();

        ConfigFile = config;
        config.SaveOnConfigSet = true;
        config.Clear();

        Language = config.Bind("Global", "Language", "system", "");
        MarkerStyleFile = config.Bind("Global", "MarkerStyleFile", "MarkerStyle.cfg", "");
        GuiUpdatesPerSecond = config.Bind("Global", "GuiUpdatesPerSecond", 10, "Increase to be more accurate, decrease to reduce performance impact");

        LogDebug($"ConfigFilePath: {config.ConfigFilePath}");
        LogDebug($"Language: {Language.Value}");
        LogDebug($"MarkerStyleFile: {MarkerStyleFile.Value}");
        LogDebug($"GuiUpdatesPerSecond: {GuiUpdatesPerSecond.Value}");

        LogDebug($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");

        OnLanguageChanged();
        OnMarkerStyleFileChanged();

        SetConfigDelegates();
        _configFileWatcher.Set(config.ConfigFilePath, OnConfigFileChanged);
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
    }

    public static void OnLanguageChanged()
    {
        LogDebug($"OnLanguageChanged: {Language.Entry.Value}");

        var lang = Language.Value;
        if (lang is "" or "system")
            lang = CultureInfo.CurrentCulture.Name;

        // Load language-specific configurations
        GuiStyle.LoadLanguageConfig(lang);
        Strings.LoadLanguageConfig(lang);
    }

    public static void OnMarkerStyleFileChanged()
    {
        LogDebug($"OnMarkerStyleFileChanged: {MarkerStyleFile.Value}");

        var styleFile = Path.Combine(BepInExDir, "config", "KingdomMod.OverlayMap", MarkerStyleFile.Value);
        LogDebug($"MarkerStyle file: {styleFile}");

        if (!File.Exists(styleFile))
        {
            LogWarning($"MarkerStyle file do not exist: {styleFile}");
            if (MarkerStyle.ConfigFile != null)
                return;
            styleFile = Path.Combine(BepInExDir, "config", "KingdomMod.OverlayMap", "MarkerStyle.cfg");
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
}
