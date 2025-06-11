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
    private Rect? _cachedRect;
    private RectInt? _cachedRectInt;
    private RectOffset _cachedRectOffset;

    public ConfigEntryWrapper(ConfigEntry<T> entry)
    {
        Entry = entry;
    }

    public static implicit operator ConfigEntry<T>(ConfigEntryWrapper<T> d) => d.Entry;
    public static implicit operator ConfigEntryWrapper<T>(ConfigEntry<T> d) => new ConfigEntryWrapper<T>(d);
    public static implicit operator T(ConfigEntryWrapper<T> d) => d.Entry.Value;

    public static implicit operator Color(ConfigEntryWrapper<T> d)
    {
        if (d._cachedColor == null)
        {
            d.Entry.SettingChanged += (sender, args) => OnColorValueChanged(d, args);
            d._cachedColor = StrToColor(d.Entry as ConfigEntry<string>);
        }
        return d._cachedColor.Value;
    }

    private static void OnColorValueChanged(object sender, EventArgs e)
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

    public static implicit operator Rect(ConfigEntryWrapper<T> d)
    {
        if (d._cachedRect == null)
        {
            d.Entry.SettingChanged += (sender, args) => OnRectValueChanged(d, args);
            d._cachedRect = StrToRect(d.Entry as ConfigEntry<string>);
        }

        return d._cachedRect.Value;
    }

    private static void OnRectValueChanged(object sender, EventArgs e)
    {
        if (sender is ConfigEntryWrapper<string> entryWrapper)
        {
            entryWrapper._cachedRect = StrToRect(entryWrapper.Entry);
        }
    }

    private static Rect StrToRect(ConfigEntry<string> entry)
    {
        try
        {
            var str = entry.Value;
            if (str != null)
            {
                var values = str.Split(',');
                if (values.Length == 4)
                    return new Rect(
                        float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(values[3], CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        catch (Exception e)
        {
            LogError($"Parse UnityEngine.Rect failed: ConfigEntry: [{entry.Definition.Section}].{entry.Definition.Key}, ConfigValue: {entry.Value}\nException: {e}");
        }

        return Rect.zero;
    }

    /// <summary>
    /// Returns a RectInt from the ConfigEntryWrapper.
    /// </summary>
    /// <param name="d"></param>

    public static implicit operator RectInt(ConfigEntryWrapper<T> d)
    {
        if (d._cachedRectInt == null)
        {
            d.Entry.SettingChanged += (sender, args) => OnRectIntValueChanged(d, args);
            d._cachedRectInt = StrToRectInt(d.Entry as ConfigEntry<string>);
        }

        return d._cachedRectInt.Value;
    }

    private static void OnRectIntValueChanged(object sender, EventArgs e)
    {
        if (sender is ConfigEntryWrapper<string> entryWrapper)
        {
            entryWrapper._cachedRectInt = StrToRectInt(entryWrapper.Entry);
        }
    }

    private static RectInt StrToRectInt(ConfigEntry<string> entry)
    {
        try
        {
            var str = entry.Value;
            if (str != null)
            {
                var values = str.Split(',');
                if (values.Length == 4)
                    return new RectInt(
                        int.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat),
                        int.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat),
                        int.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat),
                        int.Parse(values[3], CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        catch (Exception e)
        {
            LogError($"Parse UnityEngine.Rect failed: ConfigEntry: [{entry.Definition.Section}].{entry.Definition.Key}, ConfigValue: {entry.Value}\nException: {e}");
        }

        return new RectInt();
    }

    /// <summary>
    /// Returns a RectOffset from the ConfigEntryWrapper.
    /// </summary>
    /// <param name="d"></param>

    public static implicit operator RectOffset(ConfigEntryWrapper<T> d)
    {
        if (d._cachedRectOffset == null)
        {
            d.Entry.SettingChanged += (sender, args) => OnRectOffsetValueChanged(d, args);
            d._cachedRectOffset = StrToRectOffset(d.Entry as ConfigEntry<string>);
        }

        return d._cachedRectOffset;
    }

    private static void OnRectOffsetValueChanged(object sender, EventArgs e)
    {
        if (sender is ConfigEntryWrapper<string> entryWrapper)
        {
            entryWrapper._cachedRectOffset = StrToRectOffset(entryWrapper.Entry);
        }
    }

    private static RectOffset StrToRectOffset(ConfigEntry<string> entry)
    {
        try
        {
            var str = entry.Value;
            if (str != null)
            {
                var values = str.Split(',');
                if (values.Length == 4)
                    return new RectOffset(
                        int.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat),
                        int.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat),
                        int.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat),
                        int.Parse(values[3], CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        catch (Exception e)
        {
            LogError($"Parse UnityEngine.Rect failed: ConfigEntry: [{entry.Definition.Section}].{entry.Definition.Key}, ConfigValue: {entry.Value}\nException: {e}");
        }

        return new RectOffset();
    }
}