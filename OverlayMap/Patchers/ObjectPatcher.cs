using HarmonyLib;
using UnityEngine;
using KingdomMod.OverlayMap;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers;

public class ObjectPatcher
{
    public delegate void ComponentEventHandler(Component component);
    public static event ComponentEventHandler OnComponentCreated;
    public static event ComponentEventHandler OnComponentDestroyed;

    // [HarmonyPatch(typeof(Pool), nameof(Pool.FastSpawn), new[] { typeof(Vector3), typeof(Quaternion), typeof(Transform), typeof(short), typeof(bool) })]
    // public class FastSpawnPatcher
    // {
    //     public static void Postfix(GameObject __result)
    //     {
    //         if (__result != null)
    //         {
    //             // LogDebug($"FastSpawn: {__result.name}");
    //             var components = __result.GetComponentsInChildren<Component>(true);
    //             foreach (var c in components)
    //             {
    //                 OnComponentCreated?.Invoke(c);
    //             }
    //         }
    //     }
    // }
    //
    // [HarmonyPatch(typeof(Pool), nameof(Pool.FastDespawn), new[] { typeof(GameObject), typeof(float), typeof(bool) })]
    // public class FastDespawnPatcher
    // {
    //     public static void Prefix(GameObject clone, float delay)
    //     {
    //         // LogDebug($"FastDespawn: {clone?.name}, delay: {delay}");
    //         if (delay <= 0 && clone != null)
    //         {
    //             var components = clone.GetComponentsInChildren<Component>(true);
    //             foreach (var c in components)
    //             {
    //                 OnComponentDestroyed?.Invoke(c);
    //             }
    //         }
    //     }
    // }
}