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

    // 对象状态枚举
    private enum ObjectState
    {
        Created,    // 仅创建
        Destroyed,  // 仅销毁
        Canceled    // 创建后又销毁，取消处理
    }

    // 缓存条目
    private class ObjectCacheEntry
    {
        public ObjectState State { get; set; }
    }

    // 单一缓存池
    private static Dictionary<Object, ObjectCacheEntry> _objectCache = new Dictionary<Object, ObjectCacheEntry>();
    
    // 批处理间隔（秒）
    private static float _batchProcessInterval = 0.1f;
    
    // 协程引用
    private static Coroutine _processingCoroutine = null;

    private static void HandleObjectInstantiate(Object go)
    {
        if (_objectCache.TryGetValue(go, out var entry))
        {
            // 对象已在缓存中
            if (entry.State == ObjectState.Destroyed)
            {
                // 之前被标记为销毁，现在又创建了，取消处理
                entry.State = ObjectState.Canceled;
            }
        }
        else
        {
            // 新对象，添加到缓存
            _objectCache[go] = new ObjectCacheEntry
            {
                State = ObjectState.Created
            };

            // LogDebug($"Object created: {go.name}");
        }
    }

    private static void HandleObjectDestroy(Object go)
    {
        if (_objectCache.TryGetValue(go, out var entry))
        {
            // 对象已在缓存中
            if (entry.State == ObjectState.Created)
            {
                // 在同一批次内创建后又销毁，标记为取消
                entry.State = ObjectState.Canceled;
            }
        }
        else
        {
            // 新对象，添加到缓存
            _objectCache[go] = new ObjectCacheEntry
            {
                State = ObjectState.Destroyed
            };
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

            if (_objectCache.Count > 0)
            {
                LogDebug($"ProcessCachedObjects: {_objectCache.Count} objects to process");
                var toProcess = new List<KeyValuePair<Object, ObjectCacheEntry>>(_objectCache);
                _objectCache.Clear();
                
                foreach (var kvp in toProcess)
                {
                    var obj = kvp.Key;
                    var entry = kvp.Value;
                    
                    // 跳过 null 对象和已取消的对象
                    if (obj == null || entry.State == ObjectState.Canceled)
                        continue;
                    
                    if (entry.State == ObjectState.Created)
                    {
                        // LogDebug($"Process cached object created: {obj.name}");

                        var go = obj.TryCast<GameObject>();
                        if (go != null)
                        {
                            var allComponents = go.GetComponentsInChildren<Component>(true);
                            foreach (var c in allComponents)
                            {
                                OnComponentCreated?.Invoke(c);
                            }

                            continue;
                        }

                        var comp = obj.TryCast<Component>();
                        if (comp != null)
                        {
                            LogDebug($"Process cached Component created: {comp.name}, Pointer: {comp.Pointer:X}");
                            OnComponentCreated?.Invoke(comp);
                        }
                    }
                    else if (entry.State == ObjectState.Destroyed)
                    {
                        // LogDebug($"Process cached object destroyed: {obj.name}");

                        var go = obj.TryCast<GameObject>();
                        if (go != null)
                        {
                            var allComponents = go.GetComponentsInChildren<Component>(true);
                            foreach (var c in allComponents)
                            {
                                OnComponentDestroyed?.Invoke(c);
                            }

                            continue;
                        }
                        var comp = obj.TryCast<Component>();
                        if (comp != null)
                        {
                            LogDebug($"Process cached Component destroyed: {comp.name}, Pointer: {comp.Pointer:X}");
                            OnComponentDestroyed?.Invoke(comp);
                        }
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
            _objectCache.Clear();
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