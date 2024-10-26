using System;
using System.Collections.Generic;
using HarmonyLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers;

public class LevelLayoutPatcher
{
    [HarmonyPatch(typeof(LevelLayout), MethodType.Constructor, new Type[]
    {
        typeof(List<LevelBlock>),
        typeof(LevelConfig),
        typeof(Dictionary<LevelBlockGroup, SharedBlockCampaignData>),
        typeof(Dictionary<LevelBlockGroup, IntRange>),
        typeof(int)
    })]
    public class ConstructorPatcher
    {
        public static void Postfix(LevelLayout __instance)
        {

            LogMessage($"LevelLayout.Constructor, TotalWidth: {__instance.TotalWidth()}, " +
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