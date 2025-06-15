using System;
#if IL2CPP
using Il2CppSystem.Collections.Generic;
#else
using System.Collections.Generic;
#endif
using HarmonyLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers;

public class LevelLayoutPatcher
{
    [HarmonyPatch(typeof(LevelLayout), "Generate")]
    public class ConstructorPatcher
    {
        public static void Postfix(LevelLayout __instance)
        {
            LogDebug($"LevelLayout.Constructor, TotalWidth: {__instance.TotalWidth()}, " +
                     $"minLevelWidth: {Managers.Inst.game.currentLevelConfig.minLevelWidth}, " +
                     $"levelWidth: {Managers.Inst.level.levelWidth}, " +
                     $"GroundCollider: {Managers.Inst.level.GroundCollider.size.x}, " +
                     $"TotalWidth: {TotalWidth()}");
        }

        public static int TotalWidth()
        {
            var blocks = Managers.Inst.level.GetLevelBlocks();
            int num = 0;
            foreach (var item in blocks)
            {
                num += item.GetWidth();
            }

            return num;
        }
    }
}