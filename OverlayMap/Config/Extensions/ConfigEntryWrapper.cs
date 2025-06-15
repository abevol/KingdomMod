using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Config.Extensions;

/// <summary>
/// 配置项包装器，提供类型安全的配置值访问和自动类型转换功能
/// </summary>
/// <typeparam name="T">配置项的基础类型</typeparam>
public class ConfigEntryWrapper<T>
{
    private readonly ConfigEntry<T> _entry;
    private readonly Dictionary<Type, object> _typeCache = new();
    
    public ConfigEntry<T> Entry => _entry;
    public T Value 
    { 
        get => _entry.Value;
        set => _entry.Value = value;
    }

    public ConfigEntryWrapper(ConfigEntry<T> entry)
    {
        _entry = entry ?? throw new ArgumentNullException(nameof(entry));
    }

    public static implicit operator ConfigEntry<T>(ConfigEntryWrapper<T> d) => d._entry;
    public static implicit operator ConfigEntryWrapper<T>(ConfigEntry<T> d) => new(d);
    public static implicit operator T(ConfigEntryWrapper<T> d) => d._entry.Value;

    public static implicit operator Color(ConfigEntryWrapper<T> d) => d.GetOrCreateCachedValue<Color>(ParseColor);
    public static implicit operator Rect(ConfigEntryWrapper<T> d) => d.GetOrCreateCachedValue<Rect>(ParseRect);
    public static implicit operator RectInt(ConfigEntryWrapper<T> d) => d.GetOrCreateCachedValue<RectInt>(ParseRectInt);
    public static implicit operator RectOffset(ConfigEntryWrapper<T> d) => d.GetOrCreateCachedValue<RectOffset>(ParseRectOffset);
    public static implicit operator Vector4(ConfigEntryWrapper<T> d) => d.GetOrCreateCachedValue<Vector4>(ParseVector4);

    private TResult GetOrCreateCachedValue<TResult>(Func<ConfigEntry<string>, TResult> parser)
    {
        var type = typeof(TResult);
        if (!_typeCache.ContainsKey(type))
        {
            var result = parser(_entry as ConfigEntry<string>);
            _typeCache[type] = result;
            _entry.SettingChanged += (_, _) =>
            {
                _typeCache[type] = parser(_entry as ConfigEntry<string>);
            };
        }
        return (TResult)_typeCache[type];
    }

    private static Color ParseColor(ConfigEntry<string> entry) =>
        ParseFourComponents(
            entry,
            values => new Color(
                ParseFloat(values[0]),
                ParseFloat(values[1]),
                ParseFloat(values[2]),
                ParseFloat(values[3])),
            Color.white);

    private static Rect ParseRect(ConfigEntry<string> entry) =>
        ParseFourComponents(
            entry,
            values => new Rect(
                ParseFloat(values[0]),
                ParseFloat(values[1]),
                ParseFloat(values[2]),
                ParseFloat(values[3])),
            Rect.zero);

    private static RectInt ParseRectInt(ConfigEntry<string> entry) =>
        ParseFourComponents(
            entry,
            values => new RectInt(
                ParseInt(values[0]),
                ParseInt(values[1]),
                ParseInt(values[2]),
                ParseInt(values[3])),
            new RectInt());

    private static RectOffset ParseRectOffset(ConfigEntry<string> entry) =>
        ParseFourComponents(
            entry,
            values => new RectOffset(
                ParseInt(values[0]),
                ParseInt(values[1]),
                ParseInt(values[2]),
                ParseInt(values[3])),
            new RectOffset());

    private static Vector4 ParseVector4(ConfigEntry<string> entry) =>
        ParseFourComponents(
            entry,
            values => new Vector4(
                ParseFloat(values[0]),
                ParseFloat(values[1]),
                ParseFloat(values[2]),
                ParseFloat(values[3])),
            Vector4.zero);

    private static TResult ParseFourComponents<TResult>(
        ConfigEntry<string> entry,
        Func<string[], TResult> constructor,
        TResult defaultValue)
    {
        return ParseValue(entry, parts =>
        {
            if (parts.Length != 4)
                throw new FormatException($"{nameof(TResult)} 必须包含4个分量");
            return constructor(parts);
        }, defaultValue);
    }

    private static TResult ParseValue<TResult>(
        ConfigEntry<string> entry,
        Func<string[], TResult> parser,
        TResult defaultValue)
    {
        try
        {
            var value = entry.Value;
            if (string.IsNullOrEmpty(value)) return defaultValue;

            var parts = value.Split(',');
            return parser(parts);
        }
        catch (Exception e)
        {
            LogError($"解析失败: 配置项: [{entry.Definition.Section}].{entry.Definition.Key}, " +
                    $"配置值: {entry.Value}\n异常: {e}");
            return defaultValue;
        }
    }

    private static float ParseFloat(string value) => 
        float.Parse(value.Trim(), CultureInfo.InvariantCulture);

    private static int ParseInt(string value) => 
        int.Parse(value.Trim(), CultureInfo.InvariantCulture);

    public string[] AsStringArray
    {
        get
        {
            if (Entry is ConfigEntry<string> stringEntry)
            {
                return stringEntry.Value.Split([','], StringSplitOptions.RemoveEmptyEntries);
            }

            return [];
        }
        set
        {
            if (Entry is ConfigEntry<string> stringEntry && value != null)
            {
                stringEntry.Value = string.Join(",", value);
            }
        }
    }
}