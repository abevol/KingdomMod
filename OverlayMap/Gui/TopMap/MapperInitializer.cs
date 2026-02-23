
using System;
using System.Collections.Generic;

namespace KingdomMod.OverlayMap.Gui.TopMap;

/// <summary>
/// 负责初始化 TopMapView 的 Resolver 和 Mapper 系统。
/// 将 Resolver 和 Mapper 注册逻辑从 TopMapView 中分离，减少主类的代码量。
/// </summary>
internal static class MapperInitializer
{
    /// <summary>
    /// 初始化 TopMapView 的 Mapper 和 Resolver 系统。
    /// </summary>
    /// <param name="view">要初始化的 TopMapView 实例</param>
    public static void Initialize(TopMapView view)
    {
        // 1. 初始化 Resolver 字典
        var resolvers = new Dictionary<Type, List<IMarkerResolver>>();
        
        // 2. 注册所有 Resolver（按类别组织）
        
        // 复杂 Resolver（一对多映射）
        RegisterResolver(resolvers, new Resolvers.PayableUpgradeResolver());  // Wall, Lighthouse, Mine, Quarry
        RegisterResolver(resolvers, new Resolvers.PayableShopResolver());     // PayableShop
        RegisterResolver(resolvers, new Resolvers.WharfResolver());           // Wharf

        // 简单 Resolver（一对一映射）- 地形类
        RegisterResolver(resolvers, new Resolvers.BeachResolver());
        RegisterResolver(resolvers, new Resolvers.RiverResolver());
        
        // 简单 Resolver - 建筑类
        RegisterResolver(resolvers, new Resolvers.CastleResolver());
        RegisterResolver(resolvers, new Resolvers.ScaffoldingResolver());
        RegisterResolver(resolvers, new Resolvers.WorkableBuildingResolver());
        RegisterResolver(resolvers, new Resolvers.CabinResolver());
        RegisterResolver(resolvers, new Resolvers.FarmhouseResolver());
        RegisterResolver(resolvers, new Resolvers.CitizenHousePayableResolver());
        RegisterResolver(resolvers, new Resolvers.LighthouseResolver());
        
        // 简单 Resolver - 交互建筑
        RegisterResolver(resolvers, new Resolvers.ChestResolver());
        RegisterResolver(resolvers, new Resolvers.PayableGemChestResolver());
        RegisterResolver(resolvers, new Resolvers.BoatSummoningBellResolver());
        
        // 简单 Resolver - 传送点
        RegisterResolver(resolvers, new Resolvers.PortalResolver());
        RegisterResolver(resolvers, new Resolvers.TeleporterExitResolver());
        
        // 简单 Resolver - 雕像类
        RegisterResolver(resolvers, new Resolvers.TimeStatueResolver());
        RegisterResolver(resolvers, new Resolvers.UnlockNewRulerStatueResolver());
        RegisterResolver(resolvers, new Resolvers.StatueResolver());
        
        // 简单 Resolver - 营地类
        RegisterResolver(resolvers, new Resolvers.BeggarCampResolver());
        RegisterResolver(resolvers, new Resolvers.CampfireResolver());
        
        // 简单 Resolver - 单位类
        RegisterResolver(resolvers, new Resolvers.PlayerResolver());
        RegisterResolver(resolvers, new Resolvers.KnightResolver());
        RegisterResolver(resolvers, new Resolvers.BeggarResolver());
        RegisterResolver(resolvers, new Resolvers.DeerResolver());
        
        // 简单 Resolver - 坐骑类
        RegisterResolver(resolvers, new Resolvers.SteedResolver());
        RegisterResolver(resolvers, new Resolvers.SteedSpawnResolver());
        RegisterResolver(resolvers, new Resolvers.DogSpawnResolver());
        RegisterResolver(resolvers, new Resolvers.BoarSpawnGroupResolver());
        
        // 简单 Resolver - 载具类
        RegisterResolver(resolvers, new Resolvers.BoatResolver());
        
        // 简单 Resolver - 障碍物
        RegisterResolver(resolvers, new Resolvers.PayableBlockerResolver());
        RegisterResolver(resolvers, new Resolvers.PayableBushResolver());
        
        // 简单 Resolver - 武器
        RegisterResolver(resolvers, new Resolvers.BombResolver());
        
        // 简单 Resolver - DLC 内容
        RegisterResolver(resolvers, new Resolvers.HelPuzzleControllerResolver());
        RegisterResolver(resolvers, new Resolvers.ThorPuzzleControllerResolver());
        RegisterResolver(resolvers, new Resolvers.HephaestusForgeResolver());
        RegisterResolver(resolvers, new Resolvers.PersephoneCageResolver());
        RegisterResolver(resolvers, new Resolvers.MerchantSpawnerResolver());
        RegisterResolver(resolvers, new Resolvers.PayableTeleporterResolver());
        RegisterResolver(resolvers, new Resolvers.TeleporterRiftResolver());
        RegisterResolver(resolvers, new Resolvers.HermesShadeResolver());

        // 玩家货物
        RegisterResolver(resolvers, new Resolvers.PlayerCargoResolver());

        // 3. 构建 IL2CPP 指针查找缓存
        var resolverLookup = BuildResolverCache(resolvers);

        // 4. 初始化 Mapper 字典（基于 MapMarkerType）
        var mappers = new Dictionary<MapMarkerType, IComponentMapper>()
        {
            // 地形类
            { MapMarkerType.Beach, new Mappers.BeachMapper(view) },
            { MapMarkerType.River, new Mappers.RiverMapper(view) },
            
            // 建筑类
            { MapMarkerType.Castle, new Mappers.CastleMapper(view) },
            { MapMarkerType.Wall, new Mappers.WallMapper(view) },
            { MapMarkerType.Cabin, new Mappers.CabinMapper(view) },
            { MapMarkerType.Farmhouse, new Mappers.FarmhouseMapper(view) },
            { MapMarkerType.CitizenHouse, new Mappers.CitizenHousePayableMapper(view) },
            
            // 交互建筑
            { MapMarkerType.Lighthouse, new Mappers.LighthouseMapper(view) },
            { MapMarkerType.Mine, new Mappers.MineMapper(view) },
            { MapMarkerType.Quarry, new Mappers.QuarryMapper(view) },
            { MapMarkerType.PayableShop, new Mappers.PayableShopMapper(view) },
            { MapMarkerType.Chest, new Mappers.ChestMapper(view) },
            { MapMarkerType.GemChest, new Mappers.PayableGemChestMapper(view) },
            { MapMarkerType.BoatSummoningBell, new Mappers.BoatSummoningBellMapper(view) },
            { MapMarkerType.Wharf, new Mappers.WharfMapper(view) },
            { MapMarkerType.Teleporter, new Mappers.TeleporterMapper(view) },
            { MapMarkerType.TeleporterRift, new Mappers.TeleporterRiftMapper(view) },

            // 传送点
            { MapMarkerType.Portal, new Mappers.PortalMapper(view) },
            { MapMarkerType.TeleporterExit, new Mappers.TeleporterExitMapper(view) },
            
            // 雕像类
            { MapMarkerType.TimeStatue, new Mappers.TimeStatueMapper(view) },
            { MapMarkerType.UnlockNewRulerStatue, new Mappers.UnlockNewRulerStatueMapper(view) },
            { MapMarkerType.Statue, new Mappers.StatueMapper(view) },
            
            // 营地类
            { MapMarkerType.BeggarCamp, new Mappers.BeggarCampMapper(view) },
            { MapMarkerType.Campfire, new Mappers.CampfireMapper(view) },
            
            // 单位类
            { MapMarkerType.Player, new Mappers.PlayerMapper(view) },
            { MapMarkerType.Knight, new Mappers.KnightMapper(view) },
            { MapMarkerType.Beggar, new Mappers.BeggarMapper(view) },

            { MapMarkerType.Deer, new Mappers.DeerMapper(view) },
            
            // 坐骑类
            { MapMarkerType.Steed, new Mappers.SteedMapper(view) },
            { MapMarkerType.SteedSpawn, new Mappers.SteedSpawnMapper(view) },
            { MapMarkerType.DogSpawn, new Mappers.DogSpawnMapper(view) },
            { MapMarkerType.BoarSpawnGroup, new Mappers.BoarSpawnGroupMapper(view) },
            
            // 载具类
            { MapMarkerType.Boat, new Mappers.BoatMapper(view) },
            { MapMarkerType.BoatWreck, new Mappers.BoatWreckMapper(view) },
            
            // 障碍物
            { MapMarkerType.PayableBush, new Mappers.PayableBushMapper(view) },
            
            // 武器
            { MapMarkerType.Bomb, new Mappers.BombMapper(view) },
            
            // DLC 内容
            { MapMarkerType.HelPuzzleController, new Mappers.HelPuzzleControllerMapper(view) },
            { MapMarkerType.ThorPuzzleController, new Mappers.ThorPuzzleControllerMapper(view) },
            { MapMarkerType.HephaestusForge, new Mappers.HephaestusForgeMapper(view) },
            { MapMarkerType.PersephoneCage, new Mappers.PersephoneCageMapper(view) },
            { MapMarkerType.MerchantSpawner, new Mappers.MerchantSpawnerMapper(view) },
            { MapMarkerType.Tholos, new Mappers.TholosMapper(view) },
            { MapMarkerType.GodIdol, new Mappers.GodIdolMapper(view) },
            { MapMarkerType.HermesShade, new Mappers.HermesShadeMapper(view) },
        };

        // 5. 将初始化结果设置到 TopMapView
        view.SetResolvers(resolvers, resolverLookup);
        view.SetMappers(mappers);

        OverlayMapHolder.LogDebug($"Mapper system initialization completed: registered {resolvers.Count} types of component Resolvers and {mappers.Count} Mappers");
    }

    /// <summary>
    /// 构建 Resolver 的 IL2CPP 指针查找缓存。
    /// 将 System.Type 转换为 IntPtr 以提高查找性能。
    /// </summary>
    private static Dictionary<IntPtr, List<IMarkerResolver>> BuildResolverCache(
        Dictionary<Type, List<IMarkerResolver>> resolvers)
    {
        var resolverLookup = new Dictionary<IntPtr, List<IMarkerResolver>>();
        foreach (var kvp in resolvers)
        {
            var il2cppType = Il2CppInterop.Runtime.Il2CppType.From(kvp.Key);
            resolverLookup[il2cppType.Pointer] = kvp.Value;
        }
        return resolverLookup;
    }

    /// <summary>
    /// 注册 Resolver 到字典。
    /// 同一个游戏组件类型可以有多个 Resolver（按优先级执行）。
    /// </summary>
    private static void RegisterResolver(
        Dictionary<Type, List<IMarkerResolver>> resolvers,
        IMarkerResolver resolver)
    {
        if (!resolvers.ContainsKey(resolver.TargetComponentType))
            resolvers[resolver.TargetComponentType] = new List<IMarkerResolver>();

        resolvers[resolver.TargetComponentType].Add(resolver);
    }
}
