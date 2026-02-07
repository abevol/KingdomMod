using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 采石场标记映射器。
    /// </summary>
    public class QuarryMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Quarry;

        public void Map(Component component)
        {
            var payableUpgrade = component.Cast<PayableUpgrade>();
            var prefabId = payableUpgrade.gameObject.GetComponent<PrefabID>();
            if (prefabId == null) return;

            var gamePrefabId = (GamePrefabID)prefabId.prefabID;

            switch (gamePrefabId)
            {
                case GamePrefabID.Quarry_undeveloped:
                    view.TryAddMapMarker(component, MarkerStyle.Quarry.Locked.Color, MarkerStyle.Quarry.Sign, Strings.Quarry,
                        comp => comp.Cast<PayableUpgrade>().Price);
                    break;

                case GamePrefabID.Quarry:
                    view.TryAddMapMarker(component, null, MarkerStyle.Quarry.Sign, Strings.Quarry, null,
                        comp => comp.gameObject.activeSelf ? MarkerStyle.Quarry.Unlocked.Color : MarkerStyle.Quarry.Building.Color);
                    break;
            }
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }
}