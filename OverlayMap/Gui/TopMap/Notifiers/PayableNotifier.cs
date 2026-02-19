using HarmonyLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    [HarmonyPatch(typeof(Payable))]
    public static class PayableNotifier
    {
        [HarmonyPatch(nameof(Payable.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(Payable __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance, NotifierType.Payable));
        }

        [HarmonyPatch(nameof(Payable.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Payable __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
