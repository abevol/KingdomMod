using HarmonyLib;
#if IL2CPP
using Il2CppInterop.Runtime;
#endif
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    [HarmonyPatch(typeof(PayableBlocker))]
    public static class PayableBlockerNotifier
    {
        [HarmonyPatch(nameof(PayableBlocker.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(PayableBlocker __instance)
        {
            LogGameObject(__instance.gameObject);
            ForEachTopMapView(view => view.OnComponentCreated(__instance, NotifierType.PayableBlocker));
        }

        [HarmonyPatch(nameof(PayableBlocker.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(PayableBlocker __instance)
        {
            LogGameObject(__instance.gameObject);
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
