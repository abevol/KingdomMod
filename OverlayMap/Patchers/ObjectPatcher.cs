using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using KingdomMod.Shared;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers;

public class ObjectPatcher
{
    public delegate void GameObjectEventHandler(GameObject gameObject, HashSet<SourceFlag> sources);
    public static event GameObjectEventHandler OnGameObjectCreated;
    public static event GameObjectEventHandler OnGameObjectDestroyed;

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
        Create10,
        Destroy1,
        Destroy2,
        Destroy3,
        Destroy4,
        Destroy5,
        Destroy6,
        Destroy7,
        Destroy8,
        Destroy9,
        Destroy10
    }

    private static void HandleGameObjectInstantiate(GameObject go, HashSet<SourceFlag> sources)
    {
        OnGameObjectCreated?.Invoke(go, sources);
    }

    private static void HandleGameObjectDestroy(GameObject go, HashSet<SourceFlag> sources)
    {
        OnGameObjectDestroyed?.Invoke(go, sources);
    }

    private static void HandleInstantiate(Object __result, HashSet<SourceFlag> sources)
    {
        // LogDebug($"Object.Instantiate: {__result.GetType()}");
        if (__result is GameObject go)
        {
            HandleInstantiate(go, sources);
        }
    }

    private static void HandleInstantiate(GameObject __result, HashSet<SourceFlag> sources)
    {
        // LogDebug($"Object.Instantiate: {__result.GetType()}");
        sources.Add(SourceFlag.Create6);
        HandleGameObjectInstantiate(__result, sources);
    }

    private static void HandleDestroy(Object obj, HashSet<SourceFlag> sources)
    {
        // LogDebug($"Object.Destroy: {obj.GetType()}");

        var go = obj.TryCast<GameObject>();

        if (go != null)
        {
            sources.Add(SourceFlag.Destroy3);
            HandleGameObjectDestroy(go, sources);
        }
        else if (obj is Component comp)
        {
            LogDebug($"Object.Destroy, Component: {comp.GetType()}");

            sources.Add(SourceFlag.Destroy4);
            // HandleComponentDestroy(comp, sources);
        }
    }

    public static void GenericInstantiatePostfix(UnityEngine.Object __result)
    {
        // 1. 安全检查是否为空
        if (__result == null) return;

        // 2. 尝试转换为 GameObject
        // 在 BepInEx IL2CPP 中，必须使用 TryCast<T>() 而不是 C# 的 "as" 关键字
        // 因为这是跨越 Native/Managed 边界的对象
        var go = __result.TryCast<GameObject>();

        if (go != null)
        {
            // 这里才是真正捕获到 GameObject 的地方
            LogDebug($"Captured GameObject: {go.name}");

            HandleGameObjectInstantiate(go, [SourceFlag.Create1]);
        }
    }

    public static void PatchInstantiateGenerics()
    {
        // 1. 获取 UnityEngine.Object 类中名为 Instantiate 的所有方法
        var methods = typeof(UnityEngine.Object).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Instantiate" && m.IsGenericMethodDefinition);

        // 2. 针对你想要拦截的 T 类型（通常是 UnityEngine.Object），手动生成具体方法并 Hook
        foreach (var method in methods)
        {
            // 这里将泛型 T 实例化为 UnityEngine.Object
            var genericMethod = method.MakeGenericMethod(typeof(UnityEngine.Object));

            // 获取参数类型，以便匹配特定的重载
            var parameters = genericMethod.GetParameters();

            // 可以在这里通过 parameters.Length 或类型来过滤具体的重载
            // 比如为了保险起见，把所有泛型重载全 Hook 了：
            Patcher.HarmonyInst.Patch(
                original: genericMethod,
                postfix: new HarmonyMethod(typeof(ObjectPatcher), nameof(ObjectPatcher.GenericInstantiatePostfix))
            );

            LogDebug($"Hooked Instantiate<UnityEngine.Object> with {parameters.Length} params");
        }
    }

    // [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), new[] { typeof(Object) })]
    // public class InstantiatePatcher
    // {
    //     public static void Postfix(Object __result)
    //     {
    //         HandleInstantiate(__result, [SourceFlag.Create1]);
    //     }
    // }

    // [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), new[] { typeof(Object), typeof(Scene) })]
    // public class InstantiatePatcher1
    // {
    //     public static void Postfix(Object __result)
    //     {
    //         HandleInstantiate(__result, [SourceFlag.Create2]);
    //     }
    // }

    // [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), new[] { typeof(Object), typeof(Transform), typeof(bool) })]
    // public class InstantiatePatcher2
    // {
    //     public static void Postfix(Object __result)
    //     {
    //         HandleInstantiate(__result, [SourceFlag.Create3]);
    //     }
    // }

    // [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), new[] { typeof(Object), typeof(Vector3), typeof(Quaternion) })]
    // public class InstantiatePatcher3
    // {
    //     public static void Postfix(Object __result)
    //     {
    //         HandleInstantiate(__result, [SourceFlag.Create4]);
    //     }
    // }

    // [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), new[] { typeof(Object), typeof(Vector3), typeof(Quaternion), typeof(Transform) })]
    // public class InstantiatePatcher4
    // {
    //     public static void Postfix(Object __result)
    //     {
    //         HandleInstantiate(__result, [SourceFlag.Create5]);
    //     }
    // }

    // [HarmonyPatch(typeof(Object), nameof(Object.Destroy), new[] { typeof(Object), typeof(float) })]
    // public class DestroyPatcher
    // {
    //     public static void Prefix(Object obj)
    //     {
    //         HandleDestroy(obj, [SourceFlag.Destroy1]);
    //     }
    // }

    // [HarmonyPatch(typeof(Object), nameof(Object.DestroyImmediate), new[] { typeof(Object), typeof(bool) })]
    // public class DestroyImmediatePatcher
    // {
    //     public static void Prefix(Object obj)
    //     {
    //         HandleDestroy(obj, [SourceFlag.Destroy2]);
    //     }
    // }
}