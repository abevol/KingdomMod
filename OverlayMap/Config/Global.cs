using BepInEx.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
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

        var langCodes = Directory.GetFiles(Path.Combine(BepInExDir, "config", "KingdomMod.OverlayMap"), "Language.*.cfg")
            .Select(f => Path.GetFileNameWithoutExtension(f)!.Substring("Language.".Length))
            .Where(c => !string.IsNullOrEmpty(c))
            .ToArray();
        Language = config.Bind("Global", "Language", "system", new ConfigDescription("Language to use", new AcceptableValueList<string>(new[] { "system" }.Concat(langCodes).ToArray())));
        MarkerStyleFile = config.Bind("Global", "MarkerStyleFile", "MarkerStyle.cfg", "");
        GuiUpdatesPerSecond = config.Bind("Global", "GuiUpdatesPerSecond", 10, new ConfigDescription("Increase to be more accurate, decrease to reduce performance impact", new AcceptableValueRange<int>(1, 60)));

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
            LogError($"HResult: 0x{exception.HResult:X}, {exception.Message}");
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

        // Step 1: 取消旧配置项的事件订阅（在配置替换之前）
        Instance?.GlobalGuiStyle?.UnsubscribeConfigEvents();

        // Step 2: 加载新语言配置（替换静态 ConfigEntry 字段）
        GuiStyle.LoadLanguageConfig(lang);
        Strings.LoadLanguageConfig(lang);

        // Step 3: 重新加载样式配置（重建字体、订阅新事件、推送到 UI）
        Instance?.GlobalGuiStyle?.ReloadConfig();

        // Step 4: 刷新所有 MapMarker 的本地化引用（Title / Sign / Color）
        RefreshMapMarkerTitles();
    }

    /// <summary>
    /// 刷新所有 MapMarker 的 Title 引用。
    /// 语言切换后，Strings 的静态字段已指向新的 ConfigEntryWrapper，
    /// 但 MapMarkerData.Title 仍持有旧引用。通过 config key 查找新 wrapper 并重新赋值。
    /// Title setter 会自动处理旧事件的取消订阅和新事件的订阅。
    /// </summary>
    private static void RefreshMapMarkerTitles()
    {
        if (Instance == null) return;

        ForEachTopMapView(view =>
        {
            foreach (var pair in view.MapMarkers)
            {
                var data = pair.Value.Data;
                if (data == null) continue;

                // 刷新 Title
                if (data.Title != null)
                {
                    var titleKey = data.Title.Entry.Definition.Key;
                    var newTitle = Strings.GetByKey(titleKey);
                    if (newTitle != null)
                        data.Title = newTitle;
                }
            }

            // 刷新 PlayerMarker 的 Title
            if (view.PlayerMarkers != null)
            {
                foreach (var playerMarker in view.PlayerMarkers)
                {
                    var data = playerMarker.Data;
                    if (data?.Title != null)
                    {
                        var titleKey = data.Title.Entry.Definition.Key;
                        var newTitle = Strings.GetByKey(titleKey);
                        if (newTitle != null)
                            data.Title = newTitle;
                    }
                }
            }
        });
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
