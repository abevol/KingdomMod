using KingdomMod.SharedLib;
using System;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Resolvers
{
    /// <summary>
    /// PayableBlocker 组件解析器。
    /// </summary>
    public class PayableBlockerResolver : IMarkerResolver
    {
        public ResolverType ResolverType => ResolverType.PayableBlocker;

        public Type TargetComponentType => typeof(PayableBlocker);

        public MapMarkerType? Resolve(Component component)
        {
            var obj = component.Cast<PayableBlocker>();
            if (component.GetComponent<HelPuzzleController>()) return MapMarkerType.HelPuzzleController;
            if (component.GetComponent<ThorPuzzleController>()) return MapMarkerType.ThorPuzzleController;

            var prefabId = obj.gameObject.GetComponent<PrefabID>();
            if (prefabId == null) return null;
            var gamePrefabId = (GamePrefabID)prefabId.prefabID;
            // 根据 PrefabID 返回对应的标记类型
            return gamePrefabId switch
            {
                GamePrefabID.Quarry => MapMarkerType.Quarry,
                GamePrefabID.Mine => MapMarkerType.Mine,
                _ => MapMarkerType.PayableBlocker
            };
        }
    }
}
