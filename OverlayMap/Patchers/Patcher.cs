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

            foreach (var patchedMethod in HarmonyInst.GetPatchedMethods())
            {
                LogDebug($"Patched method: {patchedMethod.DeclaringType?.FullName}.{patchedMethod.Name}");
            }
        }
        catch (Exception ex)
        {
            LogError($"[Patcher] => {ex}");
        }
    }
}