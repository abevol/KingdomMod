using HarmonyLib;
using System;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers;

public class Patcher
{
    public static Harmony HarmonyInst;

    public static void PatchAll()
    {
        try
        {
            HarmonyInst = new Harmony("KingdomMod.OverlayMap.Patcher");
            HarmonyInst.PatchAll();
        }
        catch (Exception ex)
        {
            LogError($"[Patcher] => {ex}");
        }
    }
}