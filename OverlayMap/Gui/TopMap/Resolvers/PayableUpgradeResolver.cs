using System;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Resolvers
{
    /// <summary>
    /// PayableUpgrade 组件解析器。
    /// 通过 PrefabID 区分不同的可付费升级对象（墙体、灯塔、矿井、采石场等）。
    /// </summary>
    public class PayableUpgradeResolver : IMarkerResolver
    {
        public Type TargetComponentType => typeof(PayableUpgrade);

        public MapMarkerType? Resolve(Component component)
        {
            var payableUpgrade = component.Cast<PayableUpgrade>();
            if (payableUpgrade == null) return null;

            var prefabId = payableUpgrade.gameObject.GetComponent<PrefabID>();
            if (prefabId == null) return null;

            var gamePrefabId = (GamePrefabID)prefabId.prefabID;

            // 根据 PrefabID 返回对应的标记类型
            return gamePrefabId switch
            {
                // 墙体系列
                GamePrefabID.Wall0 or
                    GamePrefabID.Wall1 or
                    GamePrefabID.Wall2 or
                    GamePrefabID.Wall3 or
                    GamePrefabID.Wall4 or
                    GamePrefabID.Wall5 or
                    GamePrefabID.Wall1_Wreck or
                    GamePrefabID.Wall2_Wreck or
                    GamePrefabID.Wall3_Wreck or
                    GamePrefabID.Wall4_Wreck or
                    GamePrefabID.Wall5_Wreck => MapMarkerType.Wall,

                // 灯塔系列
                GamePrefabID.Lighthouse_undeveloped or
                    GamePrefabID.Lighthouse_Wood or
                    GamePrefabID.Lighthouse_Stone or
                    GamePrefabID.Lighthouse_Iron => MapMarkerType.Lighthouse,

                // 采石场系列
                GamePrefabID.Quarry_undeveloped or
                    GamePrefabID.Quarry => MapMarkerType.Quarry,

                // 矿井系列
                GamePrefabID.Mine_undeveloped or
                    GamePrefabID.Mine => MapMarkerType.Mine,

                // 悬崖传送门
                GamePrefabID.Cliff_Portal => MapMarkerType.Portal,

                // 船只残骸
                GamePrefabID.Wreck => MapMarkerType.Boat,

                // 未识别的类型
                _ => null
            };
        }
    }
}