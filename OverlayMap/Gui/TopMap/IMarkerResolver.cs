using System;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    /// <summary>
    /// 标记解析器接口。
    /// 负责将游戏组件（Component）识别为具体的地图标记类型（MapMarkerType）。
    /// 通过策略模式实现解耦，使得识别逻辑独立于地图渲染逻辑。
    /// </summary>
    public interface IMarkerResolver
    {
        /// <summary>
        /// 该解析器的类型
        /// </summary>
        ResolverType ResolverType { get; }

        /// <summary>
        /// 该解析器关注的游戏组件类型。
        /// </summary>
        Type TargetComponentType { get; }

        /// <summary>
        /// 尝试解析组件的标记类型。
        /// </summary>
        /// <param name="component">游戏组件</param>
        /// <returns>如果识别成功，返回对应的 MapMarkerType；否则返回 null</returns>
        MapMarkerType? Resolve(Component component);
    }
}
