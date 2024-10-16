using BepInEx.Configuration;
using System.IO;
using System;
using KingdomMod.OverlayMap.Config.Extensions;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Config;

public class Strings
{
    public static ConfigFile ConfigFile;
    private static readonly ConfigFileWatcher _configFileWatcher = new();

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
    public static ConfigEntryWrapper<string> PortalCliff;
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
    public static ConfigEntryWrapper<string> PortalDock;
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
        Archer = config.Bind("Strings", "Archer", "Archer", "");
        Barrier = config.Bind("Strings", "Barrier", "Barrier", "");
        Bear = config.Bind("Strings", "Bear", "Bear", "");
        Beggar = config.Bind("Strings", "Beggar", "Beggar", "");
        BeggarCamp = config.Bind("Strings", "BeggarCamp", "Camp", "");
        Bloodstained = config.Bind("Strings", "Bloodstained", "Gamigin", "");
        BoarSpawn = config.Bind("Strings", "BoarSpawn", "BoarSpawn", "");
        Boat = config.Bind("Strings", "Boat", "Boat", "");
        BoatWreck = config.Bind("Strings", "BoatWreck", "Wreck", "");
        Bomb = config.Bind("Strings", "Bomb", "Bomb", "");
        Boss = config.Bind("Strings", "Boss", "Boss", "");
        Campfire = config.Bind("Strings", "Campfire", "Campfire", "");
        Castle = config.Bind("Strings", "Castle", "Castle", "");
        CatCart = config.Bind("Strings", "CatCart", "CatCart", "");
        Chest = config.Bind("Strings", "Chest", "Chest", "");
        CitizenHouse = config.Bind("Strings", "CitizenHouse", "CitizenHouse", "");
        PortalCliff = config.Bind("Strings", "PortalCliff", "PortalCliff", "");
        Coins = config.Bind("Strings", "Coins", "Coins", "");
        DayNight = config.Bind("Strings", "DayNight", "DayNight", "");
        Days = config.Bind("Strings", "Days", "Days", "");
        Deer = config.Bind("Strings", "Deer", "Deer", "");
        DefaultSteed = config.Bind("Strings", "DefaultSteed", "Horse", "");
        Beach = config.Bind("Strings", "Beach", "Beach", "");
        DogSpawn = config.Bind("Strings", "DogSpawn", "DogTree", "");
        Enemy = config.Bind("Strings", "Enemy", "Enemy", "");
        Farmer = config.Bind("Strings", "Farmer", "Farmer", "");
        Farmhouse = config.Bind("Strings", "Farmhouse", "Farmhouse", "");
        Farmlands = config.Bind("Strings", "Farmlands", "Farmlands", "");
        Gebel = config.Bind("Strings", "Gebel", "Gebel", "");
        GemChest = config.Bind("Strings", "GemChest", "GemChest", "");
        GemMerchant = config.Bind("Strings", "GemMerchant", "GemMerchant", "");
        Gems = config.Bind("Strings", "Gems", "Gems", "");
        Griffin = config.Bind("Strings", "Griffin", "Griffin", "");
        Gullinbursti = config.Bind("Strings", "Gullinbursti", "Gullinbursti", "");
        HelPuzzleStatue = config.Bind("Strings", "HelPuzzleStatue", "HelPuzzle", "");
        HermitBaker = config.Bind("Strings", "HermitBaker", "HermitBaker", "");
        HermitBallista = config.Bind("Strings", "HermitBallista", "HermitBallista", "");
        HermitHorn = config.Bind("Strings", "HermitHorn", "HermitHorn", "");
        HermitHorse = config.Bind("Strings", "HermitHorse", "HermitHorse", "");
        HermitKnight = config.Bind("Strings", "HermitKnight", "HermitKnight", "");
        HermitPersephone = config.Bind("Strings", "HermitPersephone", "HermitPersephone", "");
        HermitFire = config.Bind("Strings", "HermitFire", "HermitFire", "");
        Hooded = config.Bind("Strings", "Hooded", "Hooded", "");
        HorseBurst = config.Bind("Strings", "HorseBurst", "HorseBurst", "");
        HorseFast = config.Bind("Strings", "HorseFast", "HorseFast", "");
        HorseStamina = config.Bind("Strings", "HorseStamina", "HorseStamina", "");
        Kelpie = config.Bind("Strings", "Kelpie", "Kelpie", "");
        King = config.Bind("Strings", "King", "King", "");
        Knight = config.Bind("Strings", "Knight", "Knight", "");
        Land = config.Bind("Strings", "Land", "Land", "");
        Lizard = config.Bind("Strings", "Lizard", "Lizard", "");
        MerchantHouse = config.Bind("Strings", "MerchantHouse", "Merchant", "");
        Mine = config.Bind("Strings", "Mine", "Mine", "");
        Miriam = config.Bind("Strings", "Miriam", "Miriam", "");
        P1 = config.Bind("Strings", "P1", "P1", "");
        P2 = config.Bind("Strings", "P2", "P2", "");
        Peasant = config.Bind("Strings", "Peasant", "Peasant", "");
        Pikeman = config.Bind("Strings", "Pikeman", "Pikeman", "");
        Portal = config.Bind("Strings", "Portal", "Portal", "");
        PortalDock = config.Bind("Strings", "PortalDock", "PortalDock", "");
        Prince = config.Bind("Strings", "Prince", "Prince", "");
        Princess = config.Bind("Strings", "Princess", "Princess", "");
        Quarry = config.Bind("Strings", "Quarry", "Quarry", "");
        Queen = config.Bind("Strings", "Queen", "Queen", "");
        Reindeer = config.Bind("Strings", "Reindeer", "Reindeer", "");
        ShopForge = config.Bind("Strings", "ShopForge", "Smithy", "");
        Sleipnir = config.Bind("Strings", "Sleipnir", "Sleipnir", "");
        Spookyhorse = config.Bind("Strings", "Spookyhorse", "Spookyhorse", "");
        Stag = config.Bind("Strings", "Stag", "Stag", "");
        StatueArcher = config.Bind("Strings", "StatueArcher", "StatueArcher", "");
        StatueFarmer = config.Bind("Strings", "StatueFarmer", "StatueFarmer", "");
        StatueKnight = config.Bind("Strings", "StatueKnight", "StatueKnight", "");
        StatueTime = config.Bind("Strings", "StatueTime", "StatueTime", "");
        StatueWorker = config.Bind("Strings", "StatueWorker", "StatueWorker", "");
        ThorPuzzleStatue = config.Bind("Strings", "ThorPuzzleStatue", "ThorPuzzle", "");
        Time = config.Bind("Strings", "Time", "Time", "");
        Trap = config.Bind("Strings", "Trap", "Trap", "");
        Unicorn = config.Bind("Strings", "Unicorn", "Unicorn", "");
        Warhorse = config.Bind("Strings", "Warhorse", "Warhorse", "");
        Wolf = config.Bind("Strings", "Wolf", "Wolf", "");
        Worker = config.Bind("Strings", "Worker", "Worker", "");
        You = config.Bind("Strings", "You", "You", "");
        Zangetsu = config.Bind("Strings", "Zangetsu", "Zangetsu", "");
        Invalid = config.Bind("Strings", "Invalid", "INVALID", "");
        Hippocampus = config.Bind("Strings", "Hippocampus", "Hippocampus", "");
        Cerberus = config.Bind("Strings", "Cerberus", "Cerberus", "");
        Spider = config.Bind("Strings", "Spider", "Spider", "");
        TheChariotDay = config.Bind("Strings", "TheChariotDay", "TheChariotDay", "");
        TheChariotNight = config.Bind("Strings", "TheChariotNight", "TheChariotNight", "");
        Pegasus = config.Bind("Strings", "Pegasus", "Pegasus", "");
        Donkey = config.Bind("Strings", "Donkey", "Donkey", "");
        MolossianHound = config.Bind("Strings", "MolossianHound", "MolossianHound", "");
        Chimera = config.Bind("Strings", "Chimera", "Chimera", "");
        Total = config.Bind("Strings", "Total", "Total", "");
        SummonBell = config.Bind("Strings", "SummonBell", "SummonBell", "");
        TeleExitP1 = config.Bind("Strings", "TeleExitP1", "TeleExitP1", "");
        TeleExitP2 = config.Bind("Strings", "TeleExitP2", "TeleExitP2", "");
        StatuePike = config.Bind("Strings", "StatuePike", "StatuePike", "");
        HephaestusForge = config.Bind("Strings", "HephaestusForge", "HephaestusForge", "");
        Lighthouse = config.Bind("Strings", "Lighthouse", "Lighthouse", "");

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