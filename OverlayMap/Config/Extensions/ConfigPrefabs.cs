using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Config.Extensions;

public struct ConfigPrefab
{
    public string ResName;
    public string FileName;
}

public class ConfigPrefabs
{
    private static readonly List<ConfigPrefab> _prefabs = new()
    {
        new ConfigPrefab
        {
            ResName = "KingdomMod.OverlayMap.ConfigPrefabs.GuiStyle.en-US.cfg",
            FileName = "GuiStyle.en-US.cfg"
        },
        new ConfigPrefab
        {
            ResName = "KingdomMod.OverlayMap.ConfigPrefabs.GuiStyle.ru-RU.cfg",
            FileName = "GuiStyle.ru-RU.cfg"
        },
        new ConfigPrefab
        {
            ResName = "KingdomMod.OverlayMap.ConfigPrefabs.GuiStyle.zh-CN.cfg",
            FileName = "GuiStyle.zh-CN.cfg"
        },
        new ConfigPrefab
        {
            ResName = "KingdomMod.OverlayMap.ConfigPrefabs.MarkerStyle.cfg",
            FileName = "MarkerStyle.cfg"
        },
        new ConfigPrefab
        {
            ResName = "KingdomMod.OverlayMap.ConfigPrefabs.Language.en-US.cfg",
            FileName = "Language.en-US.cfg"
        },
        new ConfigPrefab
        {
            ResName = "KingdomMod.OverlayMap.ConfigPrefabs.Language.ru-RU.cfg",
            FileName = "Language.ru-RU.cfg"
        },
        new ConfigPrefab
        {
            ResName = "KingdomMod.OverlayMap.ConfigPrefabs.Language.zh-CN.cfg",
            FileName = "Language.zh-CN.cfg"
        },
        new ConfigPrefab
        {
            ResName = "KingdomMod.OverlayMap.Assets.Background.png",
            FileName = "Assets\\Background.png"
        }
    };

    public static void Initialize()
    {
        foreach (var prefab in _prefabs)
        {
            var configFile = Path.Combine(BepInExDir, "config", "KingdomMod.OverlayMap", prefab.FileName);
            LogDebug($"Config prefab file: {configFile}");
            if (!File.Exists(configFile))
            {
                LogDebug($"Config prefab file do not exist: {configFile}");

                var directoryPath = Path.GetDirectoryName(configFile);
                if (directoryPath != null)
                    Directory.CreateDirectory(directoryPath);

                File.WriteAllBytes(configFile, GetEmbeddedResourceAsBytes(prefab.ResName));
            }
        }
    }

    public static string GetEmbeddedResource(string res)
    {
        using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
        if (s != null)
        {
            using var reader = new StreamReader(s);
            return reader.ReadToEnd();
        }

        return string.Empty;
    }

    public static byte[] GetEmbeddedResourceAsBytes(string res)
    {
        using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
        if (s != null)
        {
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }

        return new byte[0];
    }
}