using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PortalMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Portal;

        public void Map(Component component)
        {
            var obj = component.Cast<Portal>();
            switch (obj.type)
            {
                case Portal.Type.Regular:
                    view.TryAddMapMarker(component, MarkerStyle.Portal.Color, MarkerStyle.Portal.Sign, Strings.Portal, null, null,
                        comp => comp.gameObject.activeSelf && !comp.Cast<Portal>()._damageable.isDead);
                    break;
                case Portal.Type.Cliff:
                    view.TryAddMapMarker(component, MarkerStyle.PortalCliff.Color, MarkerStyle.PortalCliff.Sign, Strings.PortalCliff, null,
                        (comp) => comp.Cast<Portal>().state switch
                    {
                        Portal.State.Destroyed => MarkerStyle.PortalCliff.Destroyed.Color,
                        Portal.State.Rebuilding => MarkerStyle.PortalCliff.Rebuilding.Color,
                        _ => MarkerStyle.PortalCliff.Color
                    });
                    break;
                case Portal.Type.Dock:
                    view.TryAddMapMarker(component, MarkerStyle.PortalDock.Color, MarkerStyle.PortalDock.Sign, Strings.PortalDock);
                    break;
            }
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Portal))]
    public static class PortalNotifier
    {
        [HarmonyPatch(nameof(Portal.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(Portal __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Portal.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Portal __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
