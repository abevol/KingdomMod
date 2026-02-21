using System;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Resolvers
{
    /// <summary>
    /// WorkableBuilding 组件解析器。
    /// </summary>
    public class WorkableBuildingResolver : IMarkerResolver
    {
        public ResolverType ResolverType => ResolverType.WorkableBuilding;

        public Type TargetComponentType => typeof(WorkableBuilding);

        public MapMarkerType? Resolve(Component component)
        {
            // 根据游戏对象标签返回对应的标记类型
            if (component.gameObject.tag == Tags.Tholos)
                return MapMarkerType.Tholos;

            // LogDebug($"Unrecognized WorkableBuilding {component.gameObject.name}, tag: {component.gameObject.tag}");
            return MapMarkerType.WorkableBuilding;
        }
    }
}
