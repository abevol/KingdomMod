using HarmonyLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    [HarmonyPatch(typeof(WorkableBuilding))]
    public static class WorkableBuildingNotifier
    {
        [HarmonyPatch(nameof(WorkableBuilding.Start))]
        [HarmonyPostfix]
        public static void StartPostfix(WorkableBuilding __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance, NotifierType.WorkableBuilding));
        }

        [HarmonyPatch(nameof(WorkableBuilding.OnDestroy))]
        [HarmonyPrefix]
        public static void OnDestroyPrefix(WorkableBuilding __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
