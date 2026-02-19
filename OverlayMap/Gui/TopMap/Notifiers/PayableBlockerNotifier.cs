using HarmonyLib;
using Il2CppInterop.Runtime;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    [HarmonyPatch(typeof(PayableBlocker))]
    public static class PayableBlockerNotifier
    {
        private static readonly Il2CppSystem.Type[] PuzzleTypes =
        [
            Il2CppType.Of<CerberusPuzzleController>(),
            Il2CppType.Of<ChariotPuzzleController>(),
            Il2CppType.Of<HeimdallPuzzleController>(),
            Il2CppType.Of<HelPuzzleController>(),
            Il2CppType.Of<ThorPuzzleController>()
        ];

        [HarmonyPatch(nameof(PayableBlocker.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(PayableBlocker __instance)
        {
            Component target = null;
            foreach (var type in PuzzleTypes)
            {
                target = __instance.GetComponent(type);
                if (target != null) break;
            }
            target ??= __instance;
            ForEachTopMapView(view => view.OnComponentCreated(target, NotifierType.PayableBlocker));
        }

        [HarmonyPatch(nameof(PayableBlocker.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(PayableBlocker __instance)
        {
            Component target = null;
            foreach (var type in PuzzleTypes)
            {
                target = __instance.GetComponent(type);
                if (target != null) break;
            }
            target ??= __instance;
            ForEachTopMapView(view => view.OnComponentDestroyed(target));
        }
    }
}
