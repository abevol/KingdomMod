using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using System.Linq;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class DeerMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Deer;

        public Component[] GetComponents()
        {
            return GameExtensions.FindObjectsWithTagOfType<Deer>(Tags.Wildlife).Cast<Component>().ToArray();
        }

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Deer.Color, MarkerStyle.Deer.Sign, Strings.Deer, null,
                comp =>
            {
                var state = comp.Cast<Deer>()._fsm.Current;
                return state == 5 ? MarkerStyle.DeerFollowing.Color : MarkerStyle.Deer.Color;
            }, comp => comp.gameObject.activeSelf && !comp.Cast<Deer>()._damageable.isDead, MarkerRow.Movable);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Deer))]
    public static class DeerNotifier
    {
        [HarmonyPatch(nameof(Deer.Start))]
        [HarmonyPostfix]
        public static void Start(Deer __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Deer.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Deer __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
