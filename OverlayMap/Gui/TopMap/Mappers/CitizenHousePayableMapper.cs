using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class CitizenHousePayableMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.CitizenHouse;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.CitizenHouse.Building.Color, MarkerStyle.CitizenHouse.Sign, Strings.CitizenHouse,
                comp => comp.Cast<CitizenHousePayable>()._numberOfAvailableCitizens,
                comp => comp.gameObject.activeSelf ? MarkerStyle.CitizenHouse.Color : MarkerStyle.CitizenHouse.Building.Color);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(CitizenHousePayable))]
    public static class CitizenHousePayableNotifier
    {
        [HarmonyPatch(nameof(CitizenHousePayable.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(CitizenHousePayable __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(CitizenHousePayable.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(CitizenHousePayable __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
