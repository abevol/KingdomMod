using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BoatWreckMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.BoatWreck;

        public void Map(Component component)
        {
            var payableUpgrade = component.Cast<PayableUpgrade>();
            var prefabId = payableUpgrade.gameObject.GetComponent<PrefabID>();
            if (prefabId == null) return;
            var gamePrefabId = (GamePrefabID)prefabId.prefabID;
            if (gamePrefabId == GamePrefabID.Wreck)
            {
                view.TryAddMapMarker(component, MarkerStyle.BoatWreck.Color, MarkerStyle.BoatWreck.Sign, Strings.BoatWreck,
                    comp => comp.Cast<PayableUpgrade>().Price);
            }
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }
}