using HarmonyLib;
using KingdomMod.OverlayMap.Gui.TopMap;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers;

public class NetworkBigBossPatcher
{
    [HarmonyPatch(typeof(NetworkBigBoss), nameof(NetworkBigBoss.Client_OnCaughtUp))]
    public class Client_OnCaughtUpPatcher
    {
        public static void Postfix()
        {
            LogMessage("NetworkBigBoss.Client_OnCaughtUpPatcher");
            OverlayMapHolder.Instance.OnGameStart();
            OverlayMapHolder.Instance.PlayerOverlays.P1.TopMapView.OnGameStart();
            OverlayMapHolder.Instance.PlayerOverlays.P2.TopMapView.OnGameStart();
        }
    }
}