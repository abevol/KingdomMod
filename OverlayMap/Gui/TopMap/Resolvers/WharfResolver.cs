using System;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Resolvers
{
    /// <summary>
    /// 船坞解析器。
    /// Wharf 继承自 Payable，通过 PrefabID 识别。
    /// </summary>
    public class WharfResolver : IMarkerResolver
    {
        public Type TargetComponentType => typeof(Wharf);

        public MapMarkerType? Resolve(Component component)
        {
            var prefabId = component.gameObject.GetComponent<PrefabID>();
            if (prefabId == null) return null;

            var gamePrefabId = (GamePrefabID)prefabId.prefabID;

            // 只识别 Wharf
            return gamePrefabId == GamePrefabID.Wharf ? MapMarkerType.Wharf : null;
        }
    }
}
