using System;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    /// &lt;summary&gt;
    /// 标记解析器接口。
    /// 负责将游戏组件（Component）识别为具体的地图标记类型（MapMarkerType）。
    /// 通过策略模式实现解耦，使得识别逻辑独立于地图渲染逻辑。
    /// &lt;/summary&gt;
    public interface IMarkerResolver
    {
        /// &lt;summary&gt;
        /// 该解析器关注的游戏组件类型。
        /// &lt;/summary&gt;
        Type TargetComponentType { get; }

        /// &lt;summary&gt;
        /// 尝试解析组件的标记类型。
        /// &lt;/summary&gt;
        /// &lt;param name="component"&gt;游戏组件&lt;/param&gt;
        /// &lt;returns&gt;如果识别成功，返回对应的 MapMarkerType；否则返回 null&lt;/returns&gt;
        MapMarkerType? Resolve(Component component);
    }
}
