namespace KingdomMod.OverlayMap.Gui.TopMap
{
    /// <summary>
    /// 通知器类型枚举。
    /// 用于标识不同类型的游戏对象通知器，以便在地图上进行分类处理和显示。
    /// </summary>
    public enum NotifierType
    {
        /// <summary>未知类型</summary>
        Unknown,

        /// <summary>标准类型</summary>
        Standard,

        /// <summary>可付费阻挡物</summary>
        PayableBlocker,

        /// <summary>可付费对象</summary>
        Payable,

        /// <summary>玩家货物</summary>
        PlayerCargo,

        /// <summary>脚手架（建造中）</summary>
        Scaffolding,

        /// <summary>可工作建筑</summary>
        WorkableBuilding,
    }
}
