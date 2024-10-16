using HarmonyLib;
using System;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers;

public class Patcher
{
    public static void PatchAll()
    {
        try
        {
            var harmony = new Harmony("KingdomMod.OverlayMap.Patcher");
            harmony.PatchAll();
        }
        catch (Exception ex)
        {
            LogError($"[Patcher] => {ex}");
        }
    }
}