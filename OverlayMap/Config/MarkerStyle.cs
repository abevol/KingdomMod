using BepInEx.Configuration;
using System.IO;
using System;
using KingdomMod.OverlayMap.Config.Extensions;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Config;

public class MarkerStyle
{
    public static ConfigFile ConfigFile;
    private static readonly ConfigFileWatcher _configFileWatcher = new();

    public static MarkerConfig Portal;
    public static MarkerConfig PortalDock;
    public static MarkerConfigStated PortalCliff;
    public static MarkerConfig Beach;
    public static MarkerConfig BeggarCamp;
    public static MarkerConfig Beggar;
    public static MarkerConfig Player;
    public static MarkerConfig PlayerSelf;
    public static MarkerConfig Deer;
    public static MarkerConfig DeerFollowing;
    public static MarkerConfig Boss;
    public static MarkerConfig Enemy;
    public static MarkerConfig Knight;
    public static MarkerConfigStated Castle;
    public static MarkerConfig Campfire;
    public static MarkerConfig Chest;
    public static MarkerConfig GemChest;
    public static MarkerConfigStated Wall;
    public static MarkerConfig ShopForge;
    public static MarkerConfig ShopScythe;
    public static MarkerConfigStated CitizenHouse;
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
    public static MarkerConfig Boat;
    public static MarkerConfig BoatWreck;
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
    public static MarkerConfigStated Lighthouse;
    public static MarkerConfigStated Wharf;
    public static MarkerConfigStated Teleporter;
    public static MarkerConfig TeleporterRift;
    public static MarkerConfigStated Tholos;
    public static MarkerConfig GodIdol;
    public static MarkerConfig HermesShade;
    public static MarkerConfig WeakPoint;

    public static void ConfigBind(ConfigFile config)
    {
        LogDebug($"ConfigBind: {Path.GetFileName(config.ConfigFilePath)}");

        ConfigFile = config;
        config.SaveOnConfigSet = false;
        config.Clear();

        Portal.Color = config.Bind("Portal", "Color", "0.62,0,1,1", "");
        Portal.Sign = config.Bind("Portal", "Sign", "", "");

        PortalDock.Color = config.Bind("PortalDock", "Color", "0.62,0,1,1", "");
        PortalDock.Sign = config.Bind("PortalDock", "Sign", "", "");

        PortalCliff.Sign = config.Bind("PortalCliff", "Sign", "", "");
        PortalCliff.Color = config.Bind("PortalCliff", "Color", "1,0,0,1", "");
        PortalCliff.Rebuilding.Color = config.Bind("PortalCliff.Rebuilding", "Color", "0,0,1,1", "");
        PortalCliff.Destroyed.Color = config.Bind("PortalCliff.Destroyed", "Color", "0,1,0,1", "");

        Beach.Sign = config.Bind("Beach", "Sign", "", "");
        Beach.Color = config.Bind("Beach", "Color", "1,1,1,1", "");

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

        Knight.Color = config.Bind("Knight", "Color", "0,0,1,1", "");
        Knight.Sign = config.Bind("Knight", "Sign", "", "");

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

        ShopScythe.Color = config.Bind("ShopScythe", "Color", "1,1,1,1", "");
        ShopScythe.Sign = config.Bind("ShopScythe", "Sign", "", "");

        CitizenHouse.Sign = config.Bind("CitizenHouse", "Sign", "", "");
        CitizenHouse.Color = config.Bind("CitizenHouse", "Color", "1,1,1,1", "");
        CitizenHouse.Building.Color = config.Bind("CitizenHouse.Building", "Color", "0,0,1,1", "");

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

        Bomb.Color = config.Bind("Bomb", "Color", "1,0,0,1", "");
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

        BoatWreck.Sign = config.Bind("BoatWreck", "Sign", "", "");
        BoatWreck.Color = config.Bind("BoatWreck", "Color", "1,0,0,1", "");

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

        Lighthouse.Sign = config.Bind("Lighthouse", "Sign", "", "");
        Lighthouse.Color = config.Bind("Lighthouse", "Color", "0,1,0,1", "");
        Lighthouse.Locked.Color = config.Bind("Lighthouse.Locked", "Color", "0.5,0.5,0.5,1", "");
        Lighthouse.Unpaid.Color = config.Bind("Lighthouse.Locked", "Color", "1,0,0,1", "");
        Lighthouse.Building.Color = config.Bind("Lighthouse.Building", "Color", "0,0,1,1", "");

        Wharf.Sign = config.Bind("Wharf", "Sign", "", "");
        Wharf.Color = config.Bind("Wharf", "Color", "0,1,0,1", "");
        Wharf.Building.Color = config.Bind("Wharf.Building", "Color", "0,0,1,1", "");

        Teleporter.Color = config.Bind("Teleporter", "Color", "0.62,0,1,1", "");
        Teleporter.Sign = config.Bind("Teleporter", "Sign", "۞", "");
        Teleporter.Building.Color = config.Bind("Teleporter.Building", "Color", "0,0,1,1", "");
        TeleporterRift.Color = config.Bind("TeleporterRift", "Color", "0.62,0,1,1", "");
        TeleporterRift.Sign = config.Bind("TeleporterRift", "Sign", "✧", "");

        Tholos.Color = config.Bind("Tholos", "Color", "1,1,1,1", "");
        Tholos.Sign = config.Bind("Tholos", "Sign", "", "");
        Tholos.Unpaid.Color = config.Bind("Tholos.Unpaid", "Color", "1,0,0,1", "");
        Tholos.Building.Color = config.Bind("Tholos.Building", "Color", "0,0,1,1", "");

        GodIdol.Color = config.Bind("GodIdol", "Color", "1,0.9,0,1", "");
        GodIdol.Sign = config.Bind("GodIdol", "Sign", "", "");

        HermesShade.Color = config.Bind("HermesShade", "Color", "0.62,0,1,1", "");
        HermesShade.Sign = config.Bind("HermesShade", "Sign", "₤", "");

        WeakPoint.Color = config.Bind("WeakPoint", "Color", "1,0,0,1", "");
        WeakPoint.Sign = config.Bind("WeakPoint", "Sign", "◎", "");

        LogDebug($"Loaded config: {Path.GetFileName(ConfigFile.ConfigFilePath)}");

        _configFileWatcher.Set(config.ConfigFilePath, OnConfigFileChanged);
    }

    private static void OnConfigFileChanged(object source, FileSystemEventArgs e)
    {
        try
        {
            LogDebug($"OnConfigFileChanged: {e.Name}, {e.ChangeType}");
            ConfigFile.Reload();
        }
        catch (Exception exception)
        {
            LogError($"HResult: {exception.HResult:X}, {exception.Message}");
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
    public MarkerConfigColor Unpaid;
    public MarkerConfigColor Locked;
    public MarkerConfigColor Unlocked;
    public MarkerConfigColor Wrecked;
    public MarkerConfigColor Building;
}