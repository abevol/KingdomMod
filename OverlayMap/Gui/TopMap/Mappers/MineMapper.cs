using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 矿井标记映射器。
    /// </summary>
    public class MineMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Mine;

        public void Map(Component component)
        {
            var payableUpgrade = component.Cast<PayableUpgrade>();
            var prefabId = payableUpgrade.gameObject.GetComponent<PrefabID>();
            if (prefabId == null) return;

            var gamePrefabId = (GamePrefabID)prefabId.prefabID;

            switch (gamePrefabId)
            {
                case GamePrefabID.Mine_undeveloped:
                    view.TryAddMapMarker(component, MarkerStyle.Mine.Locked.Color, MarkerStyle.Mine.Sign, Strings.Mine,
                        comp => comp.Cast<PayableUpgrade>().Price);
                    break;

                case GamePrefabID.Mine:
                    view.TryAddMapMarker(component, null, MarkerStyle.Mine.Sign, Strings.Mine, null,
                        comp => comp.gameObject.activeSelf ? MarkerStyle.Mine.Unlocked.Color : MarkerStyle.Mine.Building.Color);
                    break;
            }
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }
}