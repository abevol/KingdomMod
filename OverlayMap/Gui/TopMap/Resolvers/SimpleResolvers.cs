using System;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Resolvers
{
    /// <summary>
    /// 简单解析器基类。
    /// 用于处理一对一映射的情况（一种游戏组件类型对应一种地图标记类型）。
    /// </summary>
    public abstract class SimpleResolver : IMarkerResolver
    {
        private readonly Type _targetType;
        private readonly MapMarkerType _markerType;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="targetType">目标组件类型</param>
        /// <param name="markerType">对应的标记类型</param>
        protected SimpleResolver(Type targetType, MapMarkerType markerType)
        {
            _targetType = targetType;
            _markerType = markerType;
        }

        public ResolverType ResolverType => ResolverType.Standard;

        public Type TargetComponentType => _targetType;

        public virtual MapMarkerType? Resolve(Component component)
        {
            // 简单解析器直接返回预设的标记类型
            return _markerType;
        }
    }

    // ===== 具体的简单解析器实现 =====

    /// <summary>城堡解析器</summary>
    public class CastleResolver : SimpleResolver
    {
        public CastleResolver() : base(typeof(Castle), MapMarkerType.Castle) { }
    }

    /// <summary>海滩解析器</summary>
    public class BeachResolver : SimpleResolver
    {
        public BeachResolver() : base(typeof(Beach), MapMarkerType.Beach) { }
    }

    /// <summary>河流解析器</summary>
    public class RiverResolver : SimpleResolver
    {
        public RiverResolver() : base(typeof(River), MapMarkerType.River) { }
    }

    /// <summary>传送门解析器</summary>
    public class PortalResolver : SimpleResolver
    {
        public PortalResolver() : base(typeof(Portal), MapMarkerType.Portal) { }
    }

    /// <summary>传送门出口解析器</summary>
    public class TeleporterExitResolver : SimpleResolver
    {
        public TeleporterExitResolver() : base(typeof(TeleporterExit), MapMarkerType.TeleporterExit) { }
    }

    /// <summary>小屋解析器</summary>
    public class CabinResolver : SimpleResolver
    {
        public CabinResolver() : base(typeof(Cabin), MapMarkerType.Cabin) { }
    }

    /// <summary>农舍解析器</summary>
    public class FarmhouseResolver : SimpleResolver
    {
        public FarmhouseResolver() : base(typeof(Farmhouse), MapMarkerType.Farmhouse) { }
    }

    /// <summary>市民住宅解析器</summary>
    public class CitizenHousePayableResolver : SimpleResolver
    {
        public CitizenHousePayableResolver() : base(typeof(CitizenHousePayable), MapMarkerType.CitizenHouse) { }
    }

    /// <summary>宝箱解析器</summary>
    public class ChestResolver : SimpleResolver
    {
        public ChestResolver() : base(typeof(Chest), MapMarkerType.Chest) { }
    }

    /// <summary>宝石宝箱解析器</summary>
    public class PayableGemChestResolver : SimpleResolver
    {
        public PayableGemChestResolver() : base(typeof(PayableGemChest), MapMarkerType.GemChest) { }
    }

    /// <summary>船只召唤铃解析器</summary>
    public class BoatSummoningBellResolver : SimpleResolver
    {
        public BoatSummoningBellResolver() : base(typeof(BoatSummoningBell), MapMarkerType.BoatSummoningBell) { }
    }

    /// <summary>时间雕像解析器</summary>
    public class TimeStatueResolver : SimpleResolver
    {
        public TimeStatueResolver() : base(typeof(TimeStatue), MapMarkerType.TimeStatue) { }
    }

    /// <summary>解锁新统治者雕像解析器</summary>
    public class UnlockNewRulerStatueResolver : SimpleResolver
    {
        public UnlockNewRulerStatueResolver() : base(typeof(UnlockNewRulerStatue), MapMarkerType.UnlockNewRulerStatue) { }
    }

    /// <summary>普通雕像解析器</summary>
    public class StatueResolver : SimpleResolver
    {
        public StatueResolver() : base(typeof(Statue), MapMarkerType.Statue) { }
    }

    /// <summary>乞丐营地解析器</summary>
    public class BeggarCampResolver : SimpleResolver
    {
        public BeggarCampResolver() : base(typeof(BeggarCamp), MapMarkerType.BeggarCamp) { }
    }

    /// <summary>篝火解析器</summary>
    public class CampfireResolver : SimpleResolver
    {
        public CampfireResolver() : base(typeof(Campfire), MapMarkerType.Campfire) { }
    }

    /// <summary>玩家解析器</summary>
    public class PlayerResolver : SimpleResolver
    {
        public PlayerResolver() : base(typeof(Player), MapMarkerType.Player) { }
    }

    /// <summary>骑士解析器</summary>
    public class KnightResolver : SimpleResolver
    {
        public KnightResolver() : base(typeof(Knight), MapMarkerType.Knight) { }
    }

    /// <summary>乞丐解析器</summary>
    public class BeggarResolver : SimpleResolver
    {
        public BeggarResolver() : base(typeof(Beggar), MapMarkerType.Beggar) { }
    }

    /// <summary>鹿解析器</summary>
    public class DeerResolver : SimpleResolver
    {
        public DeerResolver() : base(typeof(Deer), MapMarkerType.Deer) { }
    }

    /// <summary>坐骑解析器</summary>
    public class SteedResolver : SimpleResolver
    {
        public SteedResolver() : base(typeof(Steed), MapMarkerType.Steed) { }
    }

    /// <summary>坐骑刷新点解析器</summary>
    public class SteedSpawnResolver : SimpleResolver
    {
        public SteedSpawnResolver() : base(typeof(SteedSpawn), MapMarkerType.SteedSpawn) { }
    }

    /// <summary>狗刷新点解析器</summary>
    public class DogSpawnResolver : SimpleResolver
    {
        public DogSpawnResolver() : base(typeof(DogSpawn), MapMarkerType.DogSpawn) { }
    }

    /// <summary>野猪刷新群解析器</summary>
    public class BoarSpawnGroupResolver : SimpleResolver
    {
        public BoarSpawnGroupResolver() : base(typeof(BoarSpawnGroup), MapMarkerType.BoarSpawnGroup) { }
    }

    /// <summary>船只解析器</summary>
    public class BoatResolver : SimpleResolver
    {
        public BoatResolver() : base(typeof(Boat), MapMarkerType.Boat) { }
    }

    /// <summary>船坞解析器</summary>
    public class WharfResolver : SimpleResolver
    {
        public WharfResolver() : base(typeof(Wharf), MapMarkerType.Wharf) { }
    }

    /// <summary>可付费商店解析器</summary>
    public class PayableShopResolver : SimpleResolver
    {
        public PayableShopResolver() : base(typeof(PayableShop), MapMarkerType.PayableShop) { }
    }

    /// <summary>可付费灌木解析器</summary>
    public class PayableBushResolver : SimpleResolver
    {
        public PayableBushResolver() : base(typeof(PayableBush), MapMarkerType.PayableBush) { }
    }

    /// <summary>炸弹解析器</summary>
    public class BombResolver : SimpleResolver
    {
        public BombResolver() : base(typeof(Bomb), MapMarkerType.Bomb) { }
    }

    /// <summary>赫尔谜题控制器解析器</summary>
    public class HelPuzzleControllerResolver : SimpleResolver
    {
        public HelPuzzleControllerResolver() : base(typeof(HelPuzzleController), MapMarkerType.HelPuzzleController) { }
    }

    /// <summary>托尔谜题控制器解析器</summary>
    public class ThorPuzzleControllerResolver : SimpleResolver
    {
        public ThorPuzzleControllerResolver() : base(typeof(ThorPuzzleController), MapMarkerType.ThorPuzzleController) { }
    }

    /// <summary>赫菲斯托斯熔炉解析器</summary>
    public class HephaestusForgeResolver : SimpleResolver
    {
        public HephaestusForgeResolver() : base(typeof(HephaestusForge), MapMarkerType.HephaestusForge) { }
    }

    /// <summary>珀尔塞福涅牢笼解析器</summary>
    public class PersephoneCageResolver : SimpleResolver
    {
        public PersephoneCageResolver() : base(typeof(PersephoneCage), MapMarkerType.PersephoneCage) { }
    }
    
    /// <summary>商人刷新点解析器</summary>
    public class MerchantSpawnerResolver : SimpleResolver
    {
        public MerchantSpawnerResolver() : base(typeof(MerchantSpawner), MapMarkerType.MerchantSpawner) { }
    }

    /// <summary>灯塔解析器</summary>
    public class LighthouseResolver : SimpleResolver
    {
        public LighthouseResolver() : base(typeof(Lighthouse), MapMarkerType.Lighthouse) { }
    }

    /// <summary>传送阵解析器</summary>
    public class PayableTeleporterResolver : SimpleResolver
    {
        public PayableTeleporterResolver() : base(typeof(PayableTeleporter), MapMarkerType.Teleporter) { }
    }

    /// <summary>传送阵裂隙解析器</summary>
    public class TeleporterRiftResolver : SimpleResolver
    {
        public TeleporterRiftResolver() : base(typeof(TeleporterRift), MapMarkerType.TeleporterRift) { }
    }
}

