using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Patchers;

public class ObjectPatcher
{
    public delegate void ComponentEventHandler(Component component);
    public static event ComponentEventHandler OnComponentCreated;
    public static event ComponentEventHandler OnComponentDestroyed;

    public static bool ProcessCachedObjectsRunning = false;

    // 创建对象缓存池（用于批处理）
    private static HashSet<Object> _createdCache = new HashSet<Object>();
    
    // 批处理间隔（秒）
    private static float _batchProcessInterval = 0.1f;
    
    // 协程引用
    private static Coroutine _processingCoroutine = null;

    private static void HandleObjectInstantiate(Object go)
    {
        if (go == null) return;
        _createdCache.Add(go);
        LogDebug($"HandleObjectInstantiate: {go.name}, Pointer: {go.Pointer:X}");
    }

    private static void HandleObjectDestroy(Object obj)
    {
        if (obj == null) return;

        // 如果该对象还在创建缓存中（尚未处理 OnComponentCreated），则将其移除，取消后续的创建处理
        if (_createdCache.Remove(obj))
        {
            return;
        }

        // 立即处理销毁事件
        var go = obj.TryCast<GameObject>();
        if (go != null)
        {
            // 获取该 GameObject 下的所有组件（包括子对象）
            var allComponents = go.GetComponentsInChildren<Component>(true);
            foreach (var c in allComponents)
            {
                if (c != null)
                {
                    LogDebug($"Process cached Component destroyed: {c.name}, Pointer: {c.Pointer:X}");
                    OnComponentDestroyed?.Invoke(c);
                }
            }
        }
        else
        {
            var comp = obj.TryCast<Component>();
            if (comp != null)
            {
                LogDebug($"Process cached Component destroyed 2: {comp.name}, Pointer: {comp.Pointer:X}");
                OnComponentDestroyed?.Invoke(comp);
            }
        }
    }

    public static void GenericInstantiatePostfix(UnityEngine.Object __result)
    {
        // 安全检查是否为空
        if (__result == null) return;

        // LogDebug($"Captured Object: {__result.name}");

        HandleObjectInstantiate(__result);
    }

    public static void PatchInstantiateGenerics()
    {
        var harmony = Patcher.HarmonyInst;

        // 1. Hook generic Instantiate methods
        var genericMethods = typeof(UnityEngine.Object).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Instantiate" && m.IsGenericMethodDefinition);

        foreach (var method in genericMethods)
        {
            var genericMethod = method.MakeGenericMethod(typeof(UnityEngine.Object));
            Patcher.HarmonyInst.Patch(
                original: genericMethod,
                postfix: new HarmonyMethod(typeof(ObjectPatcher), nameof(ObjectPatcher.GenericInstantiatePostfix))
            );
            LogDebug($"Hooked generic Instantiate<UnityEngine.Object> with {method.GetParameters().Length} params");
        }

        // 2. Hook non-generic Instantiate methods
        var nonGenericMethods = typeof(UnityEngine.Object).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Instantiate" && !m.IsGenericMethodDefinition);

        foreach (var method in nonGenericMethods)
        {
            // Filter out methods that might be obscure or obsolete if needed, but usually hooking all public static Instantiate is safe
            Patcher.HarmonyInst.Patch(
                original: method,
                postfix: new HarmonyMethod(typeof(ObjectPatcher), nameof(ObjectPatcher.GenericInstantiatePostfix))
            );
            LogDebug($"Hooked non-generic Instantiate with {method.GetParameters().Length} params");
        }
    }

    // 批处理协程
    private static IEnumerator ProcessCachedObjects()
    {
        LogDebug($"ProcessCachedObjects started");
        while (true)
        {
            ProcessCachedObjectsRunning = true;
            yield return new WaitForSeconds(_batchProcessInterval);

            if (_createdCache.Count > 0)
            {
                LogDebug($"ProcessCachedObjects: {_createdCache.Count} objects to process");
                var toProcess = _createdCache.ToList();
                _createdCache.Clear();
                
                // 使用 Dictionary 按 Pointer 去重组件
                var createdComponents = new Dictionary<System.IntPtr, Component>();
                
                foreach (var obj in toProcess)
                {
                    // 跳过 null 对象
                    if (obj == null)
                    {
                        LogDebug("ProcessCachedObjects: obj is null");
                        continue;
                    }
                    
                    var go = obj.TryCast<GameObject>();
                    if (go != null)
                    {
                        var allComponents = go.GetComponentsInChildren<Component>(true);
                        if (allComponents == null || allComponents.Count == 0)
                        {
                            LogDebug($"ProcessCachedObjects: GameObject {go.name} has no components");
                        }
                        else
                        {
                            foreach (var c in allComponents)
                            {
                                if (c != null)
                                    createdComponents.TryAdd(c.Pointer, c);
                            }
                        }
                    }
                    else
                    {
                        var comp = obj.TryCast<Component>();
                        if (comp != null)
                        {
                            createdComponents.TryAdd(comp.Pointer, comp);
                        }
                        else
                        {
                             LogDebug($"ProcessCachedObjects: Object {obj.name} ({obj.GetIl2CppType().Name}) is neither GameObject nor Component");
                        }
                    }
                }
                
                // 批量调用事件处理器
                if (createdComponents.Count > 0)
                {
                    LogDebug($"Invoking OnComponentCreated for {createdComponents.Count} components");
                    foreach (var comp in createdComponents.Values)
                    {
                        LogDebug($"Component created: {comp.name}, Pointer: {comp.Pointer:X}");
                        OnComponentCreated?.Invoke(comp);
                    }
                }
                else
                {
                     LogDebug("ProcessCachedObjects: createdComponents is empty after processing");
                }
            }

            ProcessCachedObjectsRunning = false;
        }
    }

    /// <summary>
    /// 启动对象池批处理协程
    /// </summary>
    /// <param name="monoBehaviour">用于启动协程的 MonoBehaviour</param>
    public static void StartProcessing(MonoBehaviour monoBehaviour)
    {
        if (_processingCoroutine == null && monoBehaviour != null)
        {
            _processingCoroutine = monoBehaviour.StartCoroutine(ProcessCachedObjects().WrapToIl2Cpp());
            LogDebug("Object cache processing started");
        }
    }

    /// <summary>
    /// 停止对象池批处理协程
    /// </summary>
    /// <param name="monoBehaviour">用于停止协程的 MonoBehaviour</param>
    public static void StopProcessing(MonoBehaviour monoBehaviour)
    {
        if (_processingCoroutine != null && monoBehaviour != null)
        {
            monoBehaviour.StopCoroutine(_processingCoroutine);
            _processingCoroutine = null;
            _createdCache.Clear();
            LogDebug("Object cache processing stopped");
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.Destroy), new[] { typeof(Object), typeof(float) })]
    public class DestroyPatcher
    {
        public static void Prefix(Object obj)
        {
            HandleObjectDestroy(obj);
        }
    }

    [HarmonyPatch(typeof(Object), nameof(Object.DestroyImmediate), new[] { typeof(Object), typeof(bool) })]
    public class DestroyImmediatePatcher
    {
        public static void Prefix(Object obj)
        {
            HandleObjectDestroy(obj);
        }
    }
}