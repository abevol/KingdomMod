using HarmonyLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    [HarmonyPatch(typeof(Scaffolding))]
    public static class ScaffoldingNotifier
    {
        [HarmonyPatch(nameof(Scaffolding.Awake))]
        [HarmonyPostfix]
        public static void Awake(Scaffolding __instance)
        {
            LogGameObject(__instance.gameObject);
            ForEachTopMapView(view => view.OnComponentCreated(__instance, NotifierType.Scaffolding));
        }

        [HarmonyPatch(nameof(Scaffolding.CompleteAndRemove))]
        [HarmonyPrefix]
        public static void CompleteAndRemove(Scaffolding __instance)
        {
            LogGameObject(__instance.gameObject);
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
