using HarmonyLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers
{
    public class GamePatcher
    {
        [HarmonyPatch(typeof(Game), nameof(Game.Init))]
        public class Init
        {
            public static void Postfix(Game __instance)
            {
                LogMessage("GamePatcher.Init");
                OverlayMapHolder.Instance.OnGameInit(__instance);
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.P2StateChanged))]
        public class P2StateChanged
        {
            public static void Postfix(Game __instance, bool joined)
            {
                LogMessage($"GamePatcher.P2StateChanged, joined: {joined}");
                OverlayMapHolder.Instance.OnP2StateChanged(__instance, joined);
            }
        }
    }
}