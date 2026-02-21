using HarmonyLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    [HarmonyPatch(typeof(PlayerCargo))]
    public static class PlayerCargoNotifier
    {
        [HarmonyPatch(nameof(PlayerCargo.Awake))]
        [HarmonyPostfix]
        public static void AwakePostfix(PlayerCargo __instance)
        {
            LogGameObject(__instance.gameObject);
            __instance.OnPlayerCargoPickedUp += (System.Action)(() => OnPlayerCargoPickedUp(__instance));
            ForEachTopMapView(view => view.OnComponentCreated(__instance, NotifierType.PlayerCargo));
        }

        public static void OnPlayerCargoPickedUp(PlayerCargo __instance)
        {
            LogGameObject(__instance.gameObject);
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
