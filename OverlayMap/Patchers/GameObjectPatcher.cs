using System;
using HarmonyLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers;

public class GameObjectPatcher
{
    [HarmonyPatch(typeof(GameObject), nameof(GameObject.AddComponent), new Type[] { typeof(Type) })]
    public class AddComponentPatcher
    {
        public static void Postfix(Component __result)
        {
            // LogMessage($"GameObject.AddComponent: {__result.GetType()}");
            if (__result.GetType() == typeof(Castle))
            {
                LogWarning("GameObject.AddComponent: Castle component added");
            }
        }
    }

}