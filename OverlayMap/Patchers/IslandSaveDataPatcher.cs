using HarmonyLib;
using KingdomMod.OverlayMap.Gui.TopMap;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers;

public class IslandSaveDataPatcher
{
    [HarmonyPatch(typeof(IslandSaveData), nameof(IslandSaveData.Save), new System.Type[] { typeof(int), typeof(int), typeof(int) })]
    public class Save
    {
        public static void Postfix()
        {
            LogDebug("IslandSaveDataPatcher.Save");
            OverlayMapHolder.Instance.OnGameSaved();
        }
    }
}