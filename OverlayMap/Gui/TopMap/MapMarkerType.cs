namespace KingdomMod.OverlayMap.Gui.TopMap
{
    /// <summary>
    /// 地图标记类型枚举。
    /// 这是模组定义的逻辑类型，与游戏代码的具体实现解耦。
    /// 通过引入此枚举，可以为没有专属游戏对象类型的标记创建独立 Mapper。
    /// </summary>
    public enum MapMarkerType
    {
        // ===== 地形类 (Terrain) =====
        /// <summary>海滩/码头</summary>
        Beach,
        
        /// <summary>河流</summary>
        River,

        // ===== 建筑类 (Buildings) =====
        /// <summary>城堡</summary>
        Castle,
        
        /// <summary>墙体</summary>
        Wall,
        
        /// <summary>脚手架（建造中）</summary>
        Scaffolding,

        /// <summary>可建造建筑（通用）</summary>
        WorkableBuilding,

        /// <summary>小屋</summary>
        Cabin,
        
        /// <summary>农舍</summary>
        Farmhouse,
        
        /// <summary>市民住宅（可付费招募）</summary>
        CitizenHouse,

        // ===== 可交互建筑 (Interactive Buildings) =====
        /// <summary>灯塔</summary>
        Lighthouse,
        
        /// <summary>铁矿</summary>
        Mine,
        
        /// <summary>石矿</summary>
        Quarry,
        
        /// <summary>商店</summary>
        PayableShop,
        
        /// <summary>宝箱</summary>
        Chest,
        
        /// <summary>宝石宝箱（可付费）</summary>
        GemChest,

        // ===== 传送点/特殊建筑 (Portal &amp; Special) =====
        /// <summary>传送门</summary>
        Portal,
        
        /// <summary>传送门出口</summary>
        TeleporterExit,

        /// <summary>传送阵</summary>
        Teleporter,

        /// <summary>船只召唤铃</summary>
        BoatSummoningBell,

        // ===== 雕像类 (Statues) =====
        /// <summary>普通雕像</summary>
        Statue,
        
        /// <summary>时间雕像</summary>
        TimeStatue,
        
        /// <summary>解锁新统治者雕像</summary>
        UnlockNewRulerStatue,

        // ===== 营地类 (Camps) =====
        /// <summary>乞丐营地</summary>
        BeggarCamp,
        
        /// <summary>篝火</summary>
        Campfire,

        /// <summary>商人刷新点</summary>
        MerchantSpawner,

        // ===== 单位类 (Units) =====
        /// <summary>玩家</summary>
        Player,
        
        /// <summary>乞丐</summary>
        Beggar,
        
        /// <summary>鹿</summary>
        Deer,
        
        /// <summary>敌人（怪物）</summary>
        Enemy,

        /// <summary>骑士</summary>
        Knight,

        // ===== 坐骑类 (Mounts) =====
        /// <summary>坐骑</summary>
        Steed,
        
        /// <summary>坐骑刷新点</summary>
        SteedSpawn,
        
        /// <summary>狗刷新点</summary>
        DogSpawn,
        
        /// <summary>野猪刷新群</summary>
        BoarSpawnGroup,

        // ===== 载具类 (Vehicles) =====
        /// <summary>船只</summary>
        Boat,

        /// <summary>船只残骸</summary>
        BoatWreck,

        /// <summary>船坞</summary>
        Wharf,

        // ===== 障碍物/可付费对象 (Obstacles &amp; Payables) =====
        /// <summary>可付费阻挡物</summary>
        PayableBlocker,
        
        /// <summary>灌木</summary>
        PayableBush,
        
        /// <summary>可付费升级物（通用）</summary>
        PayableUpgrade,

        // ===== 武器/道具 (Weapons &amp; Items) =====
        /// <summary>炸弹</summary>
        Bomb,

        // ===== DLC 相关 (DLC Content) =====
        /// <summary>赫尔谜题控制器（Norse Lands DLC）</summary>
        HelPuzzleController,
        
        /// <summary>托尔谜题控制器（Norse Lands DLC）</summary>
        ThorPuzzleController,

        /// <summary>神殿（Olympus DLC）</summary>
        Tholos,

        /// <summary>赫菲斯托斯熔炉（Olympus DLC）</summary>
        HephaestusForge,
        
        /// <summary>珀尔塞福涅牢笼（Olympus DLC）</summary>
        PersephoneCage,

        /// <summary>玩家货物（Olympus DLC）</summary>
        PlayerCargo,

        /// <summary>神像（Olympus DLC）</summary>
        GodIdol,
    }
}
