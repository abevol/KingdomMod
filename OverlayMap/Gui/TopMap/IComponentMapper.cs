using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    /// <summary>
    /// 组件映射器接口。
    /// 负责将游戏组件映射为地图标记。
    /// </summary>
    public interface IComponentMapper
    {
        /// <summary>
        /// 该 Mapper 负责的标记类型（可选）。
        /// 新架构中，Mapper 应该明确声明自己处理的标记类型。
        /// </summary>
        MapMarkerType? MarkerType => null;

        /// <summary>
        /// 获取需要映射的组件列表（旧方法，逐步废弃）。
        /// </summary>
        Component[] GetComponents() => [];

        /// <summary>
        /// 将组件映射为地图标记。
        /// </summary>
        /// <param name="component">游戏组件</param>
        void Map(Component component) { }

        /// <summary>
        /// 将组件映射为地图标记。
        /// </summary>
        /// <param name="component">游戏组件</param>
        /// <param name="notifierType">通知类型</param>
        /// <param name="resolverType">解析器类型</param>
        void Map(Component component, NotifierType notifierType, ResolverType resolverType) => Map(component);
    }
}
