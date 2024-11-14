using HarmonyLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers
{
    public class ProgramDirectorPatcher
    {
        [HarmonyPatch(typeof(ProgramDirector), nameof(ProgramDirector.Run))]
        public class Init
        {
            public static void Postfix()
            {
                LogTrace($"ProgramDirectorPatcher.Run, Application.isPlaying: {Application.isPlaying}");
                OverlayMapHolder.Instance.OnProgramDirectorRun();
            }
        }
    }
}