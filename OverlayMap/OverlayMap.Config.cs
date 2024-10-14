using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx.Configuration;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Color = UnityEngine.Color;
using File = System.IO.File;

namespace KingdomMod
{
    public partial class OverlayMap
    {
        public class Config
        {
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
                        ResName = "KingdomMod.ConfigPrefabs.KingdomMod.OverlayMap.Style.cfg",
                        FileName = "KingdomMod.OverlayMap.Style.cfg"
                    },
                    new ConfigPrefab
                    {
                        ResName = "KingdomMod.ConfigPrefabs.KingdomMod.OverlayMap.Language_en-US.cfg",
                        FileName = "KingdomMod.OverlayMap.Language.en-US.cfg"
                    },
                    new ConfigPrefab
                    {
                        ResName = "KingdomMod.ConfigPrefabs.KingdomMod.OverlayMap.Language_zh-CN.cfg",
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

            public class Global
            {
                public static ConfigFile ConfigFile;
                private static readonly ConfigFileWatcher _configFileWatcher = new ();
                public static ConfigEntryWrapper<string> Language;
                public static ConfigEntryWrapper<string> StyleFile;
                public static ConfigEntryWrapper<int> GUIUpdatesPerSecond;

                public static void ConfigBind(ConfigFile config)
                {
                    LogMessage($"ConfigBind: {Path.GetFileName(config.ConfigFilePath)}");

                    ConfigPrefabs.Initialize();

                    ConfigFile = config;
                    config.SaveOnConfigSet = true;
                    config.Clear();

                    Language = config.Bind("Global", "Language", "system", "");
                    StyleFile = config.Bind("Global", "StyleFile", "KingdomMod.OverlayMap.Style.cfg", "");
                    GUIUpdatesPerSecond = config.Bind("Global", "GUIUpdatesPerSecond", 10, "Increase to be more accurate, decrease to reduce performance impact");

                    LogMessage($"ConfigFilePath: {config.ConfigFilePath}");
                    LogMessage($"Language: {Language.Value}");
                    LogMessage($"StyleFile: {StyleFile.Value}");
                    LogMessage($"GUIUpdatesPerSecond: {GUIUpdatesPerSecond.Value}");

                    LogMessage($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");

                    OnLanguageChanged();
                    OnStyleFileChanged();

                    SetConfigDelegates();
                    _configFileWatcher.Set(Path.GetFileName(config.ConfigFilePath), OnConfigFileChanged);
                }

                private static void OnConfigFileChanged(object source, FileSystemEventArgs e)
                {
                    try
                    {
                        // LogMessage($"OnConfigFileChanged: {e.Name}, {e.ChangeType}");

                        ConfigFile.Reload();
                    }
                    catch (Exception exception)
                    {
                        LogMessage($"HResult: {exception.HResult:X}, {exception.Message}");
                    }
                }

                public static void SetConfigDelegates()
                {
                    Language.Entry.SettingChanged += (sender, args) => OnLanguageChanged();
                    StyleFile.Entry.SettingChanged += (sender, args) => OnStyleFileChanged();
                }

                public static void OnLanguageChanged()
                {
                    LogMessage($"OnLanguageChanged: {Language.Entry.Value}");

                    var lang = Language.Value;
                    if (lang is "" or "system")
                        lang = CultureInfo.CurrentCulture.Name;

                    var bepInExDir = GetBepInExDir();
                    var langFile = Path.Combine(bepInExDir, "config", $"KingdomMod.OverlayMap.Language.{lang}.cfg");
                    LogMessage($"Language file: {langFile}");

                    if (!File.Exists(langFile))
                    {
                        log.LogWarning($"Language file do not exist: {langFile}");
                        lang = lang.Split('-')[0];
                        var files = Directory.GetFiles(Path.Combine(bepInExDir, "config"), $"KingdomMod.OverlayMap.Language.{lang}*.cfg");
                        foreach (var file in files)
                        {
                            if (File.Exists(file))
                            {
                                langFile = file;
                                log.LogWarning($"Try to use the sub language file: {langFile}");
                                break;
                            }
                        }
                    }

                    if (!File.Exists(langFile))
                    {
                        log.LogWarning($"Language file do not exist: {langFile}");
                        if (Strings.ConfigFile != null)
                            return;
                        lang = "en-US";
                        langFile = Path.Combine(bepInExDir, "config", $"KingdomMod.OverlayMap.Language.{lang}.cfg");
                        log.LogWarning($"Try to use the default english language file: {langFile}");
                    }

                    if (Strings.ConfigFile != null)
                    {
                        if (Path.GetFileName(Strings.ConfigFile.ConfigFilePath) == Path.GetFileName(langFile))
                        {
                            LogMessage("Attempt to load the same configuration file. Skip.");
                            return;
                        }
                    }

                    LogMessage($"Try to bind Language file: {Path.GetFileName(langFile)}");
                    Strings.ConfigBind(new ConfigFile(langFile, true));
                }

                public static void OnStyleFileChanged()
                {
                    LogMessage($"OnStyleFileChanged: {StyleFile.Value}");

                    var bepInExDir = GetBepInExDir();
                    var styleFile = Path.Combine(bepInExDir, "config", StyleFile);
                    LogMessage($"Style file: {styleFile}");

                    if (!File.Exists(styleFile))
                    {
                        log.LogWarning($"Style file do not exist: {styleFile}");
                        if (Style.ConfigFile != null)
                            return;
                        styleFile = Path.Combine(bepInExDir, "config", "KingdomMod.OverlayMap.Style.cfg");
                        log.LogWarning($"Try to use the default style file: {styleFile}");
                    }

                    if (Style.ConfigFile != null)
                    {
                        if (Path.GetFileName(Style.ConfigFile.ConfigFilePath) == Path.GetFileName(styleFile))
                        {
                            LogMessage("Attempt to load the same configuration file. Skip.");
                            return;
                        }
                    }

                    LogMessage($"Try to bind style file: {Path.GetFileName(styleFile)}");
                    Style.ConfigBind(new ConfigFile(styleFile, true));
                }
            }

            public class ExploredRegions
            {
                public static ConfigFile ConfigFile;

                public static ConfigEntryWrapper<float> ExploredLeft;
                public static ConfigEntryWrapper<float> ExploredRight;
                public static ConfigEntryWrapper<float> Time;
                public static ConfigEntryWrapper<int>   Days;

                public static void ConfigBind(string archiveFilename)
                {
                    var bepInExDir = GetBepInExDir();
                    var configFilePath = Path.Combine(bepInExDir, "config", "KingdomMod.OverlayMap.ExploredRegions.cfg");
                    LogMessage($"ExploredRegions file: {configFilePath}");

                    ConfigFile = new ConfigFile(configFilePath, true);
                    ConfigFile.SaveOnConfigSet = true;
                    ConfigFile.Clear();

                    ExploredLeft = ConfigFile.Bind(archiveFilename, "ExploredLeft", 0f);
                    ExploredRight = ConfigFile.Bind(archiveFilename, "ExploredRight", 0f);
                    Time = ConfigFile.Bind(archiveFilename, "Time", 0f);
                    Days = ConfigFile.Bind(archiveFilename, "Days", 0);

                    LogMessage($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");
                }
            }

            public class Strings
            {
                public static ConfigFile ConfigFile;
                private static readonly ConfigFileWatcher _configFileWatcher = new ();

                public static ConfigEntryWrapper<string> Alfred;
                public static ConfigEntryWrapper<string> Archer;
                public static ConfigEntryWrapper<string> Barrier;
                public static ConfigEntryWrapper<string> Bear;
                public static ConfigEntryWrapper<string> Invalid;
                public static ConfigEntryWrapper<string> Beggar;
                public static ConfigEntryWrapper<string> BeggarCamp;
                public static ConfigEntryWrapper<string> Bloodstained;
                public static ConfigEntryWrapper<string> BoarSpawn;
                public static ConfigEntryWrapper<string> Boat;
                public static ConfigEntryWrapper<string> BoatWreck;
                public static ConfigEntryWrapper<string> Bomb;
                public static ConfigEntryWrapper<string> Boss;
                public static ConfigEntryWrapper<string> Campfire;
                public static ConfigEntryWrapper<string> Castle;
                public static ConfigEntryWrapper<string> CatCart;
                public static ConfigEntryWrapper<string> Chest;
                public static ConfigEntryWrapper<string> CitizenHouse;
                public static ConfigEntryWrapper<string> Cliff;
                public static ConfigEntryWrapper<string> Coins;
                public static ConfigEntryWrapper<string> DayNight;
                public static ConfigEntryWrapper<string> Days;
                public static ConfigEntryWrapper<string> Deer;
                public static ConfigEntryWrapper<string> DefaultSteed;
                public static ConfigEntryWrapper<string> Beach;
                public static ConfigEntryWrapper<string> DogSpawn;
                public static ConfigEntryWrapper<string> Enemy;
                public static ConfigEntryWrapper<string> Farmer;
                public static ConfigEntryWrapper<string> Farmhouse;
                public static ConfigEntryWrapper<string> Farmlands;
                public static ConfigEntryWrapper<string> Gebel;
                public static ConfigEntryWrapper<string> GemChest;
                public static ConfigEntryWrapper<string> GemMerchant;
                public static ConfigEntryWrapper<string> Gems;
                public static ConfigEntryWrapper<string> Griffin;
                public static ConfigEntryWrapper<string> Gullinbursti;
                public static ConfigEntryWrapper<string> HelPuzzleStatue;
                public static ConfigEntryWrapper<string> HermitBaker;
                public static ConfigEntryWrapper<string> HermitBallista;
                public static ConfigEntryWrapper<string> HermitHorn;
                public static ConfigEntryWrapper<string> HermitHorse;
                public static ConfigEntryWrapper<string> HermitKnight;
                public static ConfigEntryWrapper<string> HermitPersephone;
                public static ConfigEntryWrapper<string> HermitFire;
                public static ConfigEntryWrapper<string> Hooded;
                public static ConfigEntryWrapper<string> HorseBurst;
                public static ConfigEntryWrapper<string> HorseFast;
                public static ConfigEntryWrapper<string> HorseStamina;
                public static ConfigEntryWrapper<string> Kelpie;
                public static ConfigEntryWrapper<string> King;
                public static ConfigEntryWrapper<string> Knight;
                public static ConfigEntryWrapper<string> Land;
                public static ConfigEntryWrapper<string> Lizard;
                public static ConfigEntryWrapper<string> MerchantHouse;
                public static ConfigEntryWrapper<string> Mine;
                public static ConfigEntryWrapper<string> Miriam;
                public static ConfigEntryWrapper<string> P1;
                public static ConfigEntryWrapper<string> P2;
                public static ConfigEntryWrapper<string> Peasant;
                public static ConfigEntryWrapper<string> Pikeman;
                public static ConfigEntryWrapper<string> Portal;
                public static ConfigEntryWrapper<string> Prince;
                public static ConfigEntryWrapper<string> Princess;
                public static ConfigEntryWrapper<string> Quarry;
                public static ConfigEntryWrapper<string> Queen;
                public static ConfigEntryWrapper<string> Reindeer;
                public static ConfigEntryWrapper<string> Hippocampus;
                public static ConfigEntryWrapper<string> Cerberus;
                public static ConfigEntryWrapper<string> Spider;
                public static ConfigEntryWrapper<string> TheChariotDay;
                public static ConfigEntryWrapper<string> TheChariotNight;
                public static ConfigEntryWrapper<string> Pegasus;
                public static ConfigEntryWrapper<string> Donkey;
                public static ConfigEntryWrapper<string> MolossianHound;
                public static ConfigEntryWrapper<string> Chimera;
                public static ConfigEntryWrapper<string> Total;
                public static ConfigEntryWrapper<string> ShopForge;
                public static ConfigEntryWrapper<string> Sleipnir;
                public static ConfigEntryWrapper<string> Spookyhorse;
                public static ConfigEntryWrapper<string> Stag;
                public static ConfigEntryWrapper<string> StatueArcher;
                public static ConfigEntryWrapper<string> StatueFarmer;
                public static ConfigEntryWrapper<string> StatueKnight;
                public static ConfigEntryWrapper<string> StatueTime;
                public static ConfigEntryWrapper<string> StatueWorker;
                public static ConfigEntryWrapper<string> ThorPuzzleStatue;
                public static ConfigEntryWrapper<string> Time;
                public static ConfigEntryWrapper<string> Trap;
                public static ConfigEntryWrapper<string> Unicorn;
                public static ConfigEntryWrapper<string> Warhorse;
                public static ConfigEntryWrapper<string> Wolf;
                public static ConfigEntryWrapper<string> Worker;
                public static ConfigEntryWrapper<string> You;
                public static ConfigEntryWrapper<string> Zangetsu;
                public static ConfigEntryWrapper<string> SummonBell;
                public static ConfigEntryWrapper<string> TeleExitP1;
                public static ConfigEntryWrapper<string> TeleExitP2;
                public static ConfigEntryWrapper<string> StatuePike;
                public static ConfigEntryWrapper<string> HephaestusForge;
                public static ConfigEntryWrapper<string> Lighthouse;

                public static void ConfigBind(ConfigFile config)
                {
                    LogMessage($"ConfigBind: {Path.GetFileName(config.ConfigFilePath)}");

                    ConfigFile = config;
                    config.SaveOnConfigSet = false;
                    config.Clear();

                    Alfred = config.Bind("Strings", "Alfred", "Alfred", "");
                    Archer             = config.Bind("Strings", "Archer", "Archer", "");
                    Barrier            = config.Bind("Strings", "Barrier", "Barrier", "");
                    Bear               = config.Bind("Strings", "Bear", "Bear", "");
                    Beggar             = config.Bind("Strings", "Beggar", "Beggar", "");
                    BeggarCamp         = config.Bind("Strings", "BeggarCamp", "Camp", "");
                    Bloodstained       = config.Bind("Strings", "Bloodstained", "Gamigin", "");
                    BoarSpawn          = config.Bind("Strings", "BoarSpawn", "BoarSpawn", "");
                    Boat               = config.Bind("Strings", "Boat", "Boat", "");
                    BoatWreck          = config.Bind("Strings", "BoatWreck", "Wreck", "");
                    Bomb               = config.Bind("Strings", "Bomb", "Bomb", "");
                    Boss               = config.Bind("Strings", "Boss", "Boss", "");
                    Campfire           = config.Bind("Strings", "Campfire", "Campfire", "");
                    Castle             = config.Bind("Strings", "Castle", "Castle", "");
                    CatCart            = config.Bind("Strings", "CatCart", "CatCart", "");
                    Chest              = config.Bind("Strings", "Chest", "Chest", "");
                    CitizenHouse       = config.Bind("Strings", "CitizenHouse", "CitizenHouse", "");
                    Cliff              = config.Bind("Strings", "Cliff", "Cliff", "");
                    Coins              = config.Bind("Strings", "Coins", "Coins", "");
                    DayNight           = config.Bind("Strings", "DayNight", "DayNight", "");
                    Days               = config.Bind("Strings", "Days", "Days", "");
                    Deer               = config.Bind("Strings", "Deer", "Deer", "");
                    DefaultSteed       = config.Bind("Strings", "DefaultSteed", "Horse", "");
                    Beach              = config.Bind("Strings", "Beach", "Beach", "");
                    DogSpawn           = config.Bind("Strings", "DogSpawn", "DogTree", "");
                    Enemy              = config.Bind("Strings", "Enemy", "Enemy", "");
                    Farmer             = config.Bind("Strings", "Farmer", "Farmer", "");
                    Farmhouse          = config.Bind("Strings", "Farmhouse", "Farmhouse", "");
                    Farmlands          = config.Bind("Strings", "Farmlands", "Farmlands", "");
                    Gebel              = config.Bind("Strings", "Gebel", "Gebel", "");
                    GemChest           = config.Bind("Strings", "GemChest", "GemChest", "");
                    GemMerchant        = config.Bind("Strings", "GemMerchant", "GemMerchant", "");
                    Gems               = config.Bind("Strings", "Gems", "Gems", "");
                    Griffin            = config.Bind("Strings", "Griffin", "Griffin", "");
                    Gullinbursti       = config.Bind("Strings", "Gullinbursti", "Gullinbursti", "");
                    HelPuzzleStatue    = config.Bind("Strings", "HelPuzzleStatue", "HelPuzzle", "");
                    HermitBaker        = config.Bind("Strings", "HermitBaker", "HermitBaker", "");
                    HermitBallista     = config.Bind("Strings", "HermitBallista", "HermitBallista", "");
                    HermitHorn         = config.Bind("Strings", "HermitHorn", "HermitHorn", "");
                    HermitHorse        = config.Bind("Strings", "HermitHorse", "HermitHorse", "");
                    HermitKnight       = config.Bind("Strings", "HermitKnight", "HermitKnight", "");
                    HermitPersephone   = config.Bind("Strings", "HermitPersephone", "HermitPersephone", "");
                    HermitFire         = config.Bind("Strings", "HermitFire", "HermitFire", "");
                    Hooded             = config.Bind("Strings", "Hooded", "Hooded", "");
                    HorseBurst         = config.Bind("Strings", "HorseBurst", "HorseBurst", "");
                    HorseFast          = config.Bind("Strings", "HorseFast", "HorseFast", "");
                    HorseStamina       = config.Bind("Strings", "HorseStamina", "HorseStamina", "");
                    Kelpie             = config.Bind("Strings", "Kelpie", "Kelpie", "");
                    King               = config.Bind("Strings", "King", "King", "");
                    Knight             = config.Bind("Strings", "Knight", "Knight", "");
                    Land               = config.Bind("Strings", "Land", "Land", "");
                    Lizard             = config.Bind("Strings", "Lizard", "Lizard", "");
                    MerchantHouse      = config.Bind("Strings", "MerchantHouse", "Merchant", "");
                    Mine               = config.Bind("Strings", "Mine", "Mine", "");
                    Miriam             = config.Bind("Strings", "Miriam", "Miriam", "");
                    P1                 = config.Bind("Strings", "P1", "P1", "");
                    P2                 = config.Bind("Strings", "P2", "P2", "");
                    Peasant            = config.Bind("Strings", "Peasant", "Peasant", "");
                    Pikeman            = config.Bind("Strings", "Pikeman", "Pikeman", "");
                    Portal             = config.Bind("Strings", "Portal", "Portal", "");
                    Prince             = config.Bind("Strings", "Prince", "Prince", "");
                    Princess           = config.Bind("Strings", "Princess", "Princess", "");
                    Quarry             = config.Bind("Strings", "Quarry", "Quarry", "");
                    Queen              = config.Bind("Strings", "Queen", "Queen", "");
                    Reindeer           = config.Bind("Strings", "Reindeer", "Reindeer", "");
                    ShopForge          = config.Bind("Strings", "ShopForge", "Smithy", "");
                    Sleipnir           = config.Bind("Strings", "Sleipnir", "Sleipnir", "");
                    Spookyhorse        = config.Bind("Strings", "Spookyhorse", "Spookyhorse", "");
                    Stag               = config.Bind("Strings", "Stag", "Stag", "");
                    StatueArcher       = config.Bind("Strings", "StatueArcher", "StatueArcher", "");
                    StatueFarmer       = config.Bind("Strings", "StatueFarmer", "StatueFarmer", "");
                    StatueKnight       = config.Bind("Strings", "StatueKnight", "StatueKnight", "");
                    StatueTime         = config.Bind("Strings", "StatueTime", "StatueTime", "");
                    StatueWorker       = config.Bind("Strings", "StatueWorker", "StatueWorker", "");
                    ThorPuzzleStatue   = config.Bind("Strings", "ThorPuzzleStatue", "ThorPuzzle", "");
                    Time               = config.Bind("Strings", "Time", "Time", "");
                    Trap               = config.Bind("Strings", "Trap", "Trap", "");
                    Unicorn            = config.Bind("Strings", "Unicorn", "Unicorn", "");
                    Warhorse           = config.Bind("Strings", "Warhorse", "Warhorse", "");
                    Wolf               = config.Bind("Strings", "Wolf", "Wolf", "");
                    Worker             = config.Bind("Strings", "Worker", "Worker", "");
                    You                = config.Bind("Strings", "You", "You", "");
                    Zangetsu           = config.Bind("Strings", "Zangetsu", "Zangetsu", "");
                    Invalid            = config.Bind("Strings", "Invalid", "INVALID", "");
                    Hippocampus        = config.Bind("Strings", "Hippocampus", "Hippocampus", "");
                    Cerberus           = config.Bind("Strings", "Cerberus", "Cerberus", "");
                    Spider             = config.Bind("Strings", "Spider", "Spider", "");
                    TheChariotDay      = config.Bind("Strings", "TheChariotDay", "TheChariotDay", "");
                    TheChariotNight    = config.Bind("Strings", "TheChariotNight", "TheChariotNight", "");
                    Pegasus            = config.Bind("Strings", "Pegasus", "Pegasus", "");
                    Donkey             = config.Bind("Strings", "Donkey", "Donkey", "");
                    MolossianHound     = config.Bind("Strings", "MolossianHound", "MolossianHound", "");
                    Chimera            = config.Bind("Strings", "Chimera", "Chimera", "");
                    Total              = config.Bind("Strings", "Total", "Total", "");
                    SummonBell         = config.Bind("Strings", "SummonBell", "SummonBell", "");
                    TeleExitP1         = config.Bind("Strings", "TeleExitP1", "TeleExitP1", "");
                    TeleExitP2         = config.Bind("Strings", "TeleExitP2", "TeleExitP2", "");
                    StatuePike         = config.Bind("Strings", "StatuePike", "StatuePike", "");
                    HephaestusForge    = config.Bind("Strings", "HephaestusForge", "HephaestusForge", "");
                    Lighthouse         = config.Bind("Strings", "Lighthouse", "Lighthouse", "");
                    
                    LogMessage($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");

                    _configFileWatcher.Set(Path.GetFileName(config.ConfigFilePath), OnConfigFileChanged);
                }

                private static void OnConfigFileChanged(object source, FileSystemEventArgs e)
                {
                    try
                    {
                        LogMessage($"OnConfigFileChanged: {e.Name}, {e.ChangeType}");

                        ConfigFile.Reload();
                    }
                    catch (Exception exception)
                    {
                        LogMessage($"HResult: {exception.HResult:X}, {exception.Message}");
                    }
                }
            }

            public struct MarkerConfig
            {
                public ConfigEntryWrapper<string> Color;
                public ConfigEntryWrapper<string> Sign;
            }

            public struct MarkerConfigColor
            {
                public ConfigEntryWrapper<string> Color;
            }

            public struct MarkerConfigStated
            {
                public ConfigEntryWrapper<string> Color;
                public ConfigEntryWrapper<string> Sign;
                public MarkerConfigColor Rebuilding;
                public MarkerConfigColor Destroyed;
                public MarkerConfigColor Locked;
                public MarkerConfigColor Unlocked;
                public MarkerConfigColor Wrecked;
                public MarkerConfigColor Building;
            }

            public class Style
            {
                public static ConfigFile ConfigFile;
                private static readonly ConfigFileWatcher _configFileWatcher = new();

                public static MarkerConfig Portal;
                public static MarkerConfigStated Cliff;
                public static MarkerConfigStated Beach;
                public static MarkerConfig BeggarCamp;
                public static MarkerConfig Beggar;
                public static MarkerConfig Player;
                public static MarkerConfig PlayerSelf;
                public static MarkerConfig Deer;
                public static MarkerConfig DeerFollowing;
                public static MarkerConfig Boss;
                public static MarkerConfig Enemy;
                public static MarkerConfigStated Castle;
                public static MarkerConfig Campfire;
                public static MarkerConfig Chest;
                public static MarkerConfig GemChest;
                public static MarkerConfigStated Wall;
                public static MarkerConfig ShopForge;
                public static MarkerConfig CitizenHouse;
                public static MarkerConfig WallFoundation;
                public static MarkerConfig River;
                public static MarkerConfig BerryBush;
                public static MarkerConfig BerryBushPaid;
                public static MarkerConfig GemMerchant;
                public static MarkerConfig DogSpawn;
                public static MarkerConfig BoarSpawn;
                public static MarkerConfig Bomb;
                public static MarkerConfig Farmhouse;
                public static MarkerConfig Steeds;
                public static MarkerConfig SteedSpawns;
                public static MarkerConfigStated Statues;
                public static MarkerConfig StatueTime;
                public static MarkerConfigStated Boat;
                public static MarkerConfigStated Quarry;
                public static MarkerConfigStated Mine;
                public static MarkerConfigStated RulerSpawns;
                public static MarkerConfigStated HermitCabins;
                public static MarkerConfigStated PersephoneCage;
                public static MarkerConfig MerchantHouse;
                public static MarkerConfigStated ThorPuzzleStatue;
                public static MarkerConfigStated HelPuzzleStatue;
                public static MarkerConfigColor StatsInfo;
                public static MarkerConfigColor ExtraInfo;
                public static MarkerConfigStated WallLine;
                public static MarkerConfig SummonBell;
                public static MarkerConfig TeleExitP1;
                public static MarkerConfig TeleExitP2;
                public static MarkerConfig HephaestusForge;
                public static MarkerConfig Lighthouse;

                public static void ConfigBind(ConfigFile config)
                {
                    LogMessage($"ConfigBind: {Path.GetFileName(config.ConfigFilePath)}");

                    ConfigFile = config;
                    config.SaveOnConfigSet = false;
                    config.Clear();

                    Portal.Color = config.Bind("Portal", "Color", "0.62,0,1,1", "");
                    Portal.Sign = config.Bind("Portal", "Sign", "", "");

                    Cliff.Sign = config.Bind("Cliff", "Sign", "", "");
                    Cliff.Color = config.Bind("Cliff", "Color", "1,0,0,1", "");
                    Cliff.Rebuilding.Color = config.Bind("Cliff.Rebuilding", "Color", "0,0,1,1", "");
                    Cliff.Destroyed.Color = config.Bind("Cliff.Destroyed", "Color", "0,1,0,1", "");

                    Beach.Sign = config.Bind("Beach", "Sign", "", "");
                    Beach.Color = config.Bind("Beach", "Color", "1,0,0,1", "");
                    Beach.Destroyed.Color = config.Bind("Beach.Destroyed", "Color", "0,1,0,1", "");

                    BeggarCamp.Color = config.Bind("BeggarCamp", "Color", "1,1,1,1", "");
                    BeggarCamp.Sign = config.Bind("BeggarCamp", "Sign", "", "");

                    Beggar.Color = config.Bind("Beggar", "Color", "1,0,0,1", "");
                    Beggar.Sign = config.Bind("Beggar", "Sign", "", "");

                    Player.Color = config.Bind("Player", "Color", "0,0,1,1", "");
                    Player.Sign = config.Bind("Player", "Sign", "", "");
                    PlayerSelf.Color = config.Bind("Player.Self", "Color", "0,1,0,1", "");

                    Deer.Color = config.Bind("Deer", "Color", "0,0,1,1", "");
                    Deer.Sign = config.Bind("Deer", "Sign", "", "");
                    DeerFollowing.Color = config.Bind("Deer.Following", "Color", "0,1,0,1", "");

                    Boss.Color = config.Bind("Boss", "Color", "1,0,0,1", "");
                    Boss.Sign = config.Bind("Boss", "Sign", "", "");

                    Enemy.Color = config.Bind("Enemy", "Color", "1,0,0,1", "");
                    Enemy.Sign = config.Bind("Enemy", "Sign", "", "");

                    Castle.Sign = config.Bind("Castle", "Sign", "♜", "");
                    Castle.Color = config.Bind("Castle", "Color", "0,1,0,1", "");
                    Castle.Locked.Color = config.Bind("Castle.Locked", "Color", "0.5,0.5,0.5,1", "");

                    Campfire.Color = config.Bind("Campfire", "Color", "1,1,1,1", "");
                    Campfire.Sign = config.Bind("Campfire", "Sign", "", "");

                    Chest.Color = config.Bind("Chest", "Color", "1,1,1,1", "");
                    Chest.Sign = config.Bind("Chest", "Sign", "", "");

                    GemChest.Color = config.Bind("GemChest", "Color", "1,1,1,1", "");
                    GemChest.Sign = config.Bind("GemChest", "Sign", "", "");

                    Wall.Sign = config.Bind("Wall", "Sign", "۩", "");
                    Wall.Color = config.Bind("Wall", "Color", "0,1,0,1", "");
                    Wall.Wrecked.Color = config.Bind("Wall.Wrecked", "Color", "1,0,0,1", "");
                    Wall.Building.Color = config.Bind("Wall.Building", "Color", "0,0,1,1", "");

                    ShopForge.Color = config.Bind("ShopForge", "Color", "1,1,1,1", "");
                    ShopForge.Sign = config.Bind("ShopForge", "Sign", "", "");

                    CitizenHouse.Color = config.Bind("CitizenHouse", "Color", "1,1,1,1", "");
                    CitizenHouse.Sign = config.Bind("CitizenHouse", "Sign", "", "");

                    WallFoundation.Color = config.Bind("WallFoundation", "Color", "0.5,0.5,0.5,1", "");
                    WallFoundation.Sign = config.Bind("WallFoundation", "Sign", "∧", "");

                    River.Color = config.Bind("River", "Color", "0.46,0.84,0.92,1", "");
                    River.Sign = config.Bind("River", "Sign", "≈", "");

                    BerryBush.Color = config.Bind("BerryBush", "Color", "1,0,0,1", "");
                    BerryBush.Sign = config.Bind("BerryBush", "Sign", "♣", "");

                    BerryBushPaid.Color = config.Bind("BerryBush.Paid", "Color", "0,1,0,1", "");
                    BerryBushPaid.Sign = config.Bind("BerryBush.Paid", "Sign", "♣", "");

                    GemMerchant.Color = config.Bind("GemMerchant", "Color", "1,1,1,1", "");
                    GemMerchant.Sign = config.Bind("GemMerchant", "Sign", "", "");

                    DogSpawn.Color = config.Bind("DogSpawn", "Color", "1,0,0,1", "");
                    DogSpawn.Sign = config.Bind("DogSpawn", "Sign", "", "");

                    BoarSpawn.Color = config.Bind("BoarSpawn", "Color", "1,0,0,1", "");
                    BoarSpawn.Sign = config.Bind("BoarSpawn", "Sign", "", "");

                    Bomb.Color = config.Bind("Bomb", "Color", "0,1,0,1", "");
                    Bomb.Sign = config.Bind("Bomb", "Sign", "", "");

                    Farmhouse.Color = config.Bind("Farmhouse", "Color", "1,1,1,1", "");
                    Farmhouse.Sign = config.Bind("Farmhouse", "Sign", "", "");

                    Steeds.Color = config.Bind("Steeds", "Color", "0,0,1,1", "");
                    Steeds.Sign = config.Bind("Steeds", "Sign", "", "");

                    SteedSpawns.Color = config.Bind("SteedSpawns", "Color", "1,0,0,1", "");
                    SteedSpawns.Sign = config.Bind("SteedSpawns", "Sign", "", "");

                    Statues.Sign = config.Bind("Statues", "Sign", "", "");
                    Statues.Locked.Color = config.Bind("Statues.Locked", "Color", "1,0,0,1", "");
                    Statues.Unlocked.Color = config.Bind("Statues.Unlocked", "Color", "0,1,0,1", "");

                    StatueTime.Color = config.Bind("StatueTime", "Color", "0,1,0,1", "");
                    StatueTime.Sign = config.Bind("StatueTime", "Sign", "", "");

                    Boat.Sign = config.Bind("Boat", "Sign", "", "");
                    Boat.Color = config.Bind("Boat", "Color", "0,1,0,1", "");
                    Boat.Wrecked.Color = config.Bind("Boat.Wrecked", "Color", "1,0,0,1", "");

                    Quarry.Sign = config.Bind("Quarry", "Sign", "", "");
                    Quarry.Locked.Color = config.Bind("Quarry.Locked", "Color", "1,0,0,1", "");
                    Quarry.Unlocked.Color = config.Bind("Quarry.Unlocked", "Color", "0,1,0,1", "");
                    Quarry.Building.Color = config.Bind("Quarry.Building", "Color", "0,0,1,1", "");

                    Mine.Sign = config.Bind("Mine", "Sign", "", "");
                    Mine.Locked.Color = config.Bind("Mine.Locked", "Color", "1,0,0,1", "");
                    Mine.Unlocked.Color = config.Bind("Mine.Unlocked", "Color", "0,1,0,1", "");
                    Mine.Building.Color = config.Bind("Mine.Building", "Color", "0,0,1,1", "");

                    RulerSpawns.Unlocked.Color = config.Bind("RulerSpawns.Unlocked", "Color", "0,1,0,1", "");
                    RulerSpawns.Locked.Color = config.Bind("RulerSpawns.Locked", "Color", "1,0,0,1", "");
                    RulerSpawns.Building.Color = config.Bind("RulerSpawns.Building", "Color", "0,0,1,1", "");
                    RulerSpawns.Sign = config.Bind("RulerSpawns", "Sign", "", "");

                    MerchantHouse.Color = config.Bind("MerchantHouse", "Color", "1,1,1,1", "");
                    MerchantHouse.Sign = config.Bind("MerchantHouse", "Sign", "", "");

                    ThorPuzzleStatue.Locked.Color = config.Bind("ThorPuzzleStatue.Locked", "Color", "1,0,0,1", "");
                    ThorPuzzleStatue.Unlocked.Color = config.Bind("ThorPuzzleStatue.Unlocked", "Color", "0,1,0,1", "");
                    ThorPuzzleStatue.Sign = config.Bind("ThorPuzzleStatue", "Sign", "", "");

                    HelPuzzleStatue.Locked.Color = config.Bind("HelPuzzleStatue.Locked", "Color", "1,0,0,1", "");
                    HelPuzzleStatue.Unlocked.Color = config.Bind("HelPuzzleStatue.Unlocked", "Color", "0,1,0,1", "");
                    HelPuzzleStatue.Sign = config.Bind("HelPuzzleStatue", "Sign", "", "");

                    HermitCabins.Locked.Color = config.Bind("HermitCabins.Locked", "Color", "1,0,0,1", "");
                    HermitCabins.Unlocked.Color = config.Bind("HermitCabins.Unlocked", "Color", "0,1,0,1", "");
                    HermitCabins.Sign = config.Bind("HermitCabins", "Sign", "", "");

                    PersephoneCage.Locked.Color = config.Bind("PersephoneCage.Locked", "Color", "1,0,0,1", "");
                    PersephoneCage.Unlocked.Color = config.Bind("PersephoneCage.Unlocked", "Color", "0,1,0,1", "");
                    PersephoneCage.Sign = config.Bind("PersephoneCage", "Sign", "", "");

                    StatsInfo.Color = config.Bind("StatsInfo", "Color", "1,1,1,1", "");

                    ExtraInfo.Color = config.Bind("ExtraInfo", "Color", "1,1,1,1", "");

                    WallLine.Color = config.Bind("WallLine", "Color", "0,1,0,1", "");
                    WallLine.Building.Color = config.Bind("WallLine.Building", "Color", "0,0,1,1", "");
                    WallLine.Wrecked.Color = config.Bind("WallLine.Wrecked", "Color", "1,0,0,1", "");

                    SummonBell.Color = config.Bind("SummonBell", "Color", "1,1,1,1", "");
                    SummonBell.Sign = config.Bind("SummonBell", "Sign", "", "");

                    TeleExitP1.Color = config.Bind("TeleExitP1", "Color", "1,1,1,1", "");
                    TeleExitP1.Sign = config.Bind("TeleExitP1", "Sign", "", "");

                    TeleExitP2.Color = config.Bind("TeleExitP2", "Color", "1,1,1,1", "");
                    TeleExitP2.Sign = config.Bind("TeleExitP2", "Sign", "", "");

                    HephaestusForge.Color = config.Bind("HephaestusForge", "Color", "1,1,1,1", "");
                    HephaestusForge.Sign = config.Bind("HephaestusForge", "Sign", "", "");

                    Lighthouse.Color = config.Bind("Lighthouse", "Color", "1,1,1,1", "");
                    Lighthouse.Sign = config.Bind("Lighthouse", "Sign", "", "");

                    LogMessage($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");

                    _configFileWatcher.Set(Path.GetFileName(config.ConfigFilePath), OnConfigFileChanged);
                }

                private static void OnConfigFileChanged(object source, FileSystemEventArgs e)
                {
                    try
                    {
                        LogMessage($"OnConfigFileChanged: {e.Name}, {e.ChangeType}");
                        ConfigFile.Reload();
                    }
                    catch (Exception exception)
                    {
                        LogMessage($"HResult: {exception.HResult:X}, {exception.Message}");
                    }
                }
            }

            public class ConfigFileWatcher
            {
                private readonly FileSystemWatcher _watcher = new();
                private string _configFileHash;
                private FileSystemEventHandler _changedEventHandler;

                public void Set(string fileName, FileSystemEventHandler changed)
                {
                    _watcher.Path = Path.Combine(GetBepInExDir(), "config");
                    _watcher.NotifyFilter = NotifyFilters.LastWrite;
                    _watcher.Filter = fileName ?? "*.cfg";
                    _watcher.Changed += OnConfigFileChanged;
                    _watcher.IncludeSubdirectories = false;
                    _watcher.EnableRaisingEvents = true;
                    _changedEventHandler = changed;
                }

                private void OnConfigFileChanged(object source, FileSystemEventArgs e)
                {
                    try
                    {
                        var hash = GetFileHash(e.FullPath);
                        if (hash == "") return;
                        if (hash == _configFileHash) return;
                        _configFileHash = hash;
                        _changedEventHandler?.Invoke(source, e);
                    }
                    catch (Exception exception)
                    {
                        LogMessage($"HResult: {exception.HResult:X}, {exception.Message}");
                    }
                }

                public static string GetFileHash(string filename)
                {
                    int retry = 0;
                    do
                    {
                        try
                        {
                            using (var md5 = MD5.Create())
                            {
                                using (var stream = File.OpenRead(filename))
                                {
                                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if ((uint)e.HResult != 0x80070020)
                                LogMessage($"HResult: {e.HResult:X}, {e.Message}");
                        }

                        retry++;
                        System.Threading.Thread.Sleep(10);
                    } while (retry < 3);

                    return "";
                }
            }

            public static string GetBepInExDir()
            {
                var baseDir = Assembly.GetExecutingAssembly().Location;
                var bepInExDir = Directory.GetParent(baseDir)?.Parent?.Parent?.FullName;

                bepInExDir ??= "BepInEx\\";
                return bepInExDir;
            }
        }
    }
}