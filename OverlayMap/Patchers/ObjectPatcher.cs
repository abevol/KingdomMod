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
                    OnComponentDestroyed?.Invoke(c);
                }
            }
        }
        else
        {
            var comp = obj.TryCast<Component>();
            if (comp != null)
            {
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

    // 批处理协程
    private static IEnumerator ProcessCachedObjects()
    {
        LogDebug($"ProcessCachedObjects started");
        while (true)
        {
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
                        continue;
                    
                    var go = obj.TryCast<GameObject>();
                    if (go != null)
                    {
                        var allComponents = go.GetComponentsInChildren<Component>(true);
                        foreach (var c in allComponents)
                        {
                            if (c != null)
                                createdComponents.TryAdd(c.Pointer, c);
                        }
                    }
                    else
                    {
                        var comp = obj.TryCast<Component>();
                        if (comp != null)
                            createdComponents.TryAdd(comp.Pointer, comp);
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
            }
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