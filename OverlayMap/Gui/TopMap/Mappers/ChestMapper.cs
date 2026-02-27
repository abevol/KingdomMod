using HarmonyLib;
using UnityEngine;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class ChestMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Chest;

        public void Map(Component component)
        {
            var obj = component.Cast<Chest>();
            var isGem = obj.currencyType == CurrencyType.Gems;
            view.TryAddMapMarker(component,
                isGem ? MarkerStyle.GemChest.Color : MarkerStyle.Chest.Color,
                isGem ? MarkerStyle.GemChest.Sign : MarkerStyle.Chest.Sign,
                isGem ? Strings.GemChest : Strings.Chest,
                comp => comp.Cast<Chest>().currencyAmount,
                null, comp => comp.Cast<Chest>().currencyAmount != 0);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Chest))]
    public static class ChestNotifier
    {
        [HarmonyPatch(nameof(Chest.Awake))]
        [HarmonyPostfix]
        public static void OnEnable(Chest __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Chest.OnDestroy))]
        [HarmonyPrefix]
        public static void OnDisable(Chest __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
