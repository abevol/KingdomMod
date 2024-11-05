using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers;

public class ObjectPatcher
{
    public delegate void ComponentEventHandler(Component component, HashSet<SourceFlag> sources);
    public static event ComponentEventHandler OnComponentCreated;
    public static event ComponentEventHandler OnComponentDestroyed;

    public enum SourceFlag
    {
        Create1,
        Create2,
        Create3,
        Create4,
        Create5,
        Create6,
        Create7,
        Create8,
        Create9,
        Destroy1,
        Destroy2,
        Destroy3,
        Destroy4,
        Destroy5,
        Destroy6,
        Destroy7,
        Destroy8,
        Destroy9
    }

    private static void HandleComponentInstantiate(Component comp, HashSet<SourceFlag> sources)
    {
        OnComponentCreated?.Invoke(comp, sources);
    }

    private static void HandleComponentDestroy(Component comp, HashSet<SourceFlag> sources)
    {
        OnComponentDestroyed?.Invoke(comp, sources);
    }

    private static void HandleInstantiate(Object __result, HashSet<SourceFlag> sources)
    {
        // LogMessage($"Object.Instantiate: {__result.GetType()}");
        if (__result is GameObject go)
        {
            sources.Add(SourceFlag.Create6);
            var comps = go.GetComponentsInChildren<Component>(true);
            foreach (var comp in comps)
            {
                HandleComponentInstantiate(comp, sources);
            }
        }
    }

    private static void HandleDestroy(Object obj, HashSet<SourceFlag> sources)
    {
        // LogMessage($"Object.Destroy: {obj.GetType()}");

        if (obj is GameObject go)
        {
            sources.Add(SourceFlag.Destroy3);
            var comps = go.GetComponentsInChildren<Component>(true);
            foreach (var comp in comps)
            {
                HandleComponentDestroy(comp, sources);
            }
        }
        else if (obj is Component comp)
        {
            // LogMessage($"Object.Destroy: {comp.GetType()}");

            sources.Add(SourceFlag.Destroy4);
            HandleComponentDestroy(comp, sources);
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), new[] { typeof(Object) })]
    public class InstantiatePatcher
    {
        public static void Postfix(Object __result)
        {
            HandleInstantiate(__result, [SourceFlag.Create1]);
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), new[] { typeof(Object), typeof(Scene) })]
    public class InstantiatePatcher1
    {
        public static void Postfix(Object __result)
        {
            HandleInstantiate(__result, [SourceFlag.Create2]);
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), new[] { typeof(Object), typeof(Transform), typeof(bool) })]
    public class InstantiatePatcher2
    {
        public static void Postfix(Object __result)
        {
            HandleInstantiate(__result, [SourceFlag.Create3]);
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), new[] { typeof(Object), typeof(Vector3), typeof(Quaternion) })]
    public class InstantiatePatcher3
    {
        public static void Postfix(Object __result)
        {
            HandleInstantiate(__result, [SourceFlag.Create4]);
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), new[] { typeof(Object), typeof(Vector3), typeof(Quaternion), typeof(Transform) })]
    public class InstantiatePatcher4
    {
        public static void Postfix(Object __result)
        {
            HandleInstantiate(__result, [SourceFlag.Create5]);
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.Destroy), new[] { typeof(Object), typeof(float) })]
    public class DestroyPatcher
    {
        public static void Prefix(Object obj)
        {
            HandleDestroy(obj, [SourceFlag.Destroy1]);
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.DestroyImmediate), new[] { typeof(Object), typeof(bool) })]
    public class DestroyImmediatePatcher
    {
        public static void Prefix(Object obj)
        {
            HandleDestroy(obj, [SourceFlag.Destroy2]);
        }
    }
}