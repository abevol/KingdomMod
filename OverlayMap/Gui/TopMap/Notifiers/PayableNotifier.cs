using HarmonyLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    [HarmonyPatch(typeof(Payable))]
    public static class PayableNotifier
    {
        [HarmonyPatch(nameof(Payable.Start))]
        [HarmonyPostfix]
        public static void Start(Payable __instance)
        {
            LogGameObject(__instance.gameObject);
            ForEachTopMapView(view => view.OnComponentCreated(__instance, NotifierType.Payable));
        }

        [HarmonyPatch(nameof(Payable.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Payable __instance)
        {
            LogGameObject(__instance.gameObject);
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
