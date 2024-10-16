using System;
using System.Globalization;
using BepInEx.Configuration;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Config.Extensions;

public class ConfigEntryWrapper<T>
{
    public ConfigEntry<T> Entry { get; set; }
    public T Value { get => Entry.Value; set => Entry.Value = value; }
    private Color? _cachedColor;

    public ConfigEntryWrapper(ConfigEntry<T> entry)
    {
        Entry = entry;
    }

    public static implicit operator ConfigEntry<T>(ConfigEntryWrapper<T> d) => d.Entry;
    public static implicit operator ConfigEntryWrapper<T>(ConfigEntry<T> d) => new ConfigEntryWrapper<T>(d);
    public static implicit operator T(ConfigEntryWrapper<T> d) => d.Entry.Value;

    public static implicit operator RectInt(ConfigEntryWrapper<T> d)
    {
        return d.Value switch
        {
            Rect rec => new RectInt((int)rec.x, (int)rec.y, (int)rec.width, (int)rec.height),
            Vector4 vec => new RectInt((int)vec.x, (int)vec.y, (int)vec.z, (int)vec.w),
            Quaternion qua => new RectInt((int)qua.x, (int)qua.y, (int)qua.z, (int)qua.w),
            _ => new RectInt()
        };
    }

    public static implicit operator RectOffset(ConfigEntryWrapper<T> d)
    {
        return d.Value switch
        {
            Rect rec => new RectOffset((int)rec.x, (int)rec.y, (int)rec.width, (int)rec.height),
            Vector4 vec => new RectOffset((int)vec.x, (int)vec.y, (int)vec.z, (int)vec.w),
            Quaternion qua => new RectOffset((int)qua.x, (int)qua.y, (int)qua.z, (int)qua.w),
            _ => new RectOffset()
        };
    }

    public static implicit operator Color(ConfigEntryWrapper<T> d)
    {
        if (d._cachedColor == null)
        {
            d.Entry.SettingChanged += (sender, args) => OnValueChanged(d, args);
            d._cachedColor = StrToColor(d.Entry as ConfigEntry<string>);
        }
        return d._cachedColor.Value;
    }

    private static void OnValueChanged(object sender, EventArgs e)
    {
        if (sender is ConfigEntryWrapper<string> entryWrapper)
        {
            entryWrapper._cachedColor = StrToColor(entryWrapper.Entry);
        }
    }

    private static Color StrToColor(ConfigEntry<string> entry)
    {
        try
        {
            var str = entry.Value;
            if (str != null)
            {
                var color = str.Split(',');
                if (color.Length == 4)
                    return new Color(
                        float.Parse(color[0], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(color[1], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(color[2], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(color[3], CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        catch (Exception e)
        {
            LogError($"Parse Color failed: ConfigEntry: [{entry.Definition.Section}].{entry.Definition.Key}, ConfigValue: {entry.Value}\nException: {e}");
        }

        return Color.white;
    }
}