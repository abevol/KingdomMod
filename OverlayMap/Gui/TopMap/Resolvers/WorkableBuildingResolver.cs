using System;
using UnityEngine;
using KingdomMod.SharedLib;
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

            var prefabId = component.gameObject.GetComponent<PrefabID>();
            if (prefabId)
            {
                var gamePrefabId = (GamePrefabID)prefabId.prefabID;
                if (gamePrefabId == GamePrefabID.Lighthouse_Iron)
                    return MapMarkerType.Lighthouse;
            }

            // LogDebug($"Unrecognized WorkableBuilding {component.gameObject.name}, tag: {component.gameObject.tag}");
            return MapMarkerType.WorkableBuilding;
        }
    }
}
