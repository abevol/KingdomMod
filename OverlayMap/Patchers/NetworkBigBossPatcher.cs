using HarmonyLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers;

public class NetworkBigBossPatcher
{
    [HarmonyPatch(typeof(NetworkBigBoss), nameof(NetworkBigBoss.Client_OnCaughtUp))]
    public class DebugIsDebugBuildPatcher
    {
        public static void Postfix()
        {
            LogMessage("NetworkBigBoss.Client_OnCaughtUp.");
            OverlayMapHolder.Instance.OnGameStart();
        }
    }
}