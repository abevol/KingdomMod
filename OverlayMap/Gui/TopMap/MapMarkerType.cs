namespace KingdomMod.OverlayMap.Gui.TopMap
{
    /// &lt;summary&gt;
    /// 地图标记类型枚举。
    /// 这是模组定义的逻辑类型，与游戏代码的具体实现解耦。
    /// 通过引入此枚举，可以为没有专属游戏对象类型的标记创建独立 Mapper。
    /// &lt;/summary&gt;
    public enum MapMarkerType
    {
        // ===== 地形类 (Terrain) =====
        /// &lt;summary&gt;海滩/码头&lt;/summary&gt;
        Beach,
        
        /// &lt;summary&gt;河流&lt;/summary&gt;
        River,

        // ===== 建筑类 (Buildings) =====
        /// &lt;summary&gt;城堡&lt;/summary&gt;
        Castle,
        
        /// &lt;summary&gt;墙体&lt;/summary&gt;
        Wall,
        
        /// &lt;summary&gt;脚手架（建造中）&lt;/summary&gt;
        Scaffolding,
        
        /// &lt;summary&gt;小屋&lt;/summary&gt;
        Cabin,
        
        /// &lt;summary&gt;农舍&lt;/summary&gt;
        Farmhouse,
        
        /// &lt;summary&gt;市民住宅（可付费招募）&lt;/summary&gt;
        CitizenHouse,

        // ===== 可交互建筑 (Interactive Buildings) =====
        /// &lt;summary&gt;灯塔&lt;/summary&gt;
        Lighthouse,
        
        /// &lt;summary&gt;矿井&lt;/summary&gt;
        Mine,
        
        /// &lt;summary&gt;采石场&lt;/summary&gt;
        Quarry,
        
        /// &lt;summary&gt;商店&lt;/summary&gt;
        Shop,
        
        /// &lt;summary&gt;宝箱&lt;/summary&gt;
        Chest,
        
        /// &lt;summary&gt;宝石宝箱（可付费）&lt;/summary&gt;
        GemChest,

        // ===== 传送点/特殊建筑 (Portal &amp; Special) =====
        /// &lt;summary&gt;传送门&lt;/summary&gt;
        Portal,
        
        /// &lt;summary&gt;传送门出口&lt;/summary&gt;
        TeleporterExit,
        
        /// &lt;summary&gt;船只召唤铃&lt;/summary&gt;
        BoatSummoningBell,

        // ===== 雕像类 (Statues) =====
        /// &lt;summary&gt;普通雕像&lt;/summary&gt;
        Statue,
        
        /// &lt;summary&gt;时间雕像&lt;/summary&gt;
        TimeStatue,
        
        /// &lt;summary&gt;解锁新统治者雕像&lt;/summary&gt;
        UnlockNewRulerStatue,

        // ===== 营地类 (Camps) =====
        /// &lt;summary&gt;乞丐营地&lt;/summary&gt;
        BeggarCamp,
        
        /// &lt;summary&gt;篝火&lt;/summary&gt;
        Campfire,

        // ===== 单位类 (Units) =====
        /// &lt;summary&gt;玩家&lt;/summary&gt;
        Player,
        
        /// &lt;summary&gt;乞丐&lt;/summary&gt;
        Beggar,
        
        /// &lt;summary&gt;鹿&lt;/summary&gt;
        Deer,
        
        /// &lt;summary&gt;敌人（怪物）&lt;/summary&gt;
        Enemy,

        /// <summary>骑士</summary>
        Knight,

        // ===== 坐骑类 (Mounts) =====
        /// &lt;summary&gt;坐骑&lt;/summary&gt;
        Steed,
        
        /// &lt;summary&gt;坐骑刷新点&lt;/summary&gt;
        SteedSpawn,
        
        /// &lt;summary&gt;狗刷新点&lt;/summary&gt;
        DogSpawn,
        
        /// &lt;summary&gt;野猪刷新群&lt;/summary&gt;
        BoarSpawnGroup,

        // ===== 载具类 (Vehicles) =====
        /// &lt;summary&gt;船只&lt;/summary&gt;
        Boat,

        /// &lt;summary&gt;船只残骸&lt;/summary&gt;
        BoatWreck,

        // ===== 障碍物/可付费对象 (Obstacles &amp; Payables) =====
        /// &lt;summary&gt;可付费阻挡物&lt;/summary&gt;
        PayableBlocker,
        
        /// &lt;summary&gt;可付费灌木&lt;/summary&gt;
        PayableBush,
        
        /// &lt;summary&gt;可付费升级物（通用）&lt;/summary&gt;
        PayableUpgrade,

        // ===== 武器/道具 (Weapons &amp; Items) =====
        /// &lt;summary&gt;炸弹&lt;/summary&gt;
        Bomb,

        // ===== DLC 相关 (DLC Content) =====
        /// &lt;summary&gt;赫尔谜题控制器（Norse Lands DLC）&lt;/summary&gt;
        HelPuzzleController,
        
        /// &lt;summary&gt;托尔谜题控制器（Norse Lands DLC）&lt;/summary&gt;
        ThorPuzzleController,
        
        /// &lt;summary&gt;赫菲斯托斯熔炉（Olympus DLC）&lt;/summary&gt;
        HephaestusForge,
        
        /// &lt;summary&gt;珀尔塞福涅牢笼（Olympus DLC）&lt;/summary&gt;
        PersephoneCage,
        
        /// &lt;summary&gt;商人刷新点&lt;/summary&gt;
        MerchantSpawner,
    }
}
