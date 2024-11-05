using HarmonyLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers
{
    public class PlayerPatcher
    {
        [HarmonyPatch(typeof(Player), nameof(Player.SetupAsPlayer))]
        public class SetupAsPlayer
        {
            public static void Postfix(Player __instance, int id)
            {
                LogMessage($"PlayerPatcher.SetupAsPlayer, id: {id}");
                OverlayMapHolder.Instance.PlayerOverlays.P1?.TopMapView?.OnSetupPlayerId(__instance, id);
                OverlayMapHolder.Instance.PlayerOverlays.P2?.TopMapView?.OnSetupPlayerId(__instance, id);
            }
        }
    }
}