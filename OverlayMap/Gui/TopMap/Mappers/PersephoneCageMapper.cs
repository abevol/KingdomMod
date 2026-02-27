using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PersephoneCageMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.PersephoneCage;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, null, MarkerStyle.PersephoneCage.Sign, Strings.HermitPersephone, null,
                comp => PersephoneCage.State.IsPersephoneLocked(comp.Cast<PersephoneCage>()._fsm.Current)
                    ? MarkerStyle.PersephoneCage.Locked.Color
                    : MarkerStyle.PersephoneCage.Unlocked.Color);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(PersephoneCage))]
    public static class PersephoneCageNotifier
    {
        [HarmonyPatch(nameof(PersephoneCage.Awake))]
        [HarmonyPostfix]
        public static void Awake(PersephoneCage __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(PersephoneCage.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(PersephoneCage __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
