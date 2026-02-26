
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
#if IL2CPP
        var resolverLookup = BuildResolverCache(resolvers);
#else
        Dictionary<IntPtr, List<IMarkerResolver>> resolverLookup = null;
#endif

        // 4. 初始化 Mapper 字典（基于 MapMarkerType）
        var mappers = new Dictionary<MapMarkerType, IComponentMapper>();

        // 地形类
        RegisterMapper(mappers, new Mappers.BeachMapper(view));
        RegisterMapper(mappers, new Mappers.RiverMapper(view));

        // 建筑类
        RegisterMapper(mappers, new Mappers.CastleMapper(view));
        RegisterMapper(mappers, new Mappers.WallMapper(view));
        RegisterMapper(mappers, new Mappers.CabinMapper(view));
        RegisterMapper(mappers, new Mappers.FarmhouseMapper(view));
        RegisterMapper(mappers, new Mappers.CitizenHousePayableMapper(view));

        // 交互建筑
        RegisterMapper(mappers, new Mappers.LighthouseMapper(view));
        RegisterMapper(mappers, new Mappers.MineMapper(view));
        RegisterMapper(mappers, new Mappers.QuarryMapper(view));
        RegisterMapper(mappers, new Mappers.PayableShopMapper(view));
        RegisterMapper(mappers, new Mappers.ChestMapper(view));
        RegisterMapper(mappers, new Mappers.PayableGemChestMapper(view));
        RegisterMapper(mappers, new Mappers.BoatSummoningBellMapper(view));
        RegisterMapper(mappers, new Mappers.WharfMapper(view));
        RegisterMapper(mappers, new Mappers.TeleporterMapper(view));
        RegisterMapper(mappers, new Mappers.TeleporterRiftMapper(view));

        // 传送点
        RegisterMapper(mappers, new Mappers.PortalMapper(view));
        RegisterMapper(mappers, new Mappers.TeleporterExitMapper(view));

        // 雕像类
        RegisterMapper(mappers, new Mappers.TimeStatueMapper(view));
        RegisterMapper(mappers, new Mappers.UnlockNewRulerStatueMapper(view));
        RegisterMapper(mappers, new Mappers.StatueMapper(view));

        // 营地类
        RegisterMapper(mappers, new Mappers.BeggarCampMapper(view));
        RegisterMapper(mappers, new Mappers.CampfireMapper(view));

        // 单位类
        RegisterMapper(mappers, new Mappers.PlayerMapper(view));
        RegisterMapper(mappers, new Mappers.KnightMapper(view));
        RegisterMapper(mappers, new Mappers.BeggarMapper(view));
        RegisterMapper(mappers, new Mappers.DeerMapper(view));

        // 坐骑类
        RegisterMapper(mappers, new Mappers.SteedMapper(view));
        RegisterMapper(mappers, new Mappers.SteedSpawnMapper(view));
        RegisterMapper(mappers, new Mappers.DogSpawnMapper(view));
        RegisterMapper(mappers, new Mappers.BoarSpawnGroupMapper(view));

        // 载具类
        RegisterMapper(mappers, new Mappers.BoatMapper(view));
        RegisterMapper(mappers, new Mappers.BoatWreckMapper(view));

        // 障碍物
        RegisterMapper(mappers, new Mappers.PayableBushMapper(view));

        // 武器
        RegisterMapper(mappers, new Mappers.BombMapper(view));

        // DLC 内容
        RegisterMapper(mappers, new Mappers.HelPuzzleControllerMapper(view));
        RegisterMapper(mappers, new Mappers.ThorPuzzleControllerMapper(view));
        RegisterMapper(mappers, new Mappers.HephaestusForgeMapper(view));
        RegisterMapper(mappers, new Mappers.PersephoneCageMapper(view));
        RegisterMapper(mappers, new Mappers.MerchantSpawnerMapper(view));
        RegisterMapper(mappers, new Mappers.TholosMapper(view));
        RegisterMapper(mappers, new Mappers.GodIdolMapper(view));
        RegisterMapper(mappers, new Mappers.HermesShadeMapper(view));

        // 5. 将初始化结果设置到 TopMapView
        view.SetResolvers(resolvers, resolverLookup);
        view.SetMappers(mappers);

        OverlayMapHolder.LogDebug($"Mapper system initialization completed: registered {resolvers.Count} types of component Resolvers and {mappers.Count} Mappers");
    }

#if IL2CPP
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
#endif

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

    /// <summary>
    /// 注册 Mapper 到字典，key 由 Mapper 自身的 MarkerType 提供。
    /// </summary>
    private static void RegisterMapper(
        Dictionary<MapMarkerType, IComponentMapper> mappers,
        IComponentMapper mapper)
    {
        mappers[mapper.MarkerType] = mapper;
    }
}
