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
                    ResName = "KingdomMod.OverlayMap.ConfigPrefabs.KingdomMod.OverlayMap.MarkerStyle.cfg",
                    FileName = "KingdomMod.OverlayMap.MarkerStyle.cfg"
                },
                new ConfigPrefab
                {
                    ResName = "KingdomMod.OverlayMap.ConfigPrefabs.KingdomMod.OverlayMap.Language_en-US.cfg",
                    FileName = "KingdomMod.OverlayMap.Language.en-US.cfg"
                },
                new ConfigPrefab
                {
                    ResName = "KingdomMod.OverlayMap.ConfigPrefabs.KingdomMod.OverlayMap.Language_zh-CN.cfg",
                    FileName = "KingdomMod.OverlayMap.Language.zh-CN.cfg"
                }
            };

    public static void Initialize()
    {
        var bepInExDir = GetBepInExDir();

        foreach (var prefab in _prefabs)
        {
            var configFile = Path.Combine(bepInExDir, "config", prefab.FileName);
            LogMessage($"Config prefab file: {configFile}");

            if (!File.Exists(configFile))
            {
                LogMessage($"Config prefab file do not exist: {configFile}");

                File.WriteAllText(configFile, GetEmbeddedResource(prefab.ResName));
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
}