using System;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Resolvers
{
    /// <summary>
    /// Scaffolding 组件解析器。
    /// 通过 Tag 或 PrefabID 区分不同的建造中的对象（墙体、灯塔、铁矿、石矿等）。
    /// </summary>
    public class ScaffoldingResolver : IMarkerResolver
    {
        public ResolverType ResolverType => ResolverType.Scaffolding;

        public Type TargetComponentType => typeof(Scaffolding);

        public MapMarkerType? Resolve(Component component)
        {
            var scaffolding = component.Cast<Scaffolding>();
            if (scaffolding == null) return null;
            var building = scaffolding.Building;
            if (building == null) return null;

            // 根据游戏对象标签返回对应的标记类型
            if (building.gameObject.tag == Tags.Wall)
                return MapMarkerType.Wall;

            var prefabId = building.gameObject.GetComponent<PrefabID>();
            if (prefabId == null) return null;

            var gamePrefabId = (GamePrefabID)prefabId.prefabID;

            // 根据 PrefabID 返回对应的标记类型
            return gamePrefabId switch
            {
                // 灯塔系列
                GamePrefabID.Lighthouse_Wood or
                GamePrefabID.Lighthouse_Stone or
                GamePrefabID.Lighthouse_Iron => MapMarkerType.Lighthouse,

                // 石矿
                GamePrefabID.Quarry => MapMarkerType.Quarry,

                // 铁矿
                GamePrefabID.Mine => MapMarkerType.Mine,

                // 传送阵
                GamePrefabID.Teleporter => MapMarkerType.Teleporter,

                // 未识别的类型
                _ => MapMarkerType.Scaffolding
            };
        }
    }
}
