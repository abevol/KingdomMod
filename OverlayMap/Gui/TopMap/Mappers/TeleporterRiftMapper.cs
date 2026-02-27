using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 传送阵裂隙标记映射器。
    /// 使用 OnEnable/OnDisable Patch 作为组件创建和销毁的事件通知。
    /// </summary>
    public class TeleporterRiftMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.TeleporterRift;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.TeleporterRift.Color, MarkerStyle.TeleporterRift.Sign, Strings.TeleporterRift);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(TeleporterRift))]
    public static class TeleporterRiftNotifier
    {
        [HarmonyPatch(nameof(TeleporterRift.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(TeleporterRift __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(TeleporterRift.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(TeleporterRift __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
